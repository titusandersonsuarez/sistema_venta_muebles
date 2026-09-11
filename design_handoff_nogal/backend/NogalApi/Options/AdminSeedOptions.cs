namespace NogalApi.Options;

/// <summary>
/// Credenciales del usuario administrador inicial, creado automáticamente
/// la primera vez que la API arranca contra una base de datos vacía.
/// Cambia la contraseña real vía configuración/entorno en producción.
/// </summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string NombreUsuario { get; set; } = "admin";
    public string Password { get; set; } = "nogal2026";
    public string Nombre { get; set; } = "Administrador";
}
