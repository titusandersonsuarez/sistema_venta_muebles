using NogalApi.Models;

namespace NogalApi.Services;

public sealed class UnavailableProduct3dGenerationService : IProduct3dGenerationService
{
    public Task<Product3dGenerationResult> GenerarAsync(Product producto, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "La generación 3D no está configurada. Define Product3d:Enabled, Provider, Endpoint y ApiKey, o carga manualmente una URL GLB.");
    }
}