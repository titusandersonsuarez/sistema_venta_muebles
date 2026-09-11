namespace NogalApi.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// Ruta física relativa al ContentRoot donde se guardan los archivos.
    /// En dev: "wwwroot/uploads/products". En producción se cambia por un bucket.
    /// </summary>
    public string RutaFisica { get; set; } = "wwwroot/uploads/products";

    /// <summary>
    /// Prefijo público de URL que devolvemos al cliente
    /// (lo que se guarda en Product.ImagenUrl).
    /// </summary>
    public string RutaPublica { get; set; } = "/uploads/products";

    /// <summary>
    /// Tamaño máximo permitido por archivo (MB).
    /// </summary>
    public int TamanoMaximoMb { get; set; } = 5;
}
