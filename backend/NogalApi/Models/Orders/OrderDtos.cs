namespace NogalApi.Models.Orders;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public int? ProductVariantId { get; set; }
    public string? VarianteNombre { get; set; }
    public decimal PrecioAjusteVariante { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public decimal Total { get; set; }
    public decimal EnvioCOP { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PagoProveedor { get; set; }
    public string? PagoTransaccionId { get; set; }
    public DateTime? PagoActualizadoEn { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Respuesta a POST /api/orders/{codigo}/pago. El frontend redirige al
/// checkoutUrl para que el cliente pague (Wompi hosted o pantalla demo).
/// </summary>
public class PagoIntencionDto
{
    public string CheckoutUrl { get; set; } = string.Empty;
    public string Proveedor { get; set; } = string.Empty;
}

/// <summary>
/// Payload del callback demo (botón "Aprobar / Rechazar" de la pantalla
/// simulada). En producción esto lo hace la pasarela vía redirect + webhook.
/// </summary>
public class ConfirmarPagoDemoDto
{
    public bool Aprobado { get; set; }
}

/// <summary>
/// Vista compacta para la tabla del panel. "ResumenProductos" es una
/// línea del tipo "Mesa auxiliar Sáchica" (o "Mesa Sáchica + 2 más" si
/// hay varios items), para meter en la columna "Producto" del prototipo.
/// </summary>
public class OrderListItemDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string ResumenProductos { get; set; } = string.Empty;
    public string? PagoProveedor { get; set; }
    public string? PagoTransaccionId { get; set; }
    public DateTime? PagoActualizadoEn { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Estado { get; set; } = string.Empty;
}

/// <summary>
/// Payload que envía la tienda pública al confirmar el carrito.
/// El servidor recalcula precios y totales; los valores del cliente
/// solo sirven para identificar producto y variante.
/// </summary>
public class CreateOrderDto
{
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int ProductId { get; set; }
    public int? ProductVariantId { get; set; }
    public int Cantidad { get; set; }
}
