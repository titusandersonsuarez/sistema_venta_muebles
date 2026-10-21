using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Models.Products;
using NogalApi.Models.Reviews;

namespace NogalApi.Services;

public class ReviewService : IReviewService
{
    private const int TamanoMax = 100;
    private const int TextoMin = 20;
    private const int TextoMax = 2000;
    private const int AutorMin = 3;
    private const int AutorMax = 120;

    private readonly AppDbContext _context;

    public ReviewService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReviewDto>> ListarPorSlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var producto = await _context.Products
            .AsNoTracking()
            .Where(p => p.Slug == slug && p.Activo)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);
        if (producto is null) return Array.Empty<ReviewDto>();

        return await _context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == producto.Id && r.Aprobado)
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => new ReviewDto
            {
                Id = r.Id,
                Autor = r.Autor,
                Rating = r.Rating,
                Texto = r.Texto,
                FechaCreacion = r.FechaCreacion
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CrearParaSlugAsync(string slug, CreateReviewDto dto, CancellationToken cancellationToken = default)
    {
        var autor = (dto.Autor ?? string.Empty).Trim();
        var texto = (dto.Texto ?? string.Empty).Trim();
        if (autor.Length < AutorMin)
            throw new InvalidOperationException("El nombre debe tener al menos 3 caracteres.");
        if (autor.Length > AutorMax)
            throw new InvalidOperationException("El nombre es demasiado largo.");
        if (dto.Rating is < 1 or > 5)
            throw new InvalidOperationException("La calificación debe estar entre 1 y 5.");
        if (texto.Length < TextoMin)
            throw new InvalidOperationException($"La reseña debe tener al menos {TextoMin} caracteres.");
        if (texto.Length > TextoMax)
            throw new InvalidOperationException($"La reseña no puede pasar de {TextoMax} caracteres.");

        var producto = await _context.Products
            .Where(p => p.Slug == slug && p.Activo)
            .Select(p => new { p.Id })
            .FirstOrDefaultAsync(cancellationToken);
        if (producto is null) return false;

        _context.Reviews.Add(new Review
        {
            ProductId = producto.Id,
            Autor = autor,
            Rating = dto.Rating,
            Texto = texto,
            Aprobado = false,
            FechaCreacion = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<PagedResult<AdminReviewDto>> ListarAdminAsync(bool? aprobado, int pagina, int tamano)
    {
        var (p, t) = NormalizarPaginacion(pagina, tamano);

        var query = _context.Reviews.AsNoTracking();
        if (aprobado.HasValue)
        {
            query = query.Where(r => r.Aprobado == aprobado.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(r => r.FechaCreacion)
            .Skip((p - 1) * t)
            .Take(t)
            .Join(_context.Products.AsNoTracking(),
                r => r.ProductId,
                prod => prod.Id,
                (r, prod) => new AdminReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductoNombre = prod.Nombre,
                    ProductoSlug = prod.Slug,
                    Autor = r.Autor,
                    Rating = r.Rating,
                    Texto = r.Texto,
                    Aprobado = r.Aprobado,
                    FechaCreacion = r.FechaCreacion
                })
            .ToListAsync();

        return new PagedResult<AdminReviewDto>
        {
            Items = items,
            Total = total,
            Pagina = p,
            Tamano = t
        };
    }

    public async Task<AdminReviewDto?> AprobarAsync(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null) return null;
        if (review.Aprobado)
        {
            return await ProyectarAdminAsync(review);
        }

        review.Aprobado = true;
        await _context.SaveChangesAsync();
        return await ProyectarAdminAsync(review);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null) return false;
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<AdminReviewDto?> ProyectarAdminAsync(Review review)
    {
        var producto = await _context.Products
            .AsNoTracking()
            .Where(p => p.Id == review.ProductId)
            .Select(p => new { p.Nombre, p.Slug })
            .FirstOrDefaultAsync();

        return new AdminReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductoNombre = producto?.Nombre ?? string.Empty,
            ProductoSlug = producto?.Slug ?? string.Empty,
            Autor = review.Autor,
            Rating = review.Rating,
            Texto = review.Texto,
            Aprobado = review.Aprobado,
            FechaCreacion = review.FechaCreacion
        };
    }

    private static (int pagina, int tamano) NormalizarPaginacion(int pagina, int tamano)
    {
        var p = pagina < 1 ? 1 : pagina;
        var t = tamano < 1 ? 20 : tamano > TamanoMax ? TamanoMax : tamano;
        return (p, t);
    }
}
