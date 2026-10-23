using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NogalApi.Models.Reviews;
using NogalApi.Services;

namespace NogalApi.Controllers;

/// <summary>
/// Endpoints públicos de reseñas por producto. Se anidan bajo el slug
/// del producto (mismo patrón que /api/products/{slug}) para que el
/// frontend no tenga que resolver el id primero.
/// </summary>
[ApiController]
[Route("api/products/{slug}/reviews")]
[AllowAnonymous]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public ReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(string slug, CancellationToken cancellationToken)
    {
        var lista = await _reviews.ListarPorSlugAsync(slug, cancellationToken);
        return Ok(lista);
    }

    [HttpPost]
    [EnableRateLimiting("publico-escritura")]
    public async Task<IActionResult> Crear(string slug, [FromBody] CreateReviewDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var creado = await _reviews.CrearParaSlugAsync(slug, dto, cancellationToken);
            if (!creado) return NotFound(new { mensaje = "El producto no existe o ya no está disponible." });
            return Accepted(new ReviewSubmittedDto
            {
                Mensaje = "¡Gracias! Tu reseña está en revisión y aparecerá cuando la aprobemos."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
