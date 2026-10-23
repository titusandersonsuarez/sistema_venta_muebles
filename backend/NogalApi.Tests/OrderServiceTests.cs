using Microsoft.EntityFrameworkCore;
using NogalApi.Models.Orders;
using NogalApi.Services;

namespace NogalApi.Tests;

public class OrderServiceTests
{
    private static CreateOrderDto Pedido(params CreateOrderItemDto[] items) => new()
    {
        Cliente = "María Pérez",
        Ciudad = "Bogotá",
        Contacto = "3001234567",
        Items = items.ToList()
    };

    [Fact]
    public async Task CobraEnvioCuandoElSubtotalEsMenorA500Mil()
    {
        using var db = TestDb.Create();
        var silla = TestDb.AddProduct(db, "Silla Roble", 180_000m);
        var service = new OrderService(db, new FakePaymentService());

        var pedido = await service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = silla.Id, Cantidad = 2 }));

        Assert.Equal(30_000m, pedido.EnvioCOP);
        Assert.Equal(390_000m, pedido.Total);
        Assert.Equal("Pago pendiente", pedido.Estado);
        Assert.StartsWith("NGL-", pedido.Codigo);
    }

    [Fact]
    public async Task EnvioGratisDesde500Mil()
    {
        using var db = TestDb.Create();
        var sofa = TestDb.AddProduct(db, "Sofá Lino", 500_000m);
        var service = new OrderService(db, new FakePaymentService());

        var pedido = await service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = sofa.Id, Cantidad = 1 }));

        Assert.Equal(0m, pedido.EnvioCOP);
        Assert.Equal(500_000m, pedido.Total);
    }

    [Fact]
    public async Task ElPrecioSeCalculaEnElServidorIncluyendoLaVariante()
    {
        using var db = TestDb.Create();
        var mesa = TestDb.AddProduct(db, "Mesa Comedor", 900_000m, true, ("Nogal oscuro", 150_000m, true));
        var service = new OrderService(db, new FakePaymentService());
        var varianteId = mesa.Variantes[0].Id;

        var pedido = await service.CrearAsync(Pedido(
            new CreateOrderItemDto { ProductId = mesa.Id, ProductVariantId = varianteId, Cantidad = 1 }));

        var item = Assert.Single(pedido.Items);
        Assert.Equal(1_050_000m, item.PrecioUnitario);
        Assert.Equal("Nogal oscuro", item.VarianteNombre);
        Assert.Equal(1_050_000m, pedido.Total);
    }

    [Fact]
    public async Task RechazaProductosInactivosOVariantesDeOtroProducto()
    {
        using var db = TestDb.Create();
        var inactivo = TestDb.AddProduct(db, "Cama Vieja", 700_000m, activo: false);
        var silla = TestDb.AddProduct(db, "Silla", 100_000m);
        var mesa = TestDb.AddProduct(db, "Mesa", 300_000m, true, ("Roble", 0m, true));
        var service = new OrderService(db, new FakePaymentService());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = inactivo.Id, Cantidad = 1 })));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearAsync(Pedido(new CreateOrderItemDto
            {
                ProductId = silla.Id, ProductVariantId = mesa.Variantes[0].Id, Cantidad = 1
            })));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = silla.Id, Cantidad = 0 })));
        Assert.Equal(0, await db.Orders.CountAsync());
    }

    [Fact]
    public async Task UnEventoDePagoViejoNoPisaElEstadoActual()
    {
        using var db = TestDb.Create();
        var silla = TestDb.AddProduct(db, "Silla", 100_000m);
        var service = new OrderService(db, new FakePaymentService());
        var pedido = await service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = silla.Id, Cantidad = 1 }));
        var ahora = new DateTime(2026, 10, 21, 15, 0, 0, DateTimeKind.Utc);

        Assert.True(await service.AplicarEventoPagoAsync(pedido.Codigo, "trx-1", PagoEstadoAgregado.Aprobado, ahora, 13_000_000, "COP"));
        // Un reintento rezagado de Wompi (anterior) que dice "rechazado" no debe deshacer la aprobación.
        Assert.False(await service.AplicarEventoPagoAsync(pedido.Codigo, "trx-1", PagoEstadoAgregado.Rechazado, ahora.AddMinutes(-5)));

        var guardado = await db.Orders.SingleAsync();
        Assert.Equal(OrderCatalogo.PagoConfirmado, guardado.Estado);
        Assert.Equal("trx-1", guardado.PagoTransaccionId);
    }

    [Fact]
    public async Task VerificarPagoNoConfiaEnElTransactionIdDelCliente()
    {
        using var db = TestDb.Create();
        var silla = TestDb.AddProduct(db, "Silla", 100_000m);
        var pagos = new AprobadorFalso();
        var service = new OrderService(db, pagos);
        var pedido = await service.CrearAsync(Pedido(new CreateOrderItemDto { ProductId = silla.Id, Cantidad = 1 }));

        var resultado = await service.VerificarPagoAsync(pedido.Codigo, "trx-de-otro-pedido");

        Assert.Equal("Pago pendiente", resultado!.Estado);
        Assert.Equal("Pago pendiente", (await db.Orders.SingleAsync()).Estado);
    }

    private sealed class AprobadorFalso : IPaymentService
    {
        public Task<PagoIntencionResultado> CrearIntencionAsync(NogalApi.Models.Order pedido, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagoIntencionResultado("wompi", "https://pago.test"));

        public Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default)
            => Task.FromResult<PagoEstadoResultado?>(new PagoEstadoResultado(PagoEstadoAgregado.Aprobado, transactionId, "APPROVED"));
    }
}
