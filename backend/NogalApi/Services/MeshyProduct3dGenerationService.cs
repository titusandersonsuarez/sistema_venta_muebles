using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Services;

public sealed class MeshyProduct3dGenerationService : IProduct3dGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly Product3dOptions _options;
    private readonly ILogger<MeshyProduct3dGenerationService> _logger;

    public MeshyProduct3dGenerationService(
        HttpClient httpClient,
        IOptions<Product3dOptions> options,
        ILogger<MeshyProduct3dGenerationService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Product3dGenerationResult> GenerarAsync(Product producto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Falta la clave de API de Meshy en la configuración (Product3d:ApiKey).");
        }

        if (string.IsNullOrWhiteSpace(producto.ImagenUrl))
        {
            throw new InvalidOperationException($"El producto '{producto.Nombre}' no tiene una imagen asociada para generar el modelo 3D.");
        }

        var imageUrl = ResolverUrlImagen(producto.ImagenUrl);
        var baseEndpoint = !string.IsNullOrWhiteSpace(_options.Endpoint)
            ? _options.Endpoint.TrimEnd('/')
            : "https://api.meshy.ai/v2/image-to-3d";

        _logger.LogInformation("Iniciando tarea Image-to-3D en Meshy para el producto {Id} ('{Nombre}'). Imagen: {ImageUrl}",
            producto.Id, producto.Nombre, imageUrl);

        using var request = new HttpRequestMessage(HttpMethod.Post, baseEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var requestBody = new
        {
            image_url = imageUrl,
            enable_pbr = true,
            surface_mode = "hard",
            ai_model = "meshy-4"
        };

        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Meshy API respondió con código {StatusCode}: {Response}", response.StatusCode, rawJson);
            throw new InvalidOperationException($"Error al crear tarea 3D en Meshy (HTTP {response.StatusCode}): {rawJson}");
        }

        using var doc = JsonDocument.Parse(rawJson);
        if (!doc.RootElement.TryGetProperty("result", out var resultProp))
        {
            throw new InvalidOperationException("La respuesta de Meshy no contiene el identificador de tarea 'result'.");
        }

        var taskId = resultProp.GetString();
        if (string.IsNullOrWhiteSpace(taskId))
        {
            throw new InvalidOperationException("El identificador de tarea recibido de Meshy está vacío.");
        }

        _logger.LogInformation("Tarea de Meshy creada con ID: {TaskId}. Iniciando sondeo de estado...", taskId);

        var taskUrl = $"{baseEndpoint}/{taskId}";
        var pollInterval = TimeSpan.FromSeconds(Math.Max(3, _options.PollingIntervalSeconds));
        var timeoutAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.MaxWaitMinutes));

        while (DateTime.UtcNow < timeoutAt && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(pollInterval, cancellationToken);

            using var pollRequest = new HttpRequestMessage(HttpMethod.Get, taskUrl);
            pollRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var pollResponse = await _httpClient.SendAsync(pollRequest, cancellationToken);
            if (!pollResponse.IsSuccessStatusCode)
            {
                continue;
            }

            var pollJson = await pollResponse.Content.ReadAsStringAsync(cancellationToken);
            using var pollDoc = JsonDocument.Parse(pollJson);
            var root = pollDoc.RootElement;

            var status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
            var progress = root.TryGetProperty("progress", out var progProp) ? progProp.GetInt32() : 0;

            _logger.LogDebug("Meshy tarea {TaskId} estado: {Status} (avance: {Progress}%)", taskId, status, progress);

            if (string.Equals(status, "SUCCEEDED", StringComparison.OrdinalIgnoreCase))
            {
                string? glbUrl = null;
                string? usdzUrl = null;

                if (root.TryGetProperty("model_urls", out var modelUrls))
                {
                    if (modelUrls.TryGetProperty("glb", out var glbProp))
                    {
                        glbUrl = glbProp.GetString();
                    }
                    if (modelUrls.TryGetProperty("usdz", out var usdzProp))
                    {
                        usdzUrl = usdzProp.GetString();
                    }
                }

                if (string.IsNullOrWhiteSpace(glbUrl))
                {
                    throw new InvalidOperationException("Meshy completó la tarea pero no devolvió la URL del archivo GLB.");
                }

                _logger.LogInformation("Modelo 3D generado con éxito por Meshy. GLB: {GlbUrl}", glbUrl);
                return new Product3dGenerationResult(glbUrl, usdzUrl);
            }

            if (string.Equals(status, "FAILED", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "EXPIRED", StringComparison.OrdinalIgnoreCase))
            {
                var errorMsg = "Desconocido";
                if (root.TryGetProperty("task_error", out var errorProp) &&
                    errorProp.TryGetProperty("message", out var msgProp))
                {
                    errorMsg = msgProp.GetString() ?? errorMsg;
                }

                throw new InvalidOperationException($"La generación 3D en Meshy falló con estado '{status}': {errorMsg}");
            }
        }

        throw new TimeoutException($"Se agotó el tiempo de espera ({_options.MaxWaitMinutes} min) esperando la finalización del modelo 3D en Meshy.");
    }

    private string ResolverUrlImagen(string imagenUrl)
    {
        if (Uri.TryCreate(imagenUrl, UriKind.Absolute, out var uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return imagenUrl;
        }

        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
        {
            var baseUri = _options.PublicBaseUrl.TrimEnd('/');
            var relative = imagenUrl.TrimStart('/');
            return $"{baseUri}/{relative}";
        }

        // Si es una ruta local relativa y no hay base pública, lanzamos una advertencia guiada
        throw new InvalidOperationException(
            $"La imagen '{imagenUrl}' es local. Para que un servicio en la nube como Meshy pueda procesarla, " +
            "debe ser una URL pública (https://...) o debes configurar 'Product3d:PublicBaseUrl' con tu dominio público o Ngrok.");
    }
}
