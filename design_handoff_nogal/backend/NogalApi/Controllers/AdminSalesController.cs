using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/admin/sales")]
[Authorize(Roles = "Admin")]
public class AdminSalesController : ControllerBase
{
    private readonly ISalesService _ventas;

    public AdminSalesController(ISalesService ventas)
    {
        _ventas = ventas;
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string granularity = "day",
        CancellationToken cancellationToken = default)
    {
        if (!DateOnly.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaDesde) ||
            !DateOnly.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaHasta))
        {
            return BadRequest(new { mensaje = "from y to son obligatorios y deben usar el formato yyyy-MM-dd." });
        }

        try
        {
            return Ok(await _ventas.ObtenerAsync(fechaDesde, fechaHasta, granularity, cancellationToken));
        }
        catch (SalesQueryException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}