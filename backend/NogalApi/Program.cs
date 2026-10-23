using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NogalApi.Data;
using NogalApi.Options;
using NogalApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<ImageStorageOptions>(builder.Configuration.GetSection(ImageStorageOptions.SectionName));
builder.Services.Configure<Product3dOptions>(builder.Configuration.GetSection(Product3dOptions.SectionName));
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.Configure<WompiOptions>(builder.Configuration.GetSection(WompiOptions.SectionName));

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el token JWT devuelto por /api/auth/login (sin la palabra 'Bearer')."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
// Storage de imágenes: Local por default, Azure Blob si ImageStorage:Provider = "azure".
// Fallback silencioso a Local si Provider=azure pero falta ConnectionString.
builder.Services.AddScoped<LocalProductImageStorage>();
builder.Services.AddScoped<IProductImageStorage>(services =>
{
    var options = services.GetRequiredService<IOptions<ImageStorageOptions>>().Value;
    var provider = (options.Provider ?? "local").Trim().ToLowerInvariant();
    var logger = services.GetRequiredService<ILogger<Program>>();
    if (provider == "azure")
    {
        if (string.IsNullOrWhiteSpace(options.AzureConnectionString))
        {
            logger.LogWarning("ImageStorage:Provider=azure pero falta AzureConnectionString. Usando storage local.");
            return services.GetRequiredService<LocalProductImageStorage>();
        }
        return ActivatorUtilities.CreateInstance<AzureBlobProductImageStorage>(services);
    }
    return services.GetRequiredService<LocalProductImageStorage>();
});
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IProductionService, ProductionService>();
builder.Services.AddScoped<IGeminiAiService, GeminiAiService>();
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddSingleton<Product3dGenerationQueue>();
builder.Services.AddSingleton<IProduct3dGenerationQueue>(services => services.GetRequiredService<Product3dGenerationQueue>());
builder.Services.AddHostedService(services => services.GetRequiredService<Product3dGenerationQueue>());
builder.Services.Configure<Product3dOptions>(builder.Configuration.GetSection(Product3dOptions.SectionName));
builder.Services.AddScoped<UnavailableProduct3dGenerationService>();
builder.Services.AddScoped<MeshyProduct3dGenerationService>();
builder.Services.AddScoped<IProduct3dGenerationService, Product3dGenerationDispatcher>();
builder.Services.AddScoped<DemoPaymentService>();
builder.Services.AddScoped<WompiPaymentService>();
builder.Services.AddScoped<IPaymentService, PaymentServiceDispatcher>();
builder.Services.AddScoped<IWompiWebhookService, WompiWebhookService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");

// Fallar cerrado fuera de Development: la llave de ejemplo es pública (está
// en el repo y en docker-compose), así que con ella cualquiera puede firmar
// un JWT con rol Admin. Igual con la contraseña de ejemplo de Postgres.
if (!builder.Environment.IsDevelopment())
{
    const string LlaveJwtDeEjemplo = "CAMBIA_ESTA_LLAVE_POR_UNA_DE_AL_MENOS_32_CARACTERES_LARGA";
    if (string.IsNullOrWhiteSpace(jwtOptions.Key) ||
        jwtOptions.Key == LlaveJwtDeEjemplo ||
        Encoding.UTF8.GetByteCount(jwtOptions.Key) < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key no está configurada o es la llave de ejemplo. Define Jwt__Key con un valor aleatorio de al menos 32 bytes.");
    }

    var cadenaDb = builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    if (System.Text.RegularExpressions.Regex.IsMatch(cadenaDb, @"(?i)password\s*=\s*nogal\s*(;|$)"))
    {
        throw new InvalidOperationException(
            "La cadena de conexión usa la contraseña de ejemplo de Postgres. Define POSTGRES_PASSWORD / ConnectionStrings__DefaultConnection.");
    }
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // El token viaja en una cookie HttpOnly `nogal_auth`. El header
        // Authorization sigue funcionando por si Swagger o herramientas
        // internas quieren usarlo, pero el frontend real solo usa cookie.
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token) &&
                    context.Request.Cookies.TryGetValue(NogalApi.Controllers.AuthController.AuthCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// La API corre detrás de nginx (y en Azure, además, detrás del ingress).
// Sin esto RemoteIpAddress es la IP del proxy y todos los clientes caerían
// en el mismo bucket del rate limiter. ForwardLimit = número de proxies
// confiables delante de la API (1 = solo nginx; 2 = ingress + nginx).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = builder.Configuration.GetValue("ForwardedHeaders:ForwardLimit", 1);
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Límites por IP para endpoints anónimos: fuerza bruta de login, abuso de
// costo de Gemini en /api/chat y spam de pedidos/reseñas/contacto.
static string ClaveIp(HttpContext ctx) => ctx.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", ctx => RateLimitPartition.GetFixedWindowLimiter(ClaveIp(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("chat", ctx => RateLimitPartition.GetSlidingWindowLimiter(ClaveIp(ctx),
        _ => new SlidingWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(10), SegmentsPerWindow = 10, QueueLimit = 0 }));
    options.AddPolicy("publico-escritura", ctx => RateLimitPartition.GetFixedWindowLimiter(ClaveIp(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    options.AddPolicy("publico-lectura", ctx => RateLimitPartition.GetFixedWindowLimiter(ClaveIp(ctx),
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 60, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              // Necesario para que el navegador incluya la cookie `nogal_auth`
              // en requests cross-origin del frontend Vite (5173 → 5199).
              .AllowCredentials();
    });
});

var app = builder.Build();

// Aplica migraciones pendientes y crea el usuario administrador inicial
// si la base de datos está vacía.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seedOptions = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

    await context.Database.MigrateAsync();
    await DbSeeder.SeedAdminAsync(context, seedOptions, app.Logger, app.Environment.IsDevelopment());
    await OrderSeeder.SeedAsync(context);
    await ProductionSeeder.SeedAsync(context);
    await DbSeeder.SeedProductVariantsAsync(context);
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// En dev servimos HTTP puro; HttpsRedirection sin puerto HTTPS
// configurado rompe POSTs (respuestas 307/503 impredecibles).
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Cabeceras de seguridad para respuestas de la API y de /uploads.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    // Swagger UI (solo Development) necesita scripts propios.
    if (!context.Request.Path.StartsWithSegments("/swagger"))
    {
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; sandbox";
    }
    await next();
});
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
