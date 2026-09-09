using EntrevistaApi.Models;
public interface IUsuarioService
{
    Task<IEnumerable<Usuario>> GetUsuariosAsync();
    Task<IEnumerable<Usuario>> GetUsuariosActivosAsync();
    Task<Usuario?> GetUsuarioByIdAsync(int id);
    Task<Usuario> CreateUsuarioAsync(Usuario usuario);
    Task<Usuario?> UpdateUsuarioAsync(int id, Usuario usuario);
    Task<bool> DeleteUsuarioAsync(int id);
}