using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using NogalApi.Options;

namespace NogalApi.Services;

public class GeminiAiService : IGeminiAiService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAiService> _logger;

    public GeminiAiService(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        ILogger<GeminiAiService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> GenerarRespuestaAsync(
        string mensajeUsuario,
        string contextoCatalogo,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var apiKey = _options.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Intenta leer de variable de entorno GEMINI_API_KEY
            apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogInformation("Gemini API Key no configurada. Usando motor de respuestas de respaldo.");
            return null;
        }

        var modelo = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-1.5-flash" : _options.Model.Trim();
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent?key={apiKey}";

        var promptSistema = """
            Eres Nogalito, el asistente virtual y maestro ebanista de Nogal Muebles, un taller y fábrica de muebles artesanales ubicado en Puente Aranda, Bogotá, Colombia.
            Tu misión es asesorar a los clientes de manera cordial, profesional, cálida, colombiana y experta.
            Conoces a la perfección las maderas macizas de alta gama (roble, nogal, cedro, flor morado), tapizados en lino y cueros, acabados y recomendaciones de cuidado.

            Pautas clave de respuesta:
            1. Sé conciso, elegante y directo (máximo 2 a 3 párrafos cortos).
            2. Habla siempre en pesos colombianos ($ COP).
            3. Si el cliente pregunta por productos específicos, precios, medidas o materiales, consulta y cita la información del catálogo del taller que se te proporciona más abajo.
            4. Si te preguntan por medidas especiales, explícales con entusiasmo que como somos fábrica propia en Bogotá, podemos adaptar dimensiones bajo pedido.
            5. Si no sabes un dato específico, indícales amablemente que pueden hablar directamente con nuestros asesores de taller al WhatsApp 300 000 0000.

            [DATOS INSTITUCIONALES DEL TALLER NOGAL]
            - Taller y showroom de fábrica: Cra. 56 #17-40, Puente Aranda, Bogotá.
            - Horario de atención: Lunes a viernes de 8:00 a 17:00, sábados de 8:00 a 12:00.
            - Garantía: 10 años de garantía de fábrica por estructura y ensamble artesanal.
            - Envíos: Envío gratis en Bogotá y municipios aledaños en compras desde $ 500.000 COP. Despachos nacionales coordinados con transportadoras especializadas en muebles.
            - Tiempos de fabricación y despacho: 5 a 8 días hábiles porque fabricamos en taller propio sin intermediarios.
            - Servicio de armado: Opcional por $ 89.000 COP al momento de la entrega en Bogotá.
            - Métodos de pago: Tarjetas de crédito/débito, PSE, Addi (cuotas sin interés) y Sistecrédito.
            - Experiencia 3D/AR: Contamos con realidad aumentada en la tienda para proyectar los muebles en la sala usando la cámara del celular.
            """;

        var payload = new
        {
            systemInstruction = new
            {
                parts = new[] { new { text = promptSistema } }
            },
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new
                        {
                            text = $"[INFORMACIÓN ACTUALIZADA DEL CATÁLOGO DEL TALLER]:\n{contextoCatalogo}\n\n[PREGUNTA DEL CLIENTE]:\n{mensajeUsuario}"
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.5,
                maxOutputTokens = 600
            }
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(7)); // Timeout rápido para no demorar la UI

            var response = await _httpClient.PostAsJsonAsync(url, payload, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogWarning("Error al invocar Gemini API ({StatusCode}): {Error}", response.StatusCode, errorBody);
                return null;
            }

            var geminiResponse = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(cancellationToken: cts.Token);
            var respuestaTexto = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (!string.IsNullOrWhiteSpace(respuestaTexto))
            {
                return respuestaTexto.Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Excepción durante la llamada a Gemini API. Se recurrirá a respuestas locales.");
        }

        return null;
    }

    private class GeminiApiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }

    private class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
