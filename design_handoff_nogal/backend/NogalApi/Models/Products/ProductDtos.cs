namespace NogalApi.Models.Products;

public class ProductImageDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public int Orden { get; set; }
}

/// <summary>
/// Representación completa (uso interno del panel). Incluye Activo y fechas.
/// </summary>
public class ProductDto
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
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    public List<ProductImageDto> Imagenes { get; set; } = new();
}

/// <summary>
/// Vista pública del producto para la tienda: no expone campos administrativos.
/// </summary>
public class ProductPublicDto
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
    public List<ProductImageDto> Imagenes { get; set; } = new();
}

public class CreateProductDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public decimal PrecioCOP { get; set; }
    public string? Medidas { get; set; }
    public string? Peso { get; set; }
    public string? Armado { get; set; }
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
    public string? ImagenUrl { get; set; }
}

public class UpdateProductDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public decimal PrecioCOP { get; set; }
    public string? Medidas { get; set; }
    public string? Peso { get; set; }
    public string? Armado { get; set; }
    public string? Descripcion { get; set; }
    public string? Estado { get; set; }
    public string? ImagenUrl { get; set; }
}

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int Tamano { get; set; }
}

public class ProductFilters
{
    public string? Categoria { get; set; }
    public string? Material { get; set; }
    public decimal? PrecioMax { get; set; }
    public int Pagina { get; set; } = 1;
    public int Tamano { get; set; } = 20;
}
