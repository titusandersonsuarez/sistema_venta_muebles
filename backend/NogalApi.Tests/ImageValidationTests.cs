using NogalApi.Services;

namespace NogalApi.Tests;

public class ImageValidationTests
{
    private static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0x0D };
    private static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46, 0x49, 0x46, 0, 1 };

    [Fact]
    public async Task AceptaUnaImagenRealYDejaElStreamAlInicio()
    {
        using var stream = new MemoryStream(Png);

        var (extension, contentType) = await ImageValidation.ValidarAsync(stream, "Foto.PNG", "image/png", default);

        Assert.Equal(".png", extension);
        Assert.Equal("image/png", contentType);
        Assert.Equal(0, stream.Position);
    }

    [Theory]
    [InlineData("foto.exe", "application/octet-stream")]
    [InlineData("foto.png", "image/jpeg")]
    public async Task RechazaExtensionesOTiposQueNoCoinciden(string nombre, string contentType)
    {
        using var stream = new MemoryStream(Png);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ImageValidation.ValidarAsync(stream, nombre, contentType, default));
    }

    [Fact]
    public async Task RechazaUnArchivoQueSeHacePasarPorImagen()
    {
        // Un JPEG renombrado a .png, y un texto con extensión .jpg.
        using var jpegComoPng = new MemoryStream(Jpeg);
        using var texto = new MemoryStream("<script>alert(1)</script>"u8.ToArray());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ImageValidation.ValidarAsync(jpegComoPng, "foto.png", "image/png", default));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            ImageValidation.ValidarAsync(texto, "foto.jpg", "image/jpeg", default));
    }
}
