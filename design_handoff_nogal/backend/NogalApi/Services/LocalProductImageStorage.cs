using Microsoft.Extensions.Options;
using NogalApi.Options;

namespace NogalApi.Services;

public class LocalProductImageStorage : IProductImageStorage
{
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private static readonly IReadOnlyDictionary<string, string> TiposPermitidos =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };

    private readonly ImageStorageOptions _options;
    private readonly IWebHostEnvironment _env;

    public LocalProductImageStorage(IOptions<ImageStorageOptions> options, IWebHostEnvironment env)
    {
        _options = options.Value;
        _env = env;
    }

    public async Task<string> GuardarAsync(Stream contenido, string nombreOriginal, string contentType, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(nombreOriginal);
        if (string.IsNullOrWhiteSpace(extension) || !ExtensionesPermitidas.Contains(extension))
        {
            throw new InvalidOperationException("Formato de imagen no soportado. Usa JPG, PNG, WEBP o GIF.");
        }

        if (!TiposPermitidos.TryGetValue(extension, out var tipoEsperado) ||
            !string.Equals(contentType, tipoEsperado, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El tipo de contenido de la imagen no coincide con su extensión.");
        }

        if (!await TieneFirmaValidaAsync(contenido, extension, cancellationToken))
        {
            throw new InvalidOperationException("El contenido recibido no es una imagen válida.");
        }

        var rutaAbsoluta = Path.Combine(_env.ContentRootPath, _options.RutaFisica);
        Directory.CreateDirectory(rutaAbsoluta);

        var nombreArchivo = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var rutaFinal = Path.Combine(rutaAbsoluta, nombreArchivo);

        await using (var destino = File.Create(rutaFinal))
        {
            await contenido.CopyToAsync(destino, cancellationToken);
        }

        var prefijo = _options.RutaPublica.TrimEnd('/');
        return $"{prefijo}/{nombreArchivo}";
    }

    private static async Task<bool> TieneFirmaValidaAsync(Stream contenido, string extension, CancellationToken cancellationToken)
    {
        if (!contenido.CanSeek)
        {
            return false;
        }

        var posicionInicial = contenido.Position;
        var encabezado = new byte[12];
        var leidos = 0;
        while (leidos < encabezado.Length)
        {
            var cantidad = await contenido.ReadAsync(encabezado.AsMemory(leidos), cancellationToken);
            if (cantidad == 0)
            {
                break;
            }

            leidos += cantidad;
        }

        contenido.Position = posicionInicial;

        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => leidos >= 3 && encabezado[0] == 0xFF && encabezado[1] == 0xD8 && encabezado[2] == 0xFF,
            ".png" => leidos >= 8 && encabezado.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => leidos >= 6 && encabezado.AsSpan(0, 6).SequenceEqual("GIF87a"u8) ||
                leidos >= 6 && encabezado.AsSpan(0, 6).SequenceEqual("GIF89a"u8),
            ".webp" => leidos >= 12 && encabezado.AsSpan(0, 4).SequenceEqual("RIFF"u8) && encabezado.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
