using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NogalApi.Models.Products;
using NogalApi.Options;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productos;
    private readonly IProductImageStorage _almacenImagenes;
    private readonly ImageStorageOptions _imageOptions;

    public AdminProductsController(
        IProductService productos,
        IProductImageStorage almacenImagenes,
        IOptions<ImageStorageOptions> imageOptions)
    {
        _productos = productos;
        _almacenImagenes = almacenImagenes;
        _imageOptions = imageOptions.Value;
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
        if (Validar(dto.Nombre, dto.Categoria, dto.Material, dto.PrecioCOP, dto.Estado) is { } error)
        {
            return BadRequest(new { mensaje = error });
        }

        var creado = await _productos.CrearAsync(dto);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UpdateProductDto dto)
    {
        if (Validar(dto.Nombre, dto.Categoria, dto.Material, dto.PrecioCOP, dto.Estado) is { } error)
        {
            return BadRequest(new { mensaje = error });
        }

        var actualizado = await _productos.ActualizarAsync(id, dto);
        return actualizado is null ? NotFound() : Ok(actualizado);
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
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    private static string? Validar(string nombre, string categoria, string material, decimal precio, string? estado)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return "El nombre es obligatorio.";
        }
        if (precio <= 0)
        {
            return "El precio debe ser mayor a cero.";
        }
        if (!ProductCatalogo.Categorias.Contains(categoria))
        {
            return $"Categoría no válida. Usa una de: {string.Join(", ", ProductCatalogo.Categorias)}.";
        }
        if (!ProductCatalogo.Materiales.Contains(material))
        {
            return $"Material no válido. Usa uno de: {string.Join(", ", ProductCatalogo.Materiales)}.";
        }
        if (!string.IsNullOrWhiteSpace(estado) && !ProductCatalogo.Estados.Contains(estado))
        {
            return $"Estado no válido. Usa uno de: {string.Join(", ", ProductCatalogo.Estados)}.";
        }
        return null;
    }
}
