using EntrevistaApi.Data;
using EntrevistaApi.Models;
using Microsoft.EntityFrameworkCore;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Usuario>> GetUsuariosAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetUsuariosActivosAsync()
    {
        return await _context.Usuarios.Where(u => u.Activo).ToListAsync();
    }

    public async Task<Usuario?> GetUsuarioByIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<Usuario> CreateUsuarioAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario?> UpdateUsuarioAsync(int id, Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return null;
        }

        var usuarioExistente = await _context.Usuarios.FindAsync(id);

        if (usuarioExistente == null)
        {
            return null;
        }

        usuarioExistente.Name = usuario.Name;
        usuarioExistente.Email = usuario.Email;
        usuarioExistente.Activo = usuario.Activo;

        await _context.SaveChangesAsync();

        return usuarioExistente;
    }

    public async Task<bool> DeleteUsuarioAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);

        if (usuario == null)
        {
            return false;
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return true;
    }
}