using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NogalApi.Models.Products;
using NogalApi.Options;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productos;
    private readonly IProductImageStorage _almacenImagenes;
    private readonly IProduct3dGenerationQueue _colaModelos3d;
    private readonly ImageStorageOptions _imageOptions;

    public AdminProductsController(
        IProductService productos,
        IProductImageStorage almacenImagenes,
        IOptions<ImageStorageOptions> imageOptions,
        IProduct3dGenerationQueue colaModelos3d)
    {
        _productos = productos;
        _almacenImagenes = almacenImagenes;
        _imageOptions = imageOptions.Value;
        _colaModelos3d = colaModelos3d;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20,
        [FromQuery] bool incluirInactivos = true)
    {
        var resultado = await _productos.ListarAdminAsync(pagina, tamano, incluirInactivos);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var producto = await _productos.ObtenerAdminAsync(id);
        return producto is null ? NotFound() : Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CreateProductDto dto)
    {
        try
        {
            var creado = await _productos.CrearAsync(dto);
            return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
        }
        catch (ProductValidationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            var actualizado = await _productos.ActualizarAsync(id, dto);
            return actualizado is null ? NotFound() : Ok(actualizado);
        }
        catch (ProductValidationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await _productos.EliminarAsync(id);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/restore")]
    public async Task<IActionResult> Restaurar(int id)
    {
        var restaurado = await _productos.RestaurarAsync(id);
        return restaurado is null ? NotFound() : Ok(restaurado);
    }

    [HttpPost("{id:int}/image")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    public async Task<IActionResult> SubirImagen(int id, IFormFile archivo, CancellationToken cancellationToken)
    {
        if (archivo is null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "No se recibió ninguna imagen." });
        }

        var maxBytes = _imageOptions.TamanoMaximoMb * 1024L * 1024L;
        if (archivo.Length > maxBytes)
        {
            return BadRequest(new { mensaje = $"La imagen supera el tamaño máximo de {_imageOptions.TamanoMaximoMb} MB." });
        }

        var existente = await _productos.ObtenerAdminAsync(id);
        if (existente is null)
        {
            return NotFound();
        }

        string url;
        try
        {
            await using var stream = archivo.OpenReadStream();
            url = await _almacenImagenes.GuardarAsync(stream, archivo.FileName, archivo.ContentType, cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }

        var actualizado = await _productos.AsignarImagenUrlAsync(id, url);
        if (actualizado is not null)
        {
            await _colaModelos3d.EnqueueAsync(id, cancellationToken);
        }
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpPost("{id:int}/3d-generation")]
    public async Task<IActionResult> GenerarModelo3d(int id, CancellationToken cancellationToken)
    {
        try
        {
            var producto = await _productos.SolicitarModelo3dAsync(id);
            if (producto is null) return NotFound();
            await _colaModelos3d.EnqueueAsync(id, cancellationToken);
            return AcceptedAtAction(nameof(Obtener), new { id }, producto);
        }
        catch (ProductValidationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

}
