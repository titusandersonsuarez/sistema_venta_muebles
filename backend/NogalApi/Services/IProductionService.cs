using NogalApi.Models.Production;

namespace NogalApi.Services;

public interface IProductionService
{
    Task<ProductionSummaryDto> ObtenerResumenAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryItemDto>> ListarInventarioAsync(CancellationToken cancellationToken = default);
}
