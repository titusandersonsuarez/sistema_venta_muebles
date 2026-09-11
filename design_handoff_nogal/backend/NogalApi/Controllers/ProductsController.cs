using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Products;
using NogalApi.Services;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/products")]
[AllowAnonymous]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productos;

    public ProductsController(IProductService productos)
    {
        _productos = productos;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? categoria,
        [FromQuery] string? material,
        [FromQuery] decimal? precioMax,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamano = 20)
    {
        var filtros = new ProductFilters
        {
            Categoria = categoria,
            Material = material,
            PrecioMax = precioMax,
            Pagina = pagina,
            Tamano = tamano
        };
        var resultado = await _productos.ListarPublicoAsync(filtros);
        return Ok(resultado);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Obtener(string slug)
    {
        var producto = await _productos.ObtenerPorSlugAsync(slug);
        return producto is null ? NotFound() : Ok(producto);
    }
}
