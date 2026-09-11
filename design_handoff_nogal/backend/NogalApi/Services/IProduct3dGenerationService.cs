using NogalApi.Models;

namespace NogalApi.Services;

public sealed record Product3dGenerationResult(string Modelo3dUrl, string? ModeloUsdzUrl = null);

public interface IProduct3dGenerationService
{
    Task<Product3dGenerationResult> GenerarAsync(Product producto, CancellationToken cancellationToken = default);
}