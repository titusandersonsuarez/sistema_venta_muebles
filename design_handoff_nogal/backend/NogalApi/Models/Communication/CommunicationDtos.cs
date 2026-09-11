namespace NogalApi.Models.Communication;

public class CreateContactMessageDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class ChatRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class ChatResponseDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Respuesta { get; set; } = string.Empty;
}
