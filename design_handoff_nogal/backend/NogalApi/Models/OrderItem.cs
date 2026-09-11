namespace NogalApi.Models;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }

    /// <summary>
    /// Nombre del producto al momento de la compra. Se guarda denormalizado
    /// para que si el producto cambia de nombre después, el pedido siga
    /// mostrando lo que el cliente realmente vio y aceptó.
    /// </summary>
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
}
