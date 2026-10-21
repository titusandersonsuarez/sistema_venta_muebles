namespace NogalApi.Models;

public class InventoryItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Stock { get; set; } = string.Empty;
    public string Estado { get; set; } = "Normal";
}
