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
    private readonly IHostEnvironment _env;
    private readonly ILogger<PaymentServiceDispatcher> _logger;

    public PaymentServiceDispatcher(
        DemoPaymentService demo,
        WompiPaymentService wompi,
        IOptions<WompiOptions> options,
        IHostEnvironment env,
        ILogger<PaymentServiceDispatcher> logger)
    {
        _demo = demo;
        _wompi = wompi;
        _options = options.Value;
        _env = env;
        _logger = logger;
    }

    /// <summary>El modo demo solo se permite en Development o con Wompi:AllowDemo=true.</summary>
    public static bool DemoPermitido(WompiOptions options, IHostEnvironment env)
        => env.IsDevelopment() || options.AllowDemo;

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
                // Sin fallback silencioso a demo: en demo el propio cliente
                // "aprueba" su pago. Fallar cerrado.
                _logger.LogError("Wompi:Provider = 'wompi' pero faltan PublicKey/IntegritySecret. Pagos deshabilitados.");
                throw new InvalidOperationException("La pasarela de pagos no está configurada.");
            }
            return _wompi;
        }
        if (!DemoPermitido(_options, _env))
        {
            _logger.LogError("Wompi:Provider = '{Provider}' no está permitido fuera de Development (Wompi:AllowDemo=false).", provider);
            throw new InvalidOperationException("La pasarela de pagos no está configurada.");
        }
        return _demo;
    }
}
