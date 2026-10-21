using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Services;

namespace NogalApi.Controllers;

/// <summary>
/// Panel de moderación de reseñas. Solo rol Admin puede aprobar o eliminar
/// (mismo criterio que ventas y catálogo del panel).
/// </summary>
[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewsController : ControllerBase
{
    private readonly IReviewService _reviews;

    public AdminReviewsController(IReviewService reviews)
    {
        _reviews = reviews;
    }

    /// <summary>
    /// Lista paginada. <c>estado=pendiente|aprobado</c> filtra la vista;
    /// omitir el parámetro devuelve todas.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        bool? aprobado = estado?.ToLowerInvariant() switch
        {
            "pendiente" => false,
            "aprobado" => true,
            null or "" => null,
            _ => throw new InvalidOperationException()
        };
        var lista = await _reviews.ListarAdminAsync(aprobado, pagina, tamano);
        return Ok(lista);
    }

    [HttpPost("{id:int}/aprobar")]
    public async Task<IActionResult> Aprobar(int id)
    {
        var review = await _reviews.AprobarAsync(id);
        return review is null ? NotFound() : Ok(review);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _reviews.EliminarAsync(id);
        return eliminado ? NoContent() : NotFound();
    }
}
