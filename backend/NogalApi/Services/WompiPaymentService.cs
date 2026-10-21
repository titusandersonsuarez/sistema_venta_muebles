using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

/// <summary>
/// Implementación real contra Wompi Redirection API (sandbox o producción,
/// dependiendo del prefijo de la public key: pub_test_ vs pub_prod_).
///
/// Flujo:
/// 1. <see cref="CrearIntencionAsync"/> arma la URL hosted con
///    <c>amount-in-cents</c>, <c>reference</c>, <c>currency</c>,
///    <c>signature:integrity</c> (SHA256 de reference+monto+moneda+secret)
///    y <c>redirect-url</c>.
/// 2. El cliente paga en la página de Wompi.
/// 3. Wompi redirige a <c>redirect-url</c> con <c>?id=&amp;env=</c>.
/// 4. <see cref="ConsultarAsync"/> confirma el estado real vía
///    <c>GET /transactions/{id}</c>.
/// </summary>
public class WompiPaymentService : IPaymentService
{
    private readonly WompiOptions _options;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<WompiPaymentService> _logger;

    public WompiPaymentService(
        IOptions<WompiOptions> options,
        IHttpClientFactory clientFactory,
        ILogger<WompiPaymentService> logger)
    {
        _options = options.Value;
        _clientFactory = clientFactory;
        _logger = logger;
    }

    public Task<PagoIntencionResultado> CrearIntencionAsync(Order pedido, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicKey))
            throw new InvalidOperationException("Falta 'Wompi:PublicKey' en la configuración.");
        if (string.IsNullOrWhiteSpace(_options.IntegritySecret))
            throw new InvalidOperationException("Falta 'Wompi:IntegritySecret' en la configuración.");

        var amountInCents = (long)Math.Round(pedido.Total * 100m);
        var reference = pedido.Codigo;
        const string currency = "COP";
        var redirectUrl = $"{_options.RedirectBaseUrl.TrimEnd('/')}/{Uri.EscapeDataString(reference)}";
        var integritySignature = CalcularFirmaIntegridad(reference, amountInCents, currency, _options.IntegritySecret!);

        var query = new List<KeyValuePair<string, string>>
        {
            new("public-key", _options.PublicKey!),
            new("currency", currency),
            new("amount-in-cents", amountInCents.ToString()),
            new("reference", reference),
            new("signature:integrity", integritySignature),
            new("redirect-url", redirectUrl)
        };

        var url = _options.CheckoutBaseUrl + "?" + string.Join("&", query.Select(kv =>
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));

        return Task.FromResult(new PagoIntencionResultado("wompi", url));
    }

    public async Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient();
        var url = $"{_options.ApiBaseUrl.TrimEnd('/')}/transactions/{Uri.EscapeDataString(transactionId)}";
        try
        {
            using var response = await client.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Wompi respondió {Status} al consultar transacción {Id}.", response.StatusCode, transactionId);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!json.RootElement.TryGetProperty("data", out var data)) return null;
            if (!data.TryGetProperty("status", out var statusProp)) return null;

            var status = statusProp.GetString() ?? "PENDING";
            var estado = status.ToUpperInvariant() switch
            {
                "APPROVED" => PagoEstadoAgregado.Aprobado,
                "DECLINED" or "VOIDED" or "ERROR" => PagoEstadoAgregado.Rechazado,
                _ => PagoEstadoAgregado.Pendiente
            };
            return new PagoEstadoResultado(estado, transactionId, status);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar a Wompi para consultar la transacción {Id}.", transactionId);
            return null;
        }
    }

    private static string CalcularFirmaIntegridad(string reference, long amountInCents, string currency, string integritySecret)
    {
        var payload = $"{reference}{amountInCents}{currency}{integritySecret}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
