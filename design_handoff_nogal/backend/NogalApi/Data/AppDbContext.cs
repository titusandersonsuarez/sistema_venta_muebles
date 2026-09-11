using Microsoft.EntityFrameworkCore;
using NogalApi.Models;

namespace NogalApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(u => u.NombreUsuario).IsUnique();
            entity.Property(u => u.NombreUsuario).HasMaxLength(80).IsRequired();
            entity.Property(u => u.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(u => u.Rol).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique();
            entity.Property(p => p.Slug).HasMaxLength(160).IsRequired();
            entity.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Categoria).HasMaxLength(40).IsRequired();
            entity.Property(p => p.Material).HasMaxLength(40).IsRequired();
            entity.Property(p => p.PrecioCOP).HasPrecision(12, 2);
            entity.Property(p => p.Medidas).HasMaxLength(120);
            entity.Property(p => p.Peso).HasMaxLength(40);
            entity.Property(p => p.Armado).HasMaxLength(120);
            entity.Property(p => p.Estado).HasMaxLength(30).IsRequired();
            entity.Property(p => p.ImagenUrl).HasMaxLength(500);

            entity.HasMany(p => p.Imagenes)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.Url).HasMaxLength(500).IsRequired();
        });
    }
}
