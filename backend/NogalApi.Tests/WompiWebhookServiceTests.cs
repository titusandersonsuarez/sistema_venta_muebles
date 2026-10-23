using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NogalApi.Models.Orders;
using NogalApi.Options;
using NogalApi.Services;

namespace NogalApi.Tests;

public class WompiWebhookServiceTests
{
    private const string Secret = "test_events_secret";

    private static string Firmar(string id, string status, long amount, long timestamp, string secret = Secret)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{id}{status}{amount}{timestamp}{secret}"));
        return Convert.ToHexString(bytes);
    }

    private static string Evento(string reference, string id, string status, long timestamp, string checksum, long amount = 39000000) => $$"""
        {
          "event": "transaction.updated",
          "data": { "transaction": { "id": "{{id}}", "status": "{{status}}", "amount_in_cents": {{amount}}, "currency": "COP", "reference": "{{reference}}" } },
          "signature": {
            "properties": ["transaction.id", "transaction.status", "transaction.amount_in_cents"],
            "checksum": "{{checksum}}"
          },
          "timestamp": {{timestamp}}
        }
        """;

    private static (WompiWebhookService webhook, OrderService orders) Crear(NogalApi.Data.AppDbContext db, string? secret = Secret)
    {
        var orders = new OrderService(db, new FakePaymentService());
        var options = Microsoft.Extensions.Options.Options.Create(new WompiOptions { EventsSecret = secret });
        return (new WompiWebhookService(db, orders, options, NullLogger<WompiWebhookService>.Instance), orders);
    }

    private static async Task<string> CrearPedido(OrderService orders, NogalApi.Data.AppDbContext db)
    {
        var silla = TestDb.AddProduct(db, "Silla", 360_000m);
        var pedido = await orders.CrearAsync(new CreateOrderDto
        {
            Cliente = "Ana", Ciudad = "Bogotá", Contacto = "3000000000",
            Items = { new CreateOrderItemDto { ProductId = silla.Id, Cantidad = 1 } }
        });
        return pedido.Codigo;
    }

    [Fact]
    public async Task ConFirmaValidaConfirmaElPagoDelPedido()
    {
        using var db = TestDb.Create();
        var (webhook, orders) = Crear(db);
        var codigo = await CrearPedido(orders, db);
        const long ts = 1792600000;

        var res = await webhook.ProcesarAsync(Evento(codigo, "trx-123", "APPROVED", ts, Firmar("trx-123", "APPROVED", 39000000, ts)));

        Assert.Equal(WebhookResultadoCodigo.Ok, res.Codigo);
        var pedido = await db.Orders.SingleAsync();
        Assert.Equal(OrderCatalogo.PagoConfirmado, pedido.Estado);
        Assert.Equal("trx-123", pedido.PagoTransaccionId);
        Assert.True((await db.PaymentEvents.SingleAsync()).FirmaVerificada);
    }

    [Fact]
    public async Task ConFirmaFalsaGuardaElEventoPeroNoTocaElPedido()
    {
        using var db = TestDb.Create();
        var (webhook, orders) = Crear(db);
        var codigo = await CrearPedido(orders, db);
        const long ts = 1792600000;
        var firmaFalsa = Firmar("trx-123", "APPROVED", 39000000, ts, secret: "otro_secreto");

        var res = await webhook.ProcesarAsync(Evento(codigo, "trx-123", "APPROVED", ts, firmaFalsa));

        Assert.Equal(WebhookResultadoCodigo.FirmaInvalida, res.Codigo);
        Assert.Equal("Pago pendiente", (await db.Orders.SingleAsync()).Estado);
        Assert.False((await db.PaymentEvents.SingleAsync()).FirmaVerificada);
    }

    [Fact]
    public async Task SinSecretoConfiguradoNoProcesaNada()
    {
        using var db = TestDb.Create();
        var (webhook, _) = Crear(db, secret: null);

        var res = await webhook.ProcesarAsync("{}");

        Assert.Equal(WebhookResultadoCodigo.NoConfigurado, res.Codigo);
        Assert.Empty(db.PaymentEvents);
    }

    [Theory]
    [InlineData("esto no es json")]
    [InlineData("""{ "event": "transaction.updated", "timestamp": 1 }""")]
    [InlineData("""{ "signature": { "checksum": "x", "properties": ["transaction.no_existe"] }, "timestamp": 1, "data": { "transaction": {} } }""")]
    public async Task RechazaPayloadsMalFormados(string body)
    {
        using var db = TestDb.Create();
        var (webhook, _) = Crear(db);

        var res = await webhook.ProcesarAsync(body);

        Assert.Equal(WebhookResultadoCodigo.PayloadInvalido, res.Codigo);
    }

    [Fact]
    public async Task AprobacionConMontoDistintoAlTotalNoConfirmaElPedido()
    {
        using var db = TestDb.Create();
        var (webhook, orders) = Crear(db);
        var codigo = await CrearPedido(orders, db);
        const long ts = 1792600000;

        var res = await webhook.ProcesarAsync(Evento(codigo, "trx-barato", "APPROVED", ts,
            Firmar("trx-barato", "APPROVED", 100000, ts), amount: 100000));

        Assert.Equal(WebhookResultadoCodigo.Ok, res.Codigo);
        Assert.Equal("Pago pendiente", (await db.Orders.SingleAsync()).Estado);
    }

    [Fact]
    public async Task UnEventoConFirmaFalsaNoBloqueaElEventoRealPosterior()
    {
        using var db = TestDb.Create();
        var (webhook, orders) = Crear(db);
        var codigo = await CrearPedido(orders, db);
        const long ts = 1792600000;

        var falso = await webhook.ProcesarAsync(Evento(codigo, "trx-123", "APPROVED", ts,
            Firmar("trx-123", "APPROVED", 39000000, ts, secret: "otro_secreto")));
        var real = await webhook.ProcesarAsync(Evento(codigo, "trx-123", "APPROVED", ts,
            Firmar("trx-123", "APPROVED", 39000000, ts)));

        Assert.Equal(WebhookResultadoCodigo.FirmaInvalida, falso.Codigo);
        Assert.Equal(WebhookResultadoCodigo.Ok, real.Codigo);
        Assert.Null(real.Detalle);
        Assert.Equal(OrderCatalogo.PagoConfirmado, (await db.Orders.SingleAsync()).Estado);
    }
}
