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
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

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
            entity.Property(p => p.Modelo3dUrl).HasMaxLength(500);
            entity.Property(p => p.ModeloUsdzUrl).HasMaxLength(500);
            entity.Property(p => p.Modelo3dEstado).HasMaxLength(30).IsRequired().HasDefaultValue("Sin modelo");
            entity.Property(p => p.Modelo3dError).HasMaxLength(1000);

            entity.HasMany(p => p.Imagenes)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Variantes)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.Property(i => i.Url).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(v => new { v.ProductId, v.Sku }).IsUnique();
            entity.Property(v => v.Sku).HasMaxLength(60).IsRequired();
            entity.Property(v => v.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Tipo).HasMaxLength(40).IsRequired().HasDefaultValue("Madera");
            entity.Property(v => v.CodigoColorHex).HasMaxLength(20);
            entity.Property(v => v.PrecioAjusteCOP).HasPrecision(12, 2);
            entity.Property(v => v.FotoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(o => o.Codigo).IsUnique();
            entity.Property(o => o.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(o => o.Cliente).HasMaxLength(120).IsRequired();
            entity.Property(o => o.Ciudad).HasMaxLength(80).IsRequired();
            entity.Property(o => o.Estado).HasMaxLength(30).IsRequired();
            entity.Property(o => o.Total).HasPrecision(12, 2);

            entity.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.NombreProducto).HasMaxLength(200).IsRequired();
            entity.Property(i => i.PrecioUnitario).HasPrecision(12, 2);
            entity.Property(i => i.Subtotal).HasPrecision(12, 2);

            // RESTRICT en Product: no queremos borrar (físicamente) un producto
            // que aparece en pedidos históricos. Además, Products.Activo hace
            // soft delete, así que este caso solo pasaría si alguien fuerza
            // un DELETE manual en BD.
            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductionOrder>(entity =>
        {
            entity.Property(o => o.Etapa).HasMaxLength(30).IsRequired();
            entity.Property(o => o.DiasEnEtapa).HasPrecision(4, 1);

            entity.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.Property(i => i.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(i => i.Stock).HasMaxLength(40).IsRequired();
            entity.Property(i => i.Estado).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<ContactMessage>(entity =>
        {
            entity.Property(m => m.Nombre).HasMaxLength(120).IsRequired();
            entity.Property(m => m.Contacto).HasMaxLength(120).IsRequired();
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasIndex(s => s.SessionId).IsUnique();
            entity.Property(s => s.SessionId).HasMaxLength(40).IsRequired();
            entity.HasMany(s => s.Messages)
                .WithOne(m => m.Session)
                .HasForeignKey(m => m.ChatSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.Property(m => m.Origen).HasMaxLength(10).IsRequired();
        });
    }
}
