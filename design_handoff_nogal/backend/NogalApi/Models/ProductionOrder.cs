namespace NogalApi.Models;

public class ProductionOrder
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Etapa { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal DiasEnEtapa { get; set; }
    public DateTime FechaEntrada { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
