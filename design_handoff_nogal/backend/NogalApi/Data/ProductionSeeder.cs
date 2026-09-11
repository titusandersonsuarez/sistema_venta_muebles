using Microsoft.EntityFrameworkCore;
using NogalApi.Models;
using NogalApi.Models.Production;

namespace NogalApi.Data;

public static class ProductionSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (!await context.InventoryItems.AnyAsync())
        {
            context.InventoryItems.AddRange(
                new InventoryItem { Nombre = "Roble macizo 25 mm", Stock = "18 láminas", Estado = "Crítico" },
                new InventoryItem { Nombre = "Lino crudo antimanchas", Stock = "62 m", Estado = "Bajo" },
                new InventoryItem { Nombre = "Espuma alta densidad", Stock = "140 bloques", Estado = "Normal" },
                new InventoryItem { Nombre = "Herrajes metálicos", Stock = "1.240 und", Estado = "Normal" });
        }

        if (!await context.ProductionOrders.AnyAsync())
        {
            var productos = await context.Products.Where(p => p.Activo).OrderBy(p => p.Id).Take(4).ToListAsync();
            if (productos.Count > 0)
            {
                var datos = new[]
                {
                    (ProductionCatalogo.Corte, 14, 1.2m),
                    (ProductionCatalogo.Armado, 22, 2.0m),
                    (ProductionCatalogo.Tapiceria, 9, 2.6m),
                    (ProductionCatalogo.AcabadoYEmpaque, 11, 1.1m)
                };
                for (var indice = 0; indice < datos.Length; indice++)
                {
                    var (etapa, cantidad, dias) = datos[indice];
                    context.ProductionOrders.Add(new ProductionOrder
                    {
                        ProductId = productos[indice % productos.Count].Id,
                        Etapa = etapa,
                        Cantidad = cantidad,
                        DiasEnEtapa = dias,
                        FechaEntrada = DateTime.UtcNow.AddDays(-(indice + 1))
                    });
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
