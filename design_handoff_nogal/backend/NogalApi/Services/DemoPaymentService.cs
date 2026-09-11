using Microsoft.Extensions.Options;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

/// <summary>
/// Simula el flujo de pago sin llamar a Wompi. El "checkout" es una
/// pantalla propia (/carrito/pago-demo/:codigo) con botones Aprobar y
/// Rechazar. La confirmación entra por el controller demo, no por
/// ConsultarAsync.
/// </summary>
public class DemoPaymentService : IPaymentService
{
    private readonly WompiOptions _options;

    public DemoPaymentService(IOptions<WompiOptions> options)
    {
        _options = options.Value;
    }

    public Task<PagoIntencionResultado> CrearIntencionAsync(Order pedido, CancellationToken cancellationToken = default)
    {
        var url = $"{_options.DemoCheckoutBaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(pedido.Codigo)}";
        return Task.FromResult(new PagoIntencionResultado("demo", url));
    }

    public Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        // En demo la fuente de verdad es el propio callback, no hay
        // transacción externa para consultar.
        return Task.FromResult<PagoEstadoResultado?>(null);
    }
}
