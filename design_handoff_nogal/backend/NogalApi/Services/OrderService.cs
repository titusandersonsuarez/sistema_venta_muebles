using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models.Orders;
using NogalApi.Models.Products;

namespace NogalApi.Services;

public class OrderService : IOrderService
{
    private const int TamanoMax = 100;
    private static readonly Random Rng = new();

    private readonly AppDbContext _context;

    public OrderService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrderListItemDto>> ListarAsync(int pagina, int tamano, string? estado)
    {
        var query = _context.Orders.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(o => o.Estado == estado);
        }

        var (p, t) = NormalizarPaginacion(pagina, tamano);

        var total = await query.CountAsync();
        var pedidos = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((p - 1) * t)
            .Take(t)
            .Include(o => o.Items)
            .ToListAsync();

        return new PagedResult<OrderListItemDto>
        {
            Items = pedidos.Select(MapListItem).ToList(),
            Total = total,
            Pagina = p,
            Tamano = t
        };
    }

    public async Task<OrderDto?> ObtenerAsync(int id)
    {
        var pedido = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        return pedido is null ? null : Map(pedido);
    }

    public async Task<OrderDto?> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        if (!OrderCatalogo.Estados.Contains(nuevoEstado))
        {
            throw new InvalidOperationException(
                $"Estado no válido. Usa uno de: {string.Join(", ", OrderCatalogo.Estados)}.");
        }

        var pedido = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (pedido is null)
        {
            return null;
        }

        pedido.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return Map(pedido);
    }

    /// <summary>
    /// Genera un código único NGL-XXXXXX (hex, 6 chars). Reintenta si hay colisión.
    /// </summary>
    public async Task<string> GenerarCodigoUnicoAsync(CancellationToken cancellationToken = default)
    {
        for (var intento = 0; intento < 5; intento++)
        {
            var buffer = new byte[3];
            lock (Rng) { Rng.NextBytes(buffer); }
            var codigo = $"NGL-{Convert.ToHexString(buffer)}";
            var existe = await _context.Orders.AnyAsync(o => o.Codigo == codigo, cancellationToken);
            if (!existe) return codigo;
        }
        throw new InvalidOperationException("No se pudo generar un código de pedido único.");
    }

    private static (int pagina, int tamano) NormalizarPaginacion(int pagina, int tamano)
    {
        var p = pagina < 1 ? 1 : pagina;
        var t = tamano < 1 ? 20 : tamano > TamanoMax ? TamanoMax : tamano;
        return (p, t);
    }

    private static OrderListItemDto MapListItem(Models.Order o)
    {
        return new OrderListItemDto
        {
            Id = o.Id,
            Codigo = o.Codigo,
            Cliente = o.Cliente,
            Ciudad = o.Ciudad,
            Total = o.Total,
            Estado = o.Estado,
            CreatedAt = o.CreatedAt,
            ResumenProductos = ResumenDeProductos(o.Items)
        };
    }

    private static OrderDto Map(Models.Order o) => new()
    {
        Id = o.Id,
        Codigo = o.Codigo,
        Cliente = o.Cliente,
        Ciudad = o.Ciudad,
        Total = o.Total,
        Estado = o.Estado,
        CreatedAt = o.CreatedAt,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            NombreProducto = i.NombreProducto,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
            Subtotal = i.Subtotal
        }).ToList()
    };

    private static string ResumenDeProductos(IReadOnlyCollection<Models.OrderItem> items)
    {
        if (items.Count == 0) return string.Empty;
        var primero = items.First().NombreProducto;
        if (items.Count == 1) return primero;
        return $"{primero} + {items.Count - 1} más";
    }
}
