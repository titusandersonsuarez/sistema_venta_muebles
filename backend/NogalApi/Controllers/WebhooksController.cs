using Microsoft.AspNetCore.Mvc;
using NogalApi.Services;

namespace NogalApi.Controllers;

/// <summary>
/// Recibe callbacks asíncronos de pasarelas de pago. No usa auth JWT:
/// las requests vienen del servidor de la pasarela y se autentican por
/// firma en el body (Wompi) o header (otras).
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly IWompiWebhookService _wompi;

    public WebhooksController(IWompiWebhookService wompi)
    {
        _wompi = wompi;
    }

    /// <summary>
    /// Endpoint público de webhooks de Wompi. Devuelve siempre 200 cuando
    /// el evento fue aceptado (aún duplicado), 400 si el payload no se
    /// puede parsear, 401 si la firma no valida, y 503 si falta la
    /// configuración del secreto de eventos. Wompi reintenta ante no-2xx.
    /// </summary>
    [HttpPost("wompi")]
    // Los eventos de Wompi pesan unos pocos KB; sin límite Kestrel acepta 30 MB.
    [RequestSizeLimit(64 * 1024)]
    public async Task<IActionResult> Wompi(CancellationToken cancellationToken)
    {
        // Leemos el body como texto crudo — la firma se calcula sobre los
        // valores exactos que mandó Wompi, no sobre la re-serialización
        // de un DTO deserializado por ASP.NET.
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var resultado = await _wompi.ProcesarAsync(body, cancellationToken);
        return resultado.Codigo switch
        {
            WebhookResultadoCodigo.Ok => Ok(new { mensaje = resultado.Detalle ?? "Evento aceptado." }),
            WebhookResultadoCodigo.PayloadInvalido => BadRequest(new { mensaje = resultado.Detalle }),
            WebhookResultadoCodigo.FirmaInvalida => Unauthorized(new { mensaje = "Firma inválida." }),
            WebhookResultadoCodigo.NoConfigurado => StatusCode(503, new { mensaje = "Webhook no configurado." }),
            _ => StatusCode(500)
        };
    }
}
