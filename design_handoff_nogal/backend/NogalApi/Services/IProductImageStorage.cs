namespace NogalApi.Services;

public interface IProductImageStorage
{
    /// <summary>
    /// Guarda un archivo de imagen y devuelve la URL pública (relativa o absoluta)
    /// que se debe persistir en Product.ImagenUrl.
    /// </summary>
    Task<string> GuardarAsync(Stream contenido, string nombreOriginal, string contentType, CancellationToken cancellationToken = default);
}
