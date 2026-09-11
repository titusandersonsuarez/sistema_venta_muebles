using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;

namespace NogalApi.Services;

public interface IProduct3dGenerationQueue
{
    ValueTask EnqueueAsync(int productId, CancellationToken cancellationToken = default);
}

public sealed class Product3dGenerationQueue : BackgroundService, IProduct3dGenerationQueue
{
    private readonly Channel<int> _jobs = Channel.CreateUnbounded<int>();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Product3dGenerationQueue> _logger;

    public Product3dGenerationQueue(IServiceScopeFactory scopeFactory, ILogger<Product3dGenerationQueue> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public ValueTask EnqueueAsync(int productId, CancellationToken cancellationToken = default) =>
        _jobs.Writer.WriteAsync(productId, cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ReencolarPendientesAsync(stoppingToken);

        await foreach (var productId in _jobs.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var generator = scope.ServiceProvider.GetRequiredService<IProduct3dGenerationService>();
                var producto = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, stoppingToken);
                if (producto is null) continue;

                producto.Modelo3dEstado = "Procesando";
                producto.Modelo3dError = null;
                await context.SaveChangesAsync(stoppingToken);

                var resultado = await generator.GenerarAsync(producto, stoppingToken);
                producto.Modelo3dUrl = resultado.Modelo3dUrl;
                producto.ModeloUsdzUrl = resultado.ModeloUsdzUrl;
                producto.Modelo3dEstado = "Disponible";
                producto.Modelo3dError = null;
                producto.FechaActualizacion = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error generando el modelo 3D del producto {ProductId}", productId);
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var producto = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, stoppingToken);
                if (producto is null) continue;
                producto.Modelo3dEstado = "Error";
                producto.Modelo3dError = ex.Message;
                producto.FechaActualizacion = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }

    private async Task ReencolarPendientesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pendientes = await context.Products
            .Where(p => p.Modelo3dEstado == "Pendiente")
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var productId in pendientes)
        {
            await _jobs.Writer.WriteAsync(productId, cancellationToken);
        }
    }
}