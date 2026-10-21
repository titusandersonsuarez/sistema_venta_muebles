namespace NogalApi.Services;

/// <summary>
/// Reglas compartidas por todos los <see cref="IProductImageStorage"/>:
/// qué extensiones aceptamos, qué content-type corresponde a cada una y
/// cómo validar que el binario sea realmente una imagen (magic bytes).
/// Extraído para no duplicar el chequeo en Local y Azure.
/// </summary>
public static class ImageValidation
{
    public static readonly IReadOnlyDictionary<string, string> TiposPermitidos =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };

    /// <summary>
    /// Valida extensión + content-type + firma binaria. Al terminar deja
    /// el stream posicionado al inicio para que el llamador pueda
    /// escribirlo tal cual al destino.
    /// </summary>
    public static async Task<(string extension, string contentType)> ValidarAsync(
        Stream contenido, string nombreOriginal, string contentType, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(nombreOriginal);
        if (string.IsNullOrWhiteSpace(extension) || !TiposPermitidos.ContainsKey(extension))
        {
            throw new InvalidOperationException("Formato de imagen no soportado. Usa JPG, PNG, WEBP o GIF.");
        }

        var tipoEsperado = TiposPermitidos[extension];
        if (!string.Equals(contentType, tipoEsperado, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("El tipo de contenido de la imagen no coincide con su extensión.");
        }

        if (!await TieneFirmaValidaAsync(contenido, extension, cancellationToken))
        {
            throw new InvalidOperationException("El contenido recibido no es una imagen válida.");
        }

        return (extension.ToLowerInvariant(), tipoEsperado);
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
