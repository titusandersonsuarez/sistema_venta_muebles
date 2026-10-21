using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminProductionController : ControllerBase
{
    private readonly IProductionService _produccion;

    public AdminProductionController(IProductionService produccion) => _produccion = produccion;

    [HttpGet("production")]
    public async Task<IActionResult> Obtener(CancellationToken cancellationToken) =>
        Ok(await _produccion.ObtenerResumenAsync(cancellationToken));

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventario(CancellationToken cancellationToken) =>
        Ok(await _produccion.ListarInventarioAsync(cancellationToken));
}
