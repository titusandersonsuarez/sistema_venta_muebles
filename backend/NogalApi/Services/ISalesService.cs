using NogalApi.Models.Sales;

namespace NogalApi.Services;

public interface ISalesService
{
    Task<SalesDashboardDto> ObtenerAsync(
        DateOnly from,
        DateOnly to,
        string granularity,
        CancellationToken cancellationToken = default);
}