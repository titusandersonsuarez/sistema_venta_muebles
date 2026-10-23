using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using NogalApi.Models.Orders;
using NogalApi.Options;
using NogalApi.Services;

namespace NogalApi.Controllers;

/// <summary>
/// Endpoints públicos del carrito de la tienda. Sin autenticación:
/// el cliente confirma un pedido y recibe el código para consultarlo.
/// </summary>
[ApiController]
[Route("api/orders")]
[EnableRateLimiting("publico-lectura")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _pedidos;
    private readonly WompiOptions _wompiOptions;
    private readonly IWebHostEnvironment _env;

    public OrdersController(IOrderService pedidos, IOptions<WompiOptions> wompiOptions, IWebHostEnvironment env)
    {
        _pedidos = pedidos;
        _wompiOptions = wompiOptions.Value;
        _env = env;
    }

    [HttpPost]
    [EnableRateLimiting("publico-escritura")]
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
        return pedido is null ? NotFound() : Ok(VistaPublica(pedido));
    }

    /// <summary>
    /// Quien conoce el código no necesariamente es el cliente (enlaces
    /// reenviados, historial del navegador, capturas). Ocultamos el dato de
    /// contacto completo en las respuestas públicas; el panel admin lo ve entero.
    /// </summary>
    private static OrderDto VistaPublica(OrderDto pedido)
    {
        pedido.Contacto = EnmascararContacto(pedido.Contacto);
        pedido.PagoTransaccionId = null;
        return pedido;
    }

    private static string? EnmascararContacto(string? contacto)
    {
        if (string.IsNullOrWhiteSpace(contacto)) return contacto;
        var arroba = contacto.IndexOf('@');
        if (arroba > 0)
        {
            return $"{contacto[0]}***{contacto[arroba..]}";
        }
        return contacto.Length <= 4 ? "***" : $"***{contacto[^4..]}";
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
        // Este endpoint permite al propio cliente marcar su pedido como
        // pagado. Fuera de Development (o sin AllowDemo explícito) no existe.
        if (!PaymentServiceDispatcher.DemoPermitido(_wompiOptions, _env))
        {
            return NotFound();
        }

        try
        {
            var pedido = await _pedidos.ConfirmarPagoDemoAsync(codigo, dto.Aprobado);
            return pedido is null ? NotFound() : Ok(VistaPublica(pedido));
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
        return pedido is null ? NotFound() : Ok(VistaPublica(pedido));
    }
}
