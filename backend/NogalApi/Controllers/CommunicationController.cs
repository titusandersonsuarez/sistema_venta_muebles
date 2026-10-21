using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Communication;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api")]
public class CommunicationController : ControllerBase
{
    private readonly ICommunicationService _communication;
    public CommunicationController(ICommunicationService communication) => _communication = communication;

    [HttpPost("contact")]
    public async Task<IActionResult> Contacto(CreateContactMessageDto dto, CancellationToken cancellationToken)
    {
        try { await _communication.CrearContactoAsync(dto, cancellationToken); return StatusCode(StatusCodes.Status201Created); }
        catch (CommunicationValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat(ChatRequestDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await _communication.ResponderChatAsync(dto, cancellationToken)); }
        catch (CommunicationValidationException ex) { return BadRequest(new { mensaje = ex.Message }); }
    }
}
