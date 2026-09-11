using NogalApi.Models;

namespace NogalApi.Services;

public interface IPaymentService
{
    /// <summary>
    /// Genera la URL de checkout para pagar el pedido. En "demo" apunta a la
    /// pantalla local de simulación; en "wompi" arma la URL de Redirection API
    /// con firma de integridad.
    /// </summary>
    Task<PagoIntencionResultado> CrearIntencionAsync(Order pedido, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta el estado real de la transacción en la pasarela.
    /// El demo devuelve null (la confirmación llega desde el propio callback).
    /// </summary>
    Task<PagoEstadoResultado?> ConsultarAsync(string transactionId, CancellationToken cancellationToken = default);
}

public record PagoIntencionResultado(string Proveedor, string CheckoutUrl);

public enum PagoEstadoAgregado
{
    Aprobado,
    Rechazado,
    Pendiente
}

public record PagoEstadoResultado(PagoEstadoAgregado Estado, string TransaccionId, string ProveedorRaw);
