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
}
