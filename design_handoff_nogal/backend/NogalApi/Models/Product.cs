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
    public string? Modelo3dUrl { get; set; }
    public string? ModeloUsdzUrl { get; set; }
    public string Modelo3dEstado { get; set; } = "Sin modelo";
    public string? Modelo3dError { get; set; }
    public DateTime? Modelo3dSolicitadoEn { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public List<ProductImage> Imagenes { get; set; } = new();
}
