using Microsoft.Extensions.Options;
using NogalApi.Options;

namespace NogalApi.Services;

public class LocalProductImageStorage : IProductImageStorage
{
    private readonly ImageStorageOptions _options;
    private readonly IWebHostEnvironment _env;

    public LocalProductImageStorage(IOptions<ImageStorageOptions> options, IWebHostEnvironment env)
    {
        _options = options.Value;
        _env = env;
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal, string contentType, CancellationToken cancellationToken = default)
    {
        var (extension, _) = await ImageValidation.ValidarAsync(contenido, nombreOriginal, contentType, cancellationToken);

        var rutaAbsoluta = Path.Combine(_env.ContentRootPath, _options.RutaFisica);
        Directory.CreateDirectory(rutaAbsoluta);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension}";
        var rutaFinal = Path.Combine(rutaAbsoluta, nombreArchivo);

        await using (var destino = File.Create(rutaFinal))
        {
            await contenido.CopyToAsync(destino, cancellationToken);
        }

        var prefijo = _options.RutaPublica.TrimEnd('/');
        return $"{prefijo}/{nombreArchivo}";
    }
}
