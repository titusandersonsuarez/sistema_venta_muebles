namespace NogalApi.Models.Production;

public class ProductionStageDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Ordenes { get; set; }
    public decimal DiasPromedio { get; set; }
}

public class ProductionSummaryDto
{
    public List<ProductionStageDto> Etapas { get; set; } = new();
    public int CapacidadMensual { get; set; }
    public int Comprometido { get; set; }
}

public class InventoryItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Stock { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}
