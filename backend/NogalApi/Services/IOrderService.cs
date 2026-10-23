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

    /// <summary>
    /// Aplica el resultado de un evento asíncrono de pasarela (webhook)
    /// al pedido identificado por <paramref name="codigo"/>. Ignora
    /// eventos más viejos que la última actualización del pago para no
    /// pisar un estado ya confirmado con uno anterior que llegue tarde.
    /// Una aprobación solo se aplica si <paramref name="montoEnCentavos"/> y
    /// <paramref name="moneda"/> coinciden con el total del pedido.
    /// Devuelve true si el pedido cambió, false si el evento se descartó.
    /// </summary>
    Task<bool> AplicarEventoPagoAsync(
        string codigo,
        string transactionId,
        PagoEstadoAgregado estado,
        DateTime eventoEmitidoEn,
        long? montoEnCentavos = null,
        string? moneda = null,
        CancellationToken cancellationToken = default);
}
