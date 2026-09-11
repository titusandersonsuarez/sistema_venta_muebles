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

    /// <summary>
    /// Variante de acabado elegida por el cliente (opcional). Se guarda
    /// denormalizada por la misma razón que <see cref="NombreProducto"/>:
    /// si la variante cambia después, el pedido debe seguir mostrando lo
    /// que el cliente vio y aceptó.
    /// </summary>
    public int? ProductVariantId { get; set; }
    public string? VarianteNombre { get; set; }
    public decimal PrecioAjusteVariante { get; set; }

    public Order? Order { get; set; }
    public Product? Product { get; set; }
    public ProductVariant? Variante { get; set; }
}
