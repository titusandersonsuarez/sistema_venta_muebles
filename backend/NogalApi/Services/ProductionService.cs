using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models.Production;

namespace NogalApi.Services;

public class ProductionService : IProductionService
{
    private const int CapacidadMensual = 180;
    private readonly AppDbContext _context;

    public ProductionService(AppDbContext context) => _context = context;

    public async Task<ProductionSummaryDto> ObtenerResumenAsync(CancellationToken cancellationToken = default)
    {
        var ordenes = await _context.ProductionOrders.AsNoTracking().ToListAsync(cancellationToken);
        return new ProductionSummaryDto
        {
            Etapas = ProductionCatalogo.Etapas.Select(etapa =>
            {
                var enEtapa = ordenes.Where(o => o.Etapa == etapa).ToList();
                return new ProductionStageDto
                {
                    Nombre = etapa,
                    Ordenes = enEtapa.Sum(o => o.Cantidad),
                    DiasPromedio = enEtapa.Count == 0 ? 0 : Math.Round(enEtapa.Average(o => o.DiasEnEtapa), 1)
                };
            }).ToList(),
            CapacidadMensual = CapacidadMensual,
            Comprometido = ordenes.Sum(o => o.Cantidad)
        };
    }

    public async Task<IReadOnlyList<InventoryItemDto>> ListarInventarioAsync(CancellationToken cancellationToken = default)
    {
        var prioridad = new[] { "Crítico", "Bajo", "Normal" };
        return await _context.InventoryItems.AsNoTracking()
            .OrderBy(i => i.Estado == "Crítico" ? 0 : i.Estado == "Bajo" ? 1 : 2)
            .ThenBy(i => i.Nombre)
            .Select(i => new InventoryItemDto { Id = i.Id, Nombre = i.Nombre, Stock = i.Stock, Estado = i.Estado })
            .ToListAsync(cancellationToken);
    }
}
