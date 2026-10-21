using NogalApi.Models.Reviews;
using NogalApi.Services;

namespace NogalApi.Tests;

public class ReviewServiceTests
{
    private static CreateReviewDto Resena(int rating = 5, string texto = "Excelente calidad, llegó a tiempo y bien empacado.")
        => new() { Autor = "Laura Gómez", Rating = rating, Texto = texto };

    [Fact]
    public async Task UnaResenaNuevaNoSeVeHastaQueElAdminLaApruebe()
    {
        using var db = TestDb.Create();
        var sofa = TestDb.AddProduct(db, "Sofa Lino", 1_200_000m);
        var service = new ReviewService(db);

        Assert.True(await service.CrearParaSlugAsync(sofa.Slug, Resena()));
        Assert.Empty(await service.ListarPorSlugAsync(sofa.Slug));

        var pendientes = await service.ListarAdminAsync(aprobado: false, pagina: 1, tamano: 10);
        var pendiente = Assert.Single(pendientes.Items);
        await service.AprobarAsync(pendiente.Id);

        var publicas = await service.ListarPorSlugAsync(sofa.Slug);
        Assert.Equal("Laura Gómez", Assert.Single(publicas).Autor);
    }

    [Theory]
    [InlineData(0, "Excelente calidad, llegó a tiempo y bien empacado.")]
    [InlineData(6, "Excelente calidad, llegó a tiempo y bien empacado.")]
    [InlineData(5, "Muy bueno")]
    public async Task ValidaCalificacionYLongitudDelTexto(int rating, string texto)
    {
        using var db = TestDb.Create();
        var sofa = TestDb.AddProduct(db, "Sofa Lino", 1_200_000m);
        var service = new ReviewService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CrearParaSlugAsync(sofa.Slug, Resena(rating, texto)));
    }

    [Fact]
    public async Task NoSePuedeResenarUnProductoInactivo()
    {
        using var db = TestDb.Create();
        var viejo = TestDb.AddProduct(db, "Cama Vieja", 700_000m, activo: false);

        Assert.False(await new ReviewService(db).CrearParaSlugAsync(viejo.Slug, Resena()));
    }
}
