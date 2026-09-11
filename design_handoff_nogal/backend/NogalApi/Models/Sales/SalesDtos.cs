namespace NogalApi.Models.Sales;

public class SalesDashboardDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public string Granularity { get; set; } = string.Empty;
    public SalesTotalsDto Totales { get; set; } = new();
    public List<SalesBucketDto> Buckets { get; set; } = new();
    public List<SalesCategoryDto> Categorias { get; set; } = new();
    public List<TopProductSalesDto> MasVendidos { get; set; } = new();
}

public class SalesTotalsDto
{
    public decimal Ventas { get; set; }
    public int Pedidos { get; set; }
    public int Unidades { get; set; }
    public decimal TicketPromedio { get; set; }
}

public class SalesBucketDto
{
    public string Label { get; set; } = string.Empty;
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public decimal Ventas { get; set; }
    public int Pedidos { get; set; }
}

public class SalesCategoryDto
{
    public string Categoria { get; set; } = string.Empty;
    public decimal Ventas { get; set; }
    public int Pedidos { get; set; }
    public decimal Porcentaje { get; set; }
}

public class TopProductSalesDto
{
    public int ProductId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Unidades { get; set; }
    public decimal Ventas { get; set; }
}