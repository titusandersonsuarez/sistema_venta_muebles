namespace NogalApi.Options;

/// <summary>
/// Credenciales del usuario administrador inicial, creado automáticamente
/// la primera vez que la API arranca contra una base de datos vacía.
/// La contraseña se define con la variable de entorno AdminSeed__Password.
/// Si no se define, se genera una aleatoria y se muestra una sola vez en el log.
/// </summary>
public class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string NombreUsuario { get; set; } = "admin";
    public string Password { get; set; } = string.Empty;
    public string Nombre { get; set; } = "Administrador";
}
