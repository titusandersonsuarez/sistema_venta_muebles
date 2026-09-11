using NogalApi.Models.Products;

namespace NogalApi.Services;

public interface IProductService
{
    Task<PagedResult<ProductPublicDto>> ListarPublicoAsync(ProductFilters filtros);
    Task<ProductPublicDto?> ObtenerPorSlugAsync(string slug);

    Task<PagedResult<ProductDto>> ListarAdminAsync(int pagina, int tamano, bool incluirInactivos);
    Task<ProductDto?> ObtenerAdminAsync(int id);
    Task<ProductDto> CrearAsync(CreateProductDto dto);
    Task<ProductDto?> ActualizarAsync(int id, UpdateProductDto dto);
    Task<bool> EliminarAsync(int id);
    Task<ProductDto?> RestaurarAsync(int id);
    Task<ProductDto?> AsignarImagenUrlAsync(int id, string url);
    Task<ProductDto?> SolicitarModelo3dAsync(int id);
}
