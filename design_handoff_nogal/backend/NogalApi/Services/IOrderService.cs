using NogalApi.Models.Orders;
using NogalApi.Models.Products;

namespace NogalApi.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> ListarAsync(int pagina, int tamano, string? estado);
    Task<OrderDto?> ObtenerAsync(int id);
    Task<OrderDto?> CambiarEstadoAsync(int id, string nuevoEstado);
}
