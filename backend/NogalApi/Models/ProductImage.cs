namespace NogalApi.Models;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }

    public Product? Product { get; set; }
}
