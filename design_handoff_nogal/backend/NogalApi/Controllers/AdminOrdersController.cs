using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Orders;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IOrderService _pedidos;

    public AdminOrdersController(IOrderService pedidos)
    {
        _pedidos = pedidos;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 50,
        [FromQuery] string? estado = null)
    {
        var resultado = await _pedidos.ListarAsync(pagina, tamano, estado);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var pedido = await _pedidos.ObtenerAsync(id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] UpdateOrderStatusDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Estado))
        {
            return BadRequest(new { mensaje = "El estado es obligatorio." });
        }

        try
        {
            var actualizado = await _pedidos.CambiarEstadoAsync(id, dto.Estado);
            return actualizado is null ? NotFound() : Ok(actualizado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}
