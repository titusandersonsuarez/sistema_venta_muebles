using Microsoft.EntityFrameworkCore;
using NogalApi.Models;
using NogalApi.Models.Orders;
using NogalApi.Services;

namespace NogalApi.Data;

/// <summary>
/// Siembra 4-5 pedidos de ejemplo al primer arranque, si la tabla está
/// vacía y ya hay productos. Sirve para que la tabla del panel tenga
/// algo visible antes de que exista un flujo real de compras.
/// </summary>
public static class OrderSeeder
{
    private static readonly (string cliente, string ciudad, string estado, int cantidad)[] Demo =
    {
        ("Camila Restrepo",     "Bogotá",     OrderCatalogo.EnRuta,        1),
        ("Andrés Villalba",     "Medellín",   OrderCatalogo.EnTaller,      2),
        ("María Fernanda Ruiz", "Cali",       OrderCatalogo.Entregado,     1),
        ("Nicolás Pardo",       "Barranquilla", OrderCatalogo.PagoPendiente, 3),
        ("Laura Ríos",          "Bogotá",     OrderCatalogo.Entregado,     1)
    };

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Orders.AnyAsync())
        {
            return;
        }

        var productos = await context.Products
            .Where(p => p.Activo)
            .OrderBy(p => p.Id)
            .Take(4)
            .ToListAsync();

        if (productos.Count == 0)
        {
            return;
        }

        var codigoService = new OrderService(context);

        for (var i = 0; i < Demo.Length; i++)
        {
            var (cliente, ciudad, estado, cantidad) = Demo[i];
            var producto = productos[i % productos.Count];
            var precio = producto.PrecioCOP;

            var pedido = new Order
            {
                Codigo = await codigoService.GenerarCodigoUnicoAsync(),
                Cliente = cliente,
                Ciudad = ciudad,
                Estado = estado,
                Total = precio * cantidad,
                CreatedAt = DateTime.UtcNow.AddDays(-Demo.Length + i),
                Items = new List<OrderItem>
                {
                    new()
                    {
                        ProductId = producto.Id,
                        NombreProducto = producto.Nombre,
                        Cantidad = cantidad,
                        PrecioUnitario = precio,
                        Subtotal = precio * cantidad
                    }
                }
            };
            context.Orders.Add(pedido);
        }

        await context.SaveChangesAsync();
    }
}
