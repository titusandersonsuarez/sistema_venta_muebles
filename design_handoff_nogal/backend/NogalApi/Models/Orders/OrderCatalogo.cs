namespace NogalApi.Models.Orders;

/// <summary>
/// Estados válidos de un pedido, en el mismo orden y forma que espera el
/// prototipo. Se validan en el service para no aceptar valores inventados.
/// </summary>
public static class OrderCatalogo
{
    public const string EnRuta = "En ruta";
    public const string EnTaller = "En taller";
    public const string Entregado = "Entregado";
    public const string PagoPendiente = "Pago pendiente";

    public static readonly IReadOnlyList<string> Estados = new[]
    {
        EnRuta,
        EnTaller,
        Entregado,
        PagoPendiente
    };
}
