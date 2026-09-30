namespace NogalApi.Options;

public class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    /// <summary>
    /// "local" guarda en <see cref="RutaFisica"/> y sirve por <see cref="RutaPublica"/>.
    /// "azure" sube a Azure Blob Storage usando <see cref="AzureConnectionString"/> y
    /// <see cref="AzureContainer"/>; en ese caso <see cref="RutaFisica"/> y
    /// <see cref="RutaPublica"/> quedan ignoradas.
    /// </summary>
    public string Provider { get; set; } = "local";

    /// <summary>
    /// Ruta física relativa al ContentRoot donde se guardan los archivos (solo Provider=local).
    /// En dev: "wwwroot/uploads/products". En producción se cambia por un bucket.
    /// </summary>
    public string RutaFisica { get; set; } = "wwwroot/uploads/products";

    /// <summary>
    /// Prefijo público de URL que devolvemos al cliente
    /// (lo que se guarda en Product.ImagenUrl) — solo Provider=local.
    /// </summary>
    public string RutaPublica { get; set; } = "/uploads/products";

    /// <summary>
    /// Tamaño máximo permitido por archivo (MB).
    /// </summary>
    public int TamanoMaximoMb { get; set; } = 5;

    /// <summary>
    /// Cadena de conexión de Azure Storage. Puede ser la real
    /// (<c>DefaultEndpointsProtocol=...</c>) o la de Azurite en dev
    /// (<c>UseDevelopmentStorage=true</c>).
    /// </summary>
    public string? AzureConnectionString { get; set; }

    /// <summary>Nombre del contenedor blob. Debe existir con acceso público a blobs.</summary>
    public string AzureContainer { get; set; } = "productos";

    /// <summary>
    /// Base URL pública opcional para servir los blobs (útil detrás de CDN).
    /// Si es null, usamos la URI que devuelve el BlobClient — que apunta al
    /// endpoint de la cuenta de storage.
    /// </summary>
    public string? AzurePublicBaseUrl { get; set; }
}
