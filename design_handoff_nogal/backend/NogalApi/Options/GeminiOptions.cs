namespace NogalApi.Options;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-1.5-flash";
}
