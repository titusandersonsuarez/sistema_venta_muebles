namespace NogalApi.Models.Auth;

/// <summary>
/// DTO devuelto por POST /api/auth/login. No incluye el token: la sesión
/// se maneja con una cookie HttpOnly `nogal_auth` que el navegador
/// adjunta automáticamente en cada request. El frontend solo necesita
/// los datos de identidad para mostrarlos en el panel.
/// </summary>
public class LoginResponse
{
    public DateTime ExpiraEn { get; set; }
    public UsuarioResumen Usuario { get; set; } = new();
}

public class UsuarioResumen
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

/// <summary>
/// Resultado interno del AuthService — el token vive aquí para que el
/// controller lo grabe en la cookie, pero nunca sale al cliente.
/// </summary>
public record AuthenticationResult(UsuarioResumen Usuario, string Token, DateTime ExpiraEn);
