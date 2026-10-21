namespace NogalApi.Models;

public class ProductVariant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Madera"; // "Madera", "Tela", "Color"
    public string? CodigoColorHex { get; set; }
    public decimal PrecioAjusteCOP { get; set; }
    public string? FotoUrl { get; set; }
    public int Stock { get; set; } = 10;
    public bool Activo { get; set; } = true;
    public int Orden { get; set; }

    public Product? Product { get; set; }
}
