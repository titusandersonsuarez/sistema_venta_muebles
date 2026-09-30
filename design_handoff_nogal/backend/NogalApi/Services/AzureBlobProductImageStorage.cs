using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using NogalApi.Options;

namespace NogalApi.Services;

/// <summary>
/// Sube imágenes de producto a un contenedor blob público en Azure Storage.
/// El contenedor se asegura que existe con acceso <c>Blob</c> (lectura pública
/// de blobs individuales, no listado del contenedor) al primer arranque.
/// </summary>
public class AzureBlobProductImageStorage : IProductImageStorage
{
    private readonly BlobContainerClient _container;
    private readonly ImageStorageOptions _options;
    private readonly ILogger<AzureBlobProductImageStorage> _logger;
    private bool _contenedorAsegurado;

    public AzureBlobProductImageStorage(IOptions<ImageStorageOptions> options, ILogger<AzureBlobProductImageStorage> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_options.AzureConnectionString))
        {
            throw new InvalidOperationException("Falta 'ImageStorage:AzureConnectionString' con Provider=azure.");
        }
        if (string.IsNullOrWhiteSpace(_options.AzureContainer))
        {
            throw new InvalidOperationException("Falta 'ImageStorage:AzureContainer' con Provider=azure.");
        }

        _container = new BlobContainerClient(_options.AzureConnectionString, _options.AzureContainer);
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal, string contentType, CancellationToken cancellationToken = default)
    {
        var (extension, tipoValidado) = await ImageValidation.ValidarAsync(contenido, nombreOriginal, contentType, cancellationToken);

        await AsegurarContenedorAsync(cancellationToken);

        var nombreBlob = $"{Guid.NewGuid():N}{extension}";
        var blob = _container.GetBlobClient(nombreBlob);

        await blob.UploadAsync(contenido, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = tipoValidado }
        }, cancellationToken);

        // Si el usuario configuró un dominio propio (CDN), usamos ese.
        // Si no, la URI del blob apunta directo al endpoint de la cuenta,
        // que ya es la URL pública porque el contenedor es PublicAccessType.Blob.
        if (!string.IsNullOrWhiteSpace(_options.AzurePublicBaseUrl))
        {
            var baseUrl = _options.AzurePublicBaseUrl.TrimEnd('/');
            return $"{baseUrl}/{nombreBlob}";
        }
        return blob.Uri.ToString();
    }

    private async Task AsegurarContenedorAsync(CancellationToken cancellationToken)
    {
        if (_contenedorAsegurado) return;

        try
        {
            await _container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);
            _contenedorAsegurado = true;
        }
        catch (Exception ex)
        {
            // Loguear y dejar que reintente en la próxima subida — no queremos
            // que un problema temporal deje al servicio caído para siempre.
            _logger.LogWarning(ex, "No se pudo asegurar el contenedor blob {Container}.", _options.AzureContainer);
            throw;
        }
    }
}
