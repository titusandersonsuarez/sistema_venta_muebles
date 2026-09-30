namespace NogalApi.Models;

/// <summary>
/// Evento recibido de una pasarela de pago (hoy solo Wompi). Se guarda
/// crudo para auditoría y se usa para idempotencia: si el mismo evento
/// llega dos veces (Wompi reintenta cuando el webhook no responde 2xx),
/// el índice único (Proveedor, EventoId) impide aplicarlo dos veces.
/// </summary>
public class PaymentEvent
{
    public int Id { get; set; }

    /// <summary>"wompi" por ahora; deja lugar para PSE, PayU, etc.</summary>
    public string Proveedor { get; set; } = string.Empty;

    /// <summary>Id del evento tal cual lo mandó la pasarela.</summary>
    public string EventoId { get; set; } = string.Empty;

    /// <summary>Ej. "transaction.updated".</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>JSON completo del body para poder revisar después qué llegó.</summary>
    public string PayloadRaw { get; set; } = string.Empty;

    /// <summary>
    /// True si la firma <c>X-Event-Checksum</c> validó contra
    /// <c>Wompi:EventsSecret</c>. Los eventos con firma inválida se
    /// guardan igual para poder investigar, pero no se aplican al pedido.
    /// </summary>
    public bool FirmaVerificada { get; set; }

    /// <summary>Código NGL-XXXXXX del pedido resuelto por reference (nullable si no matchea).</summary>
    public string? PedidoCodigo { get; set; }

    public string? TransaccionId { get; set; }

    /// <summary>Estado agregado ("Aprobado" / "Rechazado" / "Pendiente") al momento del evento.</summary>
    public string? EstadoResultado { get; set; }

    /// <summary>Timestamp del evento reportado por la pasarela, en UTC.</summary>
    public DateTime? EventoEmitidoEn { get; set; }

    public DateTime RecibidoEn { get; set; } = DateTime.UtcNow;
}
