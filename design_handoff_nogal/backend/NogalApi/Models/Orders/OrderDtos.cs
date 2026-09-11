namespace NogalApi.Models.Orders;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
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
}

public class UpdateOrderStatusDto
{
    public string Estado { get; set; } = string.Empty;
}
