using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Models.Products;

namespace NogalApi.Services;

public class ProductService : IProductService
{
    private const int TamanoMax = 100;

    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProductPublicDto>> ListarPublicoAsync(ProductFilters filtros)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.Activo);

        if (!string.IsNullOrWhiteSpace(filtros.Categoria))
        {
            query = query.Where(p => p.Categoria == filtros.Categoria);
        }

        if (!string.IsNullOrWhiteSpace(filtros.Material))
        {
            query = query.Where(p => p.Material == filtros.Material);
        }

        if (filtros.PrecioMax is { } max && max > 0)
        {
            query = query.Where(p => p.PrecioCOP <= max);
        }

        var (pagina, tamano) = NormalizarPaginacion(filtros.Pagina, filtros.Tamano);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.FechaCreacion)
            .Skip((pagina - 1) * tamano)
            .Take(tamano)
            .Include(p => p.Imagenes.OrderBy(i => i.Orden))
            .ToListAsync();

        return new PagedResult<ProductPublicDto>
        {
            Items = items.Select(MapPublic).ToList(),
            Total = total,
            Pagina = pagina,
            Tamano = tamano
        };
    }

    public async Task<ProductPublicDto?> ObtenerPorSlugAsync(string slug)
    {
        var producto = await _context.Products
            .AsNoTracking()
            .Where(p => p.Activo && p.Slug == slug)
            .Include(p => p.Imagenes.OrderBy(i => i.Orden))
            .FirstOrDefaultAsync();

        return producto is null ? null : MapPublic(producto);
    }

    public async Task<PagedResult<ProductDto>> ListarAdminAsync(int pagina, int tamano, bool incluirInactivos)
    {
        var query = _context.Products.AsNoTracking();
        if (!incluirInactivos)
        {
            query = query.Where(p => p.Activo);
        }

        var (p, t) = NormalizarPaginacion(pagina, tamano);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(pr => pr.FechaCreacion)
            .Skip((p - 1) * t)
            .Take(t)
            .Include(pr => pr.Imagenes.OrderBy(i => i.Orden))
            .ToListAsync();

        return new PagedResult<ProductDto>
        {
            Items = items.Select(MapAdmin).ToList(),
            Total = total,
            Pagina = p,
            Tamano = t
        };
    }

    public async Task<ProductDto?> ObtenerAdminAsync(int id)
    {
        var producto = await _context.Products
            .Include(p => p.Imagenes.OrderBy(i => i.Orden))
            .FirstOrDefaultAsync(p => p.Id == id);
        return producto is null ? null : MapAdmin(producto);
    }

    public async Task<ProductDto> CrearAsync(CreateProductDto dto)
    {
        var estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Disponible" : dto.Estado!;
        var slug = await GenerarSlugUnicoAsync(dto.Nombre);

        var producto = new Product
        {
            Slug = slug,
            Nombre = dto.Nombre.Trim(),
            Categoria = dto.Categoria,
            Material = dto.Material,
            PrecioCOP = dto.PrecioCOP,
            Medidas = Trim(dto.Medidas),
            Peso = Trim(dto.Peso),
            Armado = Trim(dto.Armado),
            Descripcion = Trim(dto.Descripcion),
            Estado = estado,
            ImagenUrl = Trim(dto.ImagenUrl),
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        return MapAdmin(producto);
    }

    public async Task<ProductDto?> ActualizarAsync(int id, UpdateProductDto dto)
    {
        var producto = await _context.Products
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return null;
        }

        var nombreNormalizado = dto.Nombre.Trim();
        if (!string.Equals(producto.Nombre, nombreNormalizado, StringComparison.Ordinal))
        {
            producto.Nombre = nombreNormalizado;
            producto.Slug = await GenerarSlugUnicoAsync(nombreNormalizado, producto.Id);
        }

        producto.Categoria = dto.Categoria;
        producto.Material = dto.Material;
        producto.PrecioCOP = dto.PrecioCOP;
        producto.Medidas = Trim(dto.Medidas);
        producto.Peso = Trim(dto.Peso);
        producto.Armado = Trim(dto.Armado);
        producto.Descripcion = Trim(dto.Descripcion);
        if (!string.IsNullOrWhiteSpace(dto.Estado))
        {
            producto.Estado = dto.Estado!;
        }
        if (dto.ImagenUrl is not null)
        {
            producto.ImagenUrl = Trim(dto.ImagenUrl);
        }
        producto.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapAdmin(producto);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var producto = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return false;
        }

        producto.Activo = false;
        producto.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProductDto?> RestaurarAsync(int id)
    {
        var producto = await _context.Products
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return null;
        }

        producto.Activo = true;
        producto.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return MapAdmin(producto);
    }

    public async Task<ProductDto?> AsignarImagenUrlAsync(int id, string url)
    {
        var producto = await _context.Products
            .Include(p => p.Imagenes)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
        {
            return null;
        }

        producto.ImagenUrl = url;
        producto.Imagenes.Add(new ProductImage
        {
            Url = url,
            Orden = producto.Imagenes.Count
        });
        producto.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapAdmin(producto);
    }

    private static (int pagina, int tamano) NormalizarPaginacion(int pagina, int tamano)
    {
        var p = pagina < 1 ? 1 : pagina;
        var t = tamano < 1 ? 20 : tamano > TamanoMax ? TamanoMax : tamano;
        return (p, t);
    }

    private static string? Trim(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private async Task<string> GenerarSlugUnicoAsync(string nombre, int? ignorarId = null)
    {
        var baseSlug = Slugify(nombre);
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "producto";
        }

        var candidato = baseSlug;
        var i = 2;
        while (await _context.Products.AnyAsync(p => p.Slug == candidato && (ignorarId == null || p.Id != ignorarId)))
        {
            candidato = $"{baseSlug}-{i++}";
        }

        return candidato;
    }

    private static string Slugify(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var normalizado = valor.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalizado.Length);
        foreach (var ch in normalizado)
        {
            var categoria = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (categoria == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (ch is ' ' or '-' or '_' or '/')
            {
                sb.Append('-');
            }
        }

        var slug = sb.ToString();
        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace("--", "-", StringComparison.Ordinal);
        }
        return slug.Trim('-');
    }

    private static ProductDto MapAdmin(Product p) => new()
    {
        Id = p.Id,
        Slug = p.Slug,
        Nombre = p.Nombre,
        Categoria = p.Categoria,
        Material = p.Material,
        PrecioCOP = p.PrecioCOP,
        Medidas = p.Medidas,
        Peso = p.Peso,
        Armado = p.Armado,
        Descripcion = p.Descripcion,
        Estado = p.Estado,
        ImagenUrl = p.ImagenUrl,
        Activo = p.Activo,
        FechaCreacion = p.FechaCreacion,
        FechaActualizacion = p.FechaActualizacion,
        Imagenes = p.Imagenes.Select(i => new ProductImageDto
        {
            Id = i.Id,
            Url = i.Url,
            Orden = i.Orden
        }).ToList()
    };

    private static ProductPublicDto MapPublic(Product p) => new()
    {
        Id = p.Id,
        Slug = p.Slug,
        Nombre = p.Nombre,
        Categoria = p.Categoria,
        Material = p.Material,
        PrecioCOP = p.PrecioCOP,
        Medidas = p.Medidas,
        Peso = p.Peso,
        Armado = p.Armado,
        Descripcion = p.Descripcion,
        Estado = p.Estado,
        ImagenUrl = p.ImagenUrl,
        Imagenes = p.Imagenes.Select(i => new ProductImageDto
        {
            Id = i.Id,
            Url = i.Url,
            Orden = i.Orden
        }).ToList()
    };
}
