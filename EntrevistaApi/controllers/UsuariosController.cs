using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EntrevistaApi.Models;
using EntrevistaApi.Data;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
     private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

[HttpGet]
public async Task<IActionResult> GetUsuarios()
{
    var usuarios = await _service.GetUsuariosAsync();

    return Ok(usuarios);
}

[HttpGet("activos")]
public async Task<IActionResult> GetUsuariosActivos()
{
    var usuarios = await _service.GetUsuariosActivosAsync();

    return Ok(usuarios);
}

[HttpGet("{id}")]
public async Task<IActionResult> GetUsuario(int id)
{
    var usuario = await _service.GetUsuarioByIdAsync(id);

    if (usuario == null)
    {
        return NotFound();
    }

    return Ok(usuario);
}


[HttpPost]
public async Task<IActionResult> CrearUsuarios([FromBody] Usuario usuario)
{
    var createdUsuario = await _service.CreateUsuarioAsync(usuario);
    return CreatedAtAction(nameof(GetUsuario), new { id = createdUsuario.Id }, createdUsuario);
}
[HttpPut("{id}")]
public async Task<IActionResult> ActualizarUsuario(
    int id,
    [FromBody] Usuario usuario)
{
    if (id != usuario.Id)
    {
        return BadRequest();
    }

    var usuarioExistente = await _service.GetUsuarioByIdAsync(id);

    if (usuarioExistente == null)
    {
        return NotFound();
    }

    usuarioExistente.Name = usuario.Name;
    usuarioExistente.Email = usuario.Email;
    usuarioExistente.Activo = usuario.Activo;

    await _service.UpdateUsuarioAsync(id, usuarioExistente);

    return Ok(usuarioExistente);
}

[HttpDelete("{id}")]
public async Task<IActionResult> EliminarUsuario(int id)
{
    var usuario = await _service.GetUsuarioByIdAsync(id);

    if (usuario == null)
    {
        return NotFound();
    }

    await _service.DeleteUsuarioAsync(id);

    return NoContent();
}
}
