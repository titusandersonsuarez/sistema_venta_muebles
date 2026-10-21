namespace NogalApi.Services;

public sealed class ProductValidationException : Exception
{
    public ProductValidationException(string message)
        : base(message)
    {
    }
}