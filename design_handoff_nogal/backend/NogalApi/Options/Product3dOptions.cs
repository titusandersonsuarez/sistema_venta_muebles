namespace NogalApi.Options;

public class Product3dOptions
{
    public const string SectionName = "Product3d";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "none";
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
}