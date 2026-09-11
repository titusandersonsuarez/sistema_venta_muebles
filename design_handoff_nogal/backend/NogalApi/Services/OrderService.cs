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
    private readonly IPaymentService _paymentService;

    public OrderService(AppDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
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

    public async Task<OrderDto?> ObtenerPorCodigoAsync(string codigo)
    {
        var pedido = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Codigo == codigo);
        return pedido is null ? null : Map(pedido);
    }

    /// <summary>
    /// Crea un pedido desde la tienda pública. Recalcula todos los precios
    /// en el servidor (nunca confía en el cliente), valida producto activo
    /// y variante asociada, aplica costo de envío gratis desde $500.000 y
    /// deja el pedido en estado "Pago pendiente".
    /// </summary>
    public async Task<OrderDto> CrearAsync(CreateOrderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Cliente))
            throw new InvalidOperationException("El nombre del cliente es obligatorio.");
        if (string.IsNullOrWhiteSpace(dto.Ciudad))
            throw new InvalidOperationException("La ciudad es obligatoria.");
        if (string.IsNullOrWhiteSpace(dto.Contacto))
            throw new InvalidOperationException("Un WhatsApp o correo es obligatorio.");
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("El pedido no tiene productos.");

        var productoIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productos = await _context.Products
            .Where(p => productoIds.Contains(p.Id))
            .Include(p => p.Variantes)
            .ToListAsync();

        var pedido = new Models.Order
        {
            Cliente = dto.Cliente.Trim(),
            Ciudad = dto.Ciudad.Trim(),
            Contacto = dto.Contacto.Trim(),
            Estado = "Pago pendiente",
            CreatedAt = DateTime.UtcNow
        };

        decimal subtotal = 0;

        foreach (var item in dto.Items)
        {
            if (item.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            var producto = productos.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new InvalidOperationException($"Producto {item.ProductId} no existe.");
            if (!producto.Activo)
                throw new InvalidOperationException($"'{producto.Nombre}' ya no está disponible.");

            Models.ProductVariant? variante = null;
            decimal ajuste = 0;
            if (item.ProductVariantId.HasValue)
            {
                variante = producto.Variantes.FirstOrDefault(v => v.Id == item.ProductVariantId.Value)
                    ?? throw new InvalidOperationException(
                        $"La variante seleccionada no pertenece a '{producto.Nombre}'.");
                if (!variante.Activo)
                    throw new InvalidOperationException(
                        $"El acabado '{variante.Nombre}' ya no está disponible.");
                ajuste = variante.PrecioAjusteCOP;
            }

            var precioUnitario = producto.PrecioCOP + ajuste;
            var subtotalItem = precioUnitario * item.Cantidad;
            subtotal += subtotalItem;

            pedido.Items.Add(new Models.OrderItem
            {
                ProductId = producto.Id,
                NombreProducto = producto.Nombre,
                Cantidad = item.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotalItem,
                ProductVariantId = variante?.Id,
                VarianteNombre = variante?.Nombre,
                PrecioAjusteVariante = ajuste
            });
        }

        pedido.EnvioCOP = subtotal >= 500_000m ? 0m : 30_000m;
        pedido.Total = subtotal + pedido.EnvioCOP;
        pedido.Codigo = await GenerarCodigoUnicoAsync();

        _context.Orders.Add(pedido);
        await _context.SaveChangesAsync();

        return Map(pedido);
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

    public async Task<PagoIntencionDto?> IniciarPagoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var pedido = await _context.Orders.FirstOrDefaultAsync(o => o.Codigo == codigo, cancellationToken);
        if (pedido is null) return null;
        if (pedido.Estado == OrderCatalogo.PagoConfirmado)
        {
            throw new InvalidOperationException("Este pedido ya fue pagado.");
        }

        var intencion = await _paymentService.CrearIntencionAsync(pedido, cancellationToken);
        pedido.PagoProveedor = intencion.Proveedor;
        pedido.PagoActualizadoEn = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new PagoIntencionDto { CheckoutUrl = intencion.CheckoutUrl, Proveedor = intencion.Proveedor };
    }

    public async Task<OrderDto?> ConfirmarPagoDemoAsync(string codigo, bool aprobado, CancellationToken cancellationToken = default)
    {
        var pedido = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Codigo == codigo, cancellationToken);
        if (pedido is null) return null;
        if (pedido.PagoProveedor != "demo")
        {
            throw new InvalidOperationException("Este pedido no está en modo demo.");
        }

        AplicarResultadoPago(pedido, aprobado ? PagoEstadoAgregado.Aprobado : PagoEstadoAgregado.Rechazado,
            $"DEMO-{Guid.NewGuid():N}"[..12]);
        await _context.SaveChangesAsync(cancellationToken);
        return Map(pedido);
    }

    public async Task<OrderDto?> VerificarPagoAsync(string codigo, string transactionId, CancellationToken cancellationToken = default)
    {
        var pedido = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Codigo == codigo, cancellationToken);
        if (pedido is null) return null;

        var resultado = await _paymentService.ConsultarAsync(transactionId, cancellationToken);
        if (resultado is null)
        {
            return Map(pedido);
        }

        AplicarResultadoPago(pedido, resultado.Estado, resultado.TransaccionId);
        await _context.SaveChangesAsync(cancellationToken);
        return Map(pedido);
    }

    private static void AplicarResultadoPago(Models.Order pedido, PagoEstadoAgregado estado, string transaccionId)
    {
        pedido.PagoTransaccionId = transaccionId;
        pedido.PagoActualizadoEn = DateTime.UtcNow;
        pedido.Estado = estado switch
        {
            PagoEstadoAgregado.Aprobado => OrderCatalogo.PagoConfirmado,
            PagoEstadoAgregado.Rechazado => OrderCatalogo.PagoRechazado,
            _ => pedido.Estado
        };
    }

    /// <summary>
    /// Genera un código único NGL-XXXXXX (hex, 6 chars). Reintenta si hay colisión.
    /// </summary>
    public Task<string> GenerarCodigoUnicoAsync(CancellationToken cancellationToken = default)
        => GenerarCodigoUnicoAsync(_context, cancellationToken);

    /// <summary>
    /// Overload estático para el seeder, que no necesita instanciar el
    /// service completo con todas sus dependencias.
    /// </summary>
    public static async Task<string> GenerarCodigoUnicoAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        for (var intento = 0; intento < 5; intento++)
        {
            var buffer = new byte[3];
            lock (Rng) { Rng.NextBytes(buffer); }
            var codigo = $"NGL-{Convert.ToHexString(buffer)}";
            var existe = await context.Orders.AnyAsync(o => o.Codigo == codigo, cancellationToken);
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
        Contacto = o.Contacto,
        Total = o.Total,
        EnvioCOP = o.EnvioCOP,
        Estado = o.Estado,
        CreatedAt = o.CreatedAt,
        PagoProveedor = o.PagoProveedor,
        PagoTransaccionId = o.PagoTransaccionId,
        PagoActualizadoEn = o.PagoActualizadoEn,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            NombreProducto = i.NombreProducto,
            Cantidad = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
            Subtotal = i.Subtotal,
            ProductVariantId = i.ProductVariantId,
            VarianteNombre = i.VarianteNombre,
            PrecioAjusteVariante = i.PrecioAjusteVariante
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
