namespace NogalApi.Models;

public class Order
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    /// <summary>
    /// Correo o WhatsApp que dejó el cliente al confirmar el pedido.
    /// Opcional en pedidos históricos creados por seeders anteriores.
    /// </summary>
    public string? Contacto { get; set; }
    public decimal Total { get; set; }
    public decimal EnvioCOP { get; set; }
    public string Estado { get; set; } = "Pago pendiente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Trazabilidad del pago: proveedor con el que se creó la intención
    /// ("demo" o "wompi"), el id de la transacción externa y cuándo se
    /// actualizó por última vez. Todo nullable porque los pedidos
    /// históricos se sembraron sin flujo de pago.
    /// </summary>
    public string? PagoProveedor { get; set; }
    public string? PagoTransaccionId { get; set; }
    public DateTime? PagoActualizadoEn { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}
