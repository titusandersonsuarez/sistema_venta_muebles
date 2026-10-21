using Microsoft.Extensions.Options;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

/// <summary>
/// Selecciona la implementación de pago según Wompi:Provider. Sigue el
/// mismo patrón que Product3dGenerationDispatcher.
/// </summary>
public sealed class PaymentServiceDispatcher : IPaymentService
{
    private readonly DemoPaymentService _demo;
    private readonly WompiPaymentService _wompi;
    private readonly WompiOptions _options;
    private readonly ILogger<PaymentServiceDispatcher> _logger;

    public PaymentServiceDispatcher(
        DemoPaymentService demo,
        WompiPaymentService wompi,
        IOptions<WompiOptions> options,
        ILogger<PaymentServiceDispatcher> logger)
    {
        _demo = demo;
        _wompi = wompi;
        _options = options.Value;
        _logger = logger;
    }

    public Task<PagoIntencionResultado> CrearIntencionAsync(Order pedido, CancellationToken cancellationToken = default)
    {
        return ElegirServicio().CrearIntencionAsync(pedido, cancellationToken);
    }

    public Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return ElegirServicio().ConsultarAsync(transactionId, cancellationToken);
    }

    public string ProveedorActivo => (_options.Provider ?? "demo").ToLowerInvariant().Trim();

    private IPaymentService ElegirServicio()
    {
        var provider = ProveedorActivo;
        if (provider == "wompi")
        {
            if (string.IsNullOrWhiteSpace(_options.PublicKey) || string.IsNullOrWhiteSpace(_options.IntegritySecret))
            {
                _logger.LogWarning("Wompi:Provider = 'wompi' pero faltan credenciales. Aplicando modo demo como fallback.");
                return _demo;
            }
            return _wompi;
        }
        return _demo;
    }
}
