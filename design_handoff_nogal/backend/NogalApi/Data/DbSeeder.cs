using Microsoft.EntityFrameworkCore;
using NogalApi.Models;
using NogalApi.Options;

namespace NogalApi.Data;

public static class DbSeeder
{
    /// <summary>
    /// Crea el primer usuario administrador si todavía no existe ninguno.
    /// Así el panel siempre tiene con quién entrar en un ambiente nuevo.
    /// </summary>
    public static async Task SeedAdminAsync(AppDbContext context, AdminSeedOptions seed)
    {
        if (await context.Usuarios.AnyAsync())
        {
            return;
        }

        context.Usuarios.Add(new Usuario
        {
            NombreUsuario = seed.NombreUsuario,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(seed.Password),
            Nombre = seed.Nombre,
            Rol = "Admin",
            Activo = true
        });

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Siembra las variantes maestras de acabado (tinte de madera y tapicería)
    /// para los productos existentes si la tabla de variantes está vacía.
    /// </summary>
    public static async Task SeedProductVariantsAsync(AppDbContext context)
    {
        if (await context.ProductVariants.AnyAsync())
        {
            return;
        }

        var productos = await context.Products.ToListAsync();
        if (productos.Count == 0)
        {
            return;
        }

        foreach (var p in productos)
        {
            var baseSku = (p.Slug ?? $"PROD-{p.Id}").Replace("-", "").ToUpperInvariant();
            if (baseSku.Length > 15) baseSku = baseSku[..15];

            context.ProductVariants.AddRange(
                new ProductVariant
                {
                    ProductId = p.Id,
                    Sku = $"{baseSku}-ROB",
                    Nombre = "Roble natural",
                    Tipo = "Madera",
                    CodigoColorHex = "#c9a063",
                    PrecioAjusteCOP = 0,
                    Stock = 12,
                    Activo = true,
                    Orden = 1
                },
                new ProductVariant
                {
                    ProductId = p.Id,
                    Sku = $"{baseSku}-NOG",
                    Nombre = "Nogal oscuro",
                    Tipo = "Madera",
                    CodigoColorHex = "#4a2c11",
                    PrecioAjusteCOP = 0,
                    Stock = 8,
                    Activo = true,
                    Orden = 2
                },
                new ProductVariant
                {
                    ProductId = p.Id,
                    Sku = $"{baseSku}-LIN",
                    Nombre = "Lino crudo",
                    Tipo = "Tela",
                    CodigoColorHex = "#ded7cb",
                    PrecioAjusteCOP = 80000,
                    Stock = 15,
                    Activo = true,
                    Orden = 3
                },
                new ProductVariant
                {
                    ProductId = p.Id,
                    Sku = $"{baseSku}-GRP",
                    Nombre = "Gris piedra",
                    Tipo = "Tela",
                    CodigoColorHex = "#7d7979",
                    PrecioAjusteCOP = 80000,
                    Stock = 10,
                    Activo = true,
                    Orden = 4
                }
            );
        }

        await context.SaveChangesAsync();
    }
}
