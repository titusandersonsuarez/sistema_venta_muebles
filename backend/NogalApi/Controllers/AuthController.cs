using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Auth;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    /// <summary>
    /// Nombre de la cookie que transporta el JWT. HttpOnly para que
    /// JavaScript no pueda leerla (mitiga XSS token theft).
    /// </summary>
    public const string AuthCookieName = "nogal_auth";

    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { mensaje = "Usuario y contraseña son obligatorios." });
        }

        var resultado = await _authService.LoginAsync(request.NombreUsuario.Trim(), request.Password);

        if (resultado == null)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        Response.Cookies.Append(AuthCookieName, resultado.Token, ConstruirCookieOptions(resultado.ExpiraEn));

        return Ok(new LoginResponse
        {
            ExpiraEn = resultado.ExpiraEn,
            Usuario = resultado.Usuario
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        // Devuelve la misma forma que LoginResponse.Usuario para que el
        // frontend no tenga que reconciliar dos shapes distintos.
        var idRaw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(new UsuarioResumen
        {
            Id = int.TryParse(idRaw, out var id) ? id : 0,
            NombreUsuario = User.Identity?.Name ?? string.Empty,
            Nombre = User.FindFirstValue("nombre") ?? string.Empty,
            Rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
        });
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        // Sin [Authorize]: si la cookie ya está expirada o corrupta el
        // frontend igual necesita poder llamar aquí para limpiarla y no
        // quedar en un bucle de 401.
        Response.Cookies.Delete(AuthCookieName, ConstruirCookieOptions(null));
        return NoContent();
    }

    private CookieOptions ConstruirCookieOptions(DateTime? expiraEn)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            // En dev servimos HTTP puro; en prod debe estar detrás de HTTPS.
            Secure = !_env.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = expiraEn
        };
    }
}
