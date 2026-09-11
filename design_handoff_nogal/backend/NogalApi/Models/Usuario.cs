namespace NogalApi.Models;

/// <summary>
/// Usuario del panel interno (administrador / equipo de Nogal).
/// No confundir con los clientes de la tienda: este modelo es solo
/// para quienes tienen acceso al panel de fábrica.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = "Admin";
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
