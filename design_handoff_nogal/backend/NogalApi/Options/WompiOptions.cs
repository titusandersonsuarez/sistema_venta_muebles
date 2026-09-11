namespace NogalApi.Options;

public class WompiOptions
{
    public const string SectionName = "Wompi";

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// "demo" simula el cobro con una pantalla propia sin llamar a Wompi.
    /// "wompi" arma el checkout real usando <see cref="PublicKey"/> y
    /// <see cref="IntegritySecret"/>.
    /// </summary>
    public string Provider { get; set; } = "demo";

    public string? PublicKey { get; set; }
    public string? IntegritySecret { get; set; }
    public string? EventsSecret { get; set; }

    /// <summary>URL de la página hosted de Wompi (sandbox y prod comparten host).</summary>
    public string CheckoutBaseUrl { get; set; } = "https://checkout.wompi.co/p/";

    /// <summary>Base para consultar el estado de la transacción vía API.</summary>
    public string ApiBaseUrl { get; set; } = "https://sandbox.wompi.co/v1";

    /// <summary>
    /// URL a donde Wompi redirige después del pago. Se le concatena el
    /// código del pedido para que el frontend consulte y actualice.
    /// </summary>
    public string RedirectBaseUrl { get; set; } = "http://localhost:5173/carrito/gracias";

    /// <summary>URL de la pantalla demo (solo aplica cuando Provider = "demo").</summary>
    public string DemoCheckoutBaseUrl { get; set; } = "http://localhost:5173/carrito/pago-demo";
}
