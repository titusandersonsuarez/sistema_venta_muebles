namespace NogalApi.Services;

public class CommunicationValidationException : Exception
{
    public CommunicationValidationException(string message) : base(message) { }
}
