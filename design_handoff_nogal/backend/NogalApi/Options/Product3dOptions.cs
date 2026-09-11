namespace NogalApi.Options;

public class Product3dOptions
{
    public const string SectionName = "Product3d";

    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "demo"; // "meshy", "demo", "none"
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public string? PublicBaseUrl { get; set; }
    public int PollingIntervalSeconds { get; set; } = 5;
    public int MaxWaitMinutes { get; set; } = 3;
    public bool FallbackToDemoOnFailure { get; set; } = true;
}