namespace NogalApi.Models.Products;

/// <summary>
/// Valores permitidos para categoría, material y estado. La UI usa selects
/// controlados con estas mismas listas; el backend valida contra ellas para
/// no aceptar valores inventados desde el cliente.
/// </summary>
public static class ProductCatalogo
{
    public static readonly IReadOnlyList<string> Categorias = new[]
    {
        "Sofás",
        "Sillas",
        "Mesas",
        "Camas"
    };

    public static readonly IReadOnlyList<string> Materiales = new[]
    {
        "Madera maciza",
        "Tapizado",
        "Metal y madera"
    };

    public static readonly IReadOnlyList<string> Estados = new[]
    {
        "Disponible",
        "EnProceso",
        "Vendido"
    };
}
