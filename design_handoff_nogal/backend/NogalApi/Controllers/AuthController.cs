using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Auth;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { mensaje = "Usuario y contraseña son obligatorios." });
        }

        var resultado = await _authService.LoginAsync(request.NombreUsuario.Trim(), request.Password);

        if (resultado == null)
        {
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });
        }

        return Ok(resultado);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            nombreUsuario = User.Identity?.Name,
            nombre = User.FindFirstValue("nombre"),
            rol = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // El JWT es sin estado: cerrar sesión es responsabilidad del cliente
        // (borrar el token guardado). Este endpoint existe para que el front
        // tenga un lugar donde, más adelante, se pueda invalidar el token
        // (ej. lista negra) sin cambiar el contrato.
        return NoContent();
    }
}
