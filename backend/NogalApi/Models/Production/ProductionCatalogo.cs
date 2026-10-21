namespace NogalApi.Models.Production;

public static class ProductionCatalogo
{
    public const string Corte = "Corte";
    public const string Armado = "Armado";
    public const string Tapiceria = "Tapicería";
    public const string AcabadoYEmpaque = "Acabado y empaque";

    public static readonly IReadOnlyList<string> Etapas = [Corte, Armado, Tapiceria, AcabadoYEmpaque];
    public static readonly IReadOnlyList<string> EstadosInventario = ["Crítico", "Bajo", "Normal"];
}
