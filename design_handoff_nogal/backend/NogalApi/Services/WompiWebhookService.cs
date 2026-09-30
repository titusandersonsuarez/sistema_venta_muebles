using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

/// <summary>
/// Recibe eventos <c>transaction.updated</c> de Wompi. La firma viene en
/// el body: <c>signature.checksum</c> = SHA256(concat(propiedades listadas
/// en <c>signature.properties</c>) + timestamp + EventsSecret). El
/// checksum de Wompi está en mayúsculas hex.
/// </summary>
public sealed class WompiWebhookService : IWompiWebhookService
{
    private readonly AppDbContext _context;
    private readonly IOrderService _orders;
    private readonly WompiOptions _options;
    private readonly ILogger<WompiWebhookService> _logger;

    public WompiWebhookService(
        AppDbContext context,
        IOrderService orders,
        IOptions<WompiOptions> options,
        ILogger<WompiWebhookService> logger)
    {
        _context = context;
        _orders = orders;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<WebhookResultado> ProcesarAsync(string bodyRaw, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.EventsSecret))
        {
            _logger.LogWarning("Webhook Wompi recibido pero falta 'Wompi:EventsSecret'.");
            return new WebhookResultado(WebhookResultadoCodigo.NoConfigurado);
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(bodyRaw);
        }
        catch (JsonException)
        {
            return new WebhookResultado(WebhookResultadoCodigo.PayloadInvalido, "JSON inválido.");
        }

        using (doc)
        {
            var root = doc.RootElement;
            if (!root.TryGetProperty("signature", out var signature) ||
                !signature.TryGetProperty("checksum", out var checksumProp) ||
                !signature.TryGetProperty("properties", out var propertiesProp) ||
                propertiesProp.ValueKind != JsonValueKind.Array ||
                !root.TryGetProperty("timestamp", out var timestampProp))
            {
                return new WebhookResultado(WebhookResultadoCodigo.PayloadInvalido, "Faltan signature/timestamp.");
            }

            var timestampCrudo = timestampProp.ValueKind switch
            {
                JsonValueKind.Number => timestampProp.GetRawText(),
                JsonValueKind.String => timestampProp.GetString() ?? string.Empty,
                _ => string.Empty
            };
            if (string.IsNullOrEmpty(timestampCrudo))
            {
                return new WebhookResultado(WebhookResultadoCodigo.PayloadInvalido, "timestamp inválido.");
            }

            var checksumEsperado = checksumProp.GetString() ?? string.Empty;
            var concat = new StringBuilder();
            foreach (var prop in propertiesProp.EnumerateArray())
            {
                var path = prop.GetString();
                if (string.IsNullOrEmpty(path))
                {
                    return new WebhookResultado(WebhookResultadoCodigo.PayloadInvalido,
                        "signature.properties contiene un valor vacío.");
                }
                if (!TryResolverPath(root, path, out var valor))
                {
                    return new WebhookResultado(WebhookResultadoCodigo.PayloadInvalido,
                        $"La propiedad '{path}' listada en signature.properties no existe en el body.");
                }
                concat.Append(valor);
            }
            concat.Append(timestampCrudo);
            concat.Append(_options.EventsSecret);

            var checksumCalculado = HexSha256(concat.ToString());
            var firmaOk = string.Equals(checksumCalculado, checksumEsperado, StringComparison.OrdinalIgnoreCase);

            // Datos del evento (pueden faltar si el body vino raro, aceptamos null).
            var tipo = root.TryGetProperty("event", out var e) ? e.GetString() ?? string.Empty : string.Empty;
            root.TryGetProperty("data", out var data);
            data.TryGetProperty("transaction", out var trx);
            var trxId = trx.ValueKind == JsonValueKind.Object && trx.TryGetProperty("id", out var idProp)
                ? idProp.GetString()
                : null;
            var reference = trx.ValueKind == JsonValueKind.Object && trx.TryGetProperty("reference", out var refProp)
                ? refProp.GetString()
                : null;
            var statusRaw = trx.ValueKind == JsonValueKind.Object && trx.TryGetProperty("status", out var stProp)
                ? stProp.GetString() ?? "PENDING"
                : "PENDING";
            var estado = statusRaw.ToUpperInvariant() switch
            {
                "APPROVED" => PagoEstadoAgregado.Aprobado,
                "DECLINED" or "VOIDED" or "ERROR" => PagoEstadoAgregado.Rechazado,
                _ => PagoEstadoAgregado.Pendiente
            };

            // El "id" del evento no lo garantiza Wompi con nombre propio;
            // combinamos trxId + timestamp para lograr una llave estable
            // que sea idempotente aunque Wompi reintente el mismo evento.
            var eventoId = $"{trxId ?? "sintx"}:{timestampCrudo}";
            var emitidoEn = long.TryParse(timestampCrudo, out var unix)
                ? DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime
                : DateTime.UtcNow;

            // Persistir siempre el evento (firma válida o no), para auditoría.
            var registro = new PaymentEvent
            {
                Proveedor = "wompi",
                EventoId = eventoId,
                Tipo = string.IsNullOrEmpty(tipo) ? "transaction.updated" : tipo,
                PayloadRaw = bodyRaw,
                FirmaVerificada = firmaOk,
                PedidoCodigo = reference,
                TransaccionId = trxId,
                EstadoResultado = estado.ToString(),
                EventoEmitidoEn = emitidoEn,
                RecibidoEn = DateTime.UtcNow
            };
            _context.PaymentEvents.Add(registro);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // El índice único (Proveedor, EventoId) rechaza reintentos.
                // Confirmamos que efectivamente fue eso y no otro error.
                _context.Entry(registro).State = EntityState.Detached;
                if (await EventoYaExisteAsync("wompi", eventoId, cancellationToken))
                {
                    return new WebhookResultado(WebhookResultadoCodigo.Ok, "Evento duplicado, ignorado.");
                }
                throw;
            }

            if (!firmaOk)
            {
                _logger.LogWarning("Webhook Wompi con firma inválida. eventoId={EventoId}", eventoId);
                return new WebhookResultado(WebhookResultadoCodigo.FirmaInvalida);
            }

            if (!string.IsNullOrEmpty(reference) && !string.IsNullOrEmpty(trxId))
            {
                var aplicado = await _orders.AplicarEventoPagoAsync(
                    reference, trxId, estado, emitidoEn, cancellationToken);
                _logger.LogInformation(
                    "Webhook Wompi {Estado} para pedido {Codigo} (aplicado={Aplicado}).",
                    estado, reference, aplicado);
            }

            return new WebhookResultado(WebhookResultadoCodigo.Ok);
        }
    }

    private Task<bool> EventoYaExisteAsync(string proveedor, string eventoId, CancellationToken ct)
        => _context.PaymentEvents.AsNoTracking()
            .AnyAsync(e => e.Proveedor == proveedor && e.EventoId == eventoId, ct);

    /// <summary>
    /// Resuelve un path tipo "transaction.id" contra el subárbol
    /// <c>data</c> del body — Wompi lista propiedades sin el prefijo "data.".
    /// Devuelve el valor como string tal cual lo enviaron
    /// (números sin comillas, strings sin comillas envolventes).
    /// </summary>
    private static bool TryResolverPath(JsonElement root, string path, out string valor)
    {
        valor = string.Empty;
        if (!root.TryGetProperty("data", out var actual)) return false;
        foreach (var segmento in path.Split('.'))
        {
            if (actual.ValueKind != JsonValueKind.Object) return false;
            if (!actual.TryGetProperty(segmento, out var siguiente)) return false;
            actual = siguiente;
        }
        valor = actual.ValueKind switch
        {
            JsonValueKind.String => actual.GetString() ?? string.Empty,
            JsonValueKind.Number => actual.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            _ => actual.GetRawText()
        };
        return true;
    }

    private static string HexSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
