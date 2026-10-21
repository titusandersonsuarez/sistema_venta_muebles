using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NogalApi.Data;
using NogalApi.Models;
using NogalApi.Models.Orders;
using NogalApi.Models.Sales;

namespace NogalApi.Services;

public class SalesService : ISalesService
{
    private static readonly TimeZoneInfo ZonaBogota = ResolverZonaBogota();

    private readonly AppDbContext _context;

    public SalesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SalesDashboardDto> ObtenerAsync(
        DateOnly from,
        DateOnly to,
        string granularity,
        CancellationToken cancellationToken = default)
    {
        if (to <= from)
        {
            throw new SalesQueryException("La fecha final debe ser posterior a la fecha inicial.");
        }

        var granularidad = granularity.Trim().ToLowerInvariant();
        if (granularidad is not ("day" or "week" or "month"))
        {
            throw new SalesQueryException("La granularidad debe ser day, week o month.");
        }

        var fromUtc = AUtc(from);
        var toUtc = AUtc(to);

        var pedidos = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt < toUtc && o.Estado != OrderCatalogo.PagoPendiente)
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .ToListAsync(cancellationToken);

        var bucketStarts = GenerarBuckets(from, to, granularidad);
        var pedidosPorBucket = pedidos
            .GroupBy(p => InicioBucket(FechaLocal(p.CreatedAt), granularidad))
            .ToDictionary(g => g.Key, g => g.ToList());

        var buckets = bucketStarts.Select(inicio =>
        {
            pedidosPorBucket.TryGetValue(inicio, out var pedidosDelBucket);
            pedidosDelBucket ??= new List<Order>();
            var fin = SiguienteBucket(inicio, granularidad);

            return new SalesBucketDto
            {
                Label = EtiquetaBucket(inicio, granularidad),
                From = DateOnly.FromDateTime(inicio),
                To = DateOnly.FromDateTime(fin),
                Ventas = pedidosDelBucket.Sum(p => p.Total),
                Pedidos = pedidosDelBucket.Count
            };
        }).ToList();

        var ventas = pedidos.Sum(p => p.Total);
        var unidades = pedidos.SelectMany(p => p.Items).Sum(i => i.Cantidad);

        return new SalesDashboardDto
        {
            From = from,
            To = to,
            Granularity = granularidad,
            Totales = new SalesTotalsDto
            {
                Ventas = ventas,
                Pedidos = pedidos.Count,
                Unidades = unidades,
                TicketPromedio = pedidos.Count == 0 ? 0 : Math.Round(ventas / pedidos.Count, 2)
            },
            Buckets = buckets,
            Categorias = pedidos
                .SelectMany(p => p.Items.Select(i => new { PedidoId = p.Id, Categoria = i.Product?.Categoria ?? "Sin categoría", i.Subtotal }))
                .GroupBy(i => i.Categoria)
                .Select(g => new SalesCategoryDto
                {
                    Categoria = g.Key,
                    Ventas = g.Sum(i => i.Subtotal),
                    Pedidos = g.Select(i => i.PedidoId).Distinct().Count(),
                    Porcentaje = ventas == 0 ? 0 : Math.Round(g.Sum(i => i.Subtotal) / ventas * 100, 2)
                })
                .OrderByDescending(c => c.Ventas)
                .ToList(),
            MasVendidos = pedidos
                .SelectMany(p => p.Items)
                .GroupBy(i => new { i.ProductId, i.NombreProducto })
                .Select(g => new TopProductSalesDto
                {
                    ProductId = g.Key.ProductId,
                    Nombre = g.Key.NombreProducto,
                    Unidades = g.Sum(i => i.Cantidad),
                    Ventas = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(p => p.Unidades)
                .ThenByDescending(p => p.Ventas)
                .Take(5)
                .ToList()
        };
    }

    private static DateTime AUtc(DateOnly fecha)
    {
        var local = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, ZonaBogota);
    }

    private static DateTime FechaLocal(DateTime fechaUtc)
    {
        var utc = DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, ZonaBogota);
    }

    private static List<DateTime> GenerarBuckets(DateOnly from, DateOnly to, string granularidad)
    {
        var inicio = InicioBucket(from.ToDateTime(TimeOnly.MinValue), granularidad);
        var fin = to.ToDateTime(TimeOnly.MinValue);
        var buckets = new List<DateTime>();

        while (inicio < fin)
        {
            buckets.Add(inicio);
            inicio = SiguienteBucket(inicio, granularidad);
        }

        return buckets;
    }

    private static DateTime InicioBucket(DateTime fecha, string granularidad)
    {
        var dia = fecha.Date;
        return granularidad switch
        {
            "day" => dia,
            "week" => dia.AddDays(-((int)dia.DayOfWeek + 6) % 7),
            "month" => new DateTime(dia.Year, dia.Month, 1),
            _ => dia
        };
    }

    private static DateTime SiguienteBucket(DateTime inicio, string granularidad) => granularidad switch
    {
        "day" => inicio.AddDays(1),
        "week" => inicio.AddDays(7),
        "month" => inicio.AddMonths(1),
        _ => inicio.AddDays(1)
    };

    private static string EtiquetaBucket(DateTime inicio, string granularidad) => granularidad switch
    {
        "day" => inicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        "week" => $"Semana {inicio:yyyy-MM-dd}",
        "month" => inicio.ToString("yyyy-MM", CultureInfo.InvariantCulture),
        _ => inicio.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
    };

    private static TimeZoneInfo ResolverZonaBogota()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        }
        catch (InvalidTimeZoneException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
        }
    }
}