namespace NogalApi.Services;

public interface IWompiWebhookService
{
    /// <summary>
    /// Procesa un evento entrante de Wompi. Valida la firma del body
    /// contra <c>Wompi:EventsSecret</c>, guarda el evento crudo para
    /// auditoría (idempotente por <c>proveedor + eventoId</c>) y aplica
    /// el estado resultante al pedido si la firma es válida.
    /// </summary>
    Task<WebhookResultado> ProcesarAsync(string bodyRaw, CancellationToken cancellationToken = default);
}

public enum WebhookResultadoCodigo
{
    /// <summary>Evento aceptado y aplicado (o descartado por idempotencia).</summary>
    Ok,
    /// <summary>Body no parseable como JSON o le faltan campos obligatorios.</summary>
    PayloadInvalido,
    /// <summary>Firma no coincide con EventsSecret.</summary>
    FirmaInvalida,
    /// <summary>Falta configurar EventsSecret en appsettings.</summary>
    NoConfigurado
}

public record WebhookResultado(WebhookResultadoCodigo Codigo, string? Detalle = null);
