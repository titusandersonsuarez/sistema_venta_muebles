namespace NogalApi.Services;

public sealed class SalesQueryException : Exception
{
    public SalesQueryException(string message)
        : base(message)
    {
    }
}