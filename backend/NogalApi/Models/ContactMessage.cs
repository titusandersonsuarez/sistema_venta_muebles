namespace NogalApi.Models;

public class ContactMessage
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Contacto { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Atendido { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
