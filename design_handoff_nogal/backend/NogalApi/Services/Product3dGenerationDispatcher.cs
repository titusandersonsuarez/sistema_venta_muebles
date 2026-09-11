using Microsoft.Extensions.Options;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

public sealed class Product3dGenerationDispatcher : IProduct3dGenerationService
{
    private readonly MeshyProduct3dGenerationService _meshyService;
    private readonly UnavailableProduct3dGenerationService _unavailableService;
    private readonly Product3dOptions _options;
    private readonly ILogger<Product3dGenerationDispatcher> _logger;

    public Product3dGenerationDispatcher(
        MeshyProduct3dGenerationService meshyService,
        UnavailableProduct3dGenerationService unavailableService,
        IOptions<Product3dOptions> options,
        ILogger<Product3dGenerationDispatcher> logger)
    {
        _meshyService = meshyService;
        _unavailableService = unavailableService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Product3dGenerationResult> GenerarAsync(Product producto, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return await _unavailableService.GenerarAsync(producto, cancellationToken);
        }

        var provider = (_options.Provider ?? "demo").ToLowerInvariant().Trim();

        if (provider == "meshy")
        {
            try
            {
                return await _meshyService.GenerarAsync(producto, cancellationToken);
            }
            catch (Exception ex) when (_options.FallbackToDemoOnFailure)
            {
                _logger.LogWarning(ex, "El servicio Meshy no pudo procesar el producto {Id}. Aplicando modelo 3D demostrativo de taller...", producto.Id);
                return ObtenerModeloDemostrativo(producto);
            }
        }

        if (provider == "demo" || provider == "simulated")
        {
            _logger.LogInformation("Generando modelo 3D en modo demostrativo/taller para producto {Id} ({Categoria})", producto.Id, producto.Categoria);
            // Breve retardo para simular procesamiento en background realista
            await Task.Delay(1500, cancellationToken);
            return ObtenerModeloDemostrativo(producto);
        }

        return await _unavailableService.GenerarAsync(producto, cancellationToken);
    }

    private Product3dGenerationResult ObtenerModeloDemostrativo(Product producto)
    {
        var cat = (producto.Categoria ?? "").ToLowerInvariant();

        // Modelos glTF/GLB oficiales de alta fidelidad optimizados para WebXR y QuickLook AR
        if (cat.Contains("silla") || cat.Contains("poltrona") || cat.Contains("banco"))
        {
            return new Product3dGenerationResult(
                Modelo3dUrl: "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/SheenChair/glTF-Binary/SheenChair.glb",
                ModeloUsdzUrl: "https://modelviewer.dev/shared-assets/models/Chair.usdz"
            );
        }

        if (cat.Contains("sofa") || cat.Contains("sofá") || cat.Contains("sala"))
        {
            return new Product3dGenerationResult(
                Modelo3dUrl: "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/SheenChair/glTF-Binary/SheenChair.glb",
                ModeloUsdzUrl: "https://modelviewer.dev/shared-assets/models/Chair.usdz"
            );
        }

        // Predeterminado para mesas, camas y consolas
        return new Product3dGenerationResult(
            Modelo3dUrl: "https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Assets/main/Models/SheenChair/glTF-Binary/SheenChair.glb",
            ModeloUsdzUrl: "https://modelviewer.dev/shared-assets/models/Chair.usdz"
        );
    }
}
