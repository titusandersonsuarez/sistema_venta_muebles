namespace NogalApi.Models;

public class Product
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public decimal PrecioCOP { get; set; }
    public string? Medidas { get; set; }
    public string? Peso { get; set; }
    public string? Armado { get; set; }
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Disponible";
    public string? ImagenUrl { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public List<ProductImage> Imagenes { get; set; } = new();
}
