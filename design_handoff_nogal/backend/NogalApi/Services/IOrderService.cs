using NogalApi.Models.Orders;
using NogalApi.Models.Products;

namespace NogalApi.Services;

public interface IOrderService
{
    Task<PagedResult<OrderListItemDto>> ListarAsync(int pagina, int tamano, string? estado);
    Task<OrderDto?> ObtenerAsync(int id);
    Task<OrderDto?> ObtenerPorCodigoAsync(string codigo);
    Task<OrderDto?> CambiarEstadoAsync(int id, string nuevoEstado);
    Task<OrderDto> CrearAsync(CreateOrderDto dto);
    Task<PagoIntencionDto?> IniciarPagoAsync(string codigo, CancellationToken cancellationToken = default);
    Task<OrderDto?> ConfirmarPagoDemoAsync(string codigo, bool aprobado, CancellationToken cancellationToken = default);
    Task<OrderDto?> VerificarPagoAsync(string codigo, string transactionId, CancellationToken cancellationToken = default);
}
