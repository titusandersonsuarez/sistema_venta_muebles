namespace NogalApi.Models;

public class Order
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Estado { get; set; } = "Pago pendiente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<OrderItem> Items { get; set; } = new();
}
