using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NogalApi.Models.Orders;
using NogalApi.Models.Products;

namespace NogalApi.Controllers;

[ApiController]
[Route("api/catalog")]
[AllowAnonymous]
public class CatalogController : ControllerBase
{
    /// <summary>
    /// Devuelve las listas de valores admitidos para los selects del panel y del catálogo.
    /// Sirve como fuente única de verdad — el front no debería tener estas listas hardcodeadas.
    /// </summary>
    [HttpGet("options")]
    public IActionResult Options()
    {
        return Ok(new
        {
            categorias = ProductCatalogo.Categorias,
            materiales = ProductCatalogo.Materiales,
            estados = ProductCatalogo.Estados,
            estadosPedido = OrderCatalogo.Estados
        });
    }
}
