using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Services;

namespace NogalApi.Tests;

/// <summary>Base de datos en memoria aislada por prueba y datos de ejemplo.</summary>
internal static class TestDb
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    public static Product AddProduct(AppDbContext db, string nombre, decimal precio, bool activo = true,
        params (string nombre, decimal ajuste, bool activo)[] variantes)
    {
        var producto = new Product
        {
            Nombre = nombre,
            Slug = nombre.ToLowerInvariant().Replace(' ', '-'),
            Categoria = "Sofás",
            Material = "Madera maciza",
            PrecioCOP = precio,
            Activo = activo
        };
        foreach (var v in variantes)
        {
            producto.Variantes.Add(new ProductVariant
            {
                Nombre = v.nombre,
                Sku = $"{producto.Slug}-{v.nombre}".ToUpperInvariant(),
                PrecioAjusteCOP = v.ajuste,
                Activo = v.activo
            });
        }
        db.Products.Add(producto);
        db.SaveChanges();
        return producto;
    }
}

/// <summary>Pasarela de pago falsa: las pruebas de pedidos no llaman a Wompi.</summary>
internal sealed class FakePaymentService : IPaymentService
{
    public Task<PagoIntencionResultado> CrearIntencionAsync(Order pedido, CancellationToken cancellationToken = default)
        => Task.FromResult(new PagoIntencionResultado("fake", "https://pago.test/" + pedido.Codigo));

    public Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default)
        => Task.FromResult<PagoEstadoResultado?>(null);
}
