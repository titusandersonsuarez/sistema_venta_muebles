using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Orders;
using NogalApi.Services;

namespace NogalApi.Controllers;

/// <summary>
/// Endpoints públicos del carrito de la tienda. Sin autenticación:
/// el cliente confirma un pedido y recibe el código para consultarlo.
/// </summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _pedidos;

    public OrdersController(IOrderService pedidos)
    {
        _pedidos = pedidos;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CreateOrderDto dto)
    {
        try
        {
            var creado = await _pedidos.CrearAsync(dto);
            return CreatedAtAction(nameof(ObtenerPorCodigo), new { codigo = creado.Codigo }, creado);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> ObtenerPorCodigo(string codigo)
    {
        var pedido = await _pedidos.ObtenerPorCodigoAsync(codigo);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    /// <summary>
    /// Genera el enlace de checkout (Wompi u pantalla demo) para pagar
    /// el pedido. El cliente lo abre y termina el pago fuera del sitio.
    /// </summary>
    [HttpPost("{codigo}/pago")]
    public async Task<IActionResult> IniciarPago(string codigo)
    {
        try
        {
            var intencion = await _pedidos.IniciarPagoAsync(codigo);
            return intencion is null ? NotFound() : Ok(intencion);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Callback de la pantalla demo. Solo válido si el pedido fue
    /// iniciado en modo demo.
    /// </summary>
    [HttpPost("{codigo}/pago/demo")]
    public async Task<IActionResult> ConfirmarPagoDemo(string codigo, [FromBody] ConfirmarPagoDemoDto dto)
    {
        try
        {
            var pedido = await _pedidos.ConfirmarPagoDemoAsync(codigo, dto.Aprobado);
            return pedido is null ? NotFound() : Ok(pedido);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    /// <summary>
    /// Consulta a Wompi el estado real de la transacción cuando el
    /// cliente vuelve del checkout (query ?id=&amp;env= en la URL).
    /// </summary>
    [HttpPost("{codigo}/pago/verificar")]
    public async Task<IActionResult> VerificarPago(string codigo, [FromQuery] string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
        {
            return BadRequest(new { mensaje = "transactionId es obligatorio." });
        }
        var pedido = await _pedidos.VerificarPagoAsync(codigo, transactionId);
        return pedido is null ? NotFound() : Ok(pedido);
    }
}
