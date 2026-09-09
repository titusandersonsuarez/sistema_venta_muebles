using EntrevistaDesktop.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace EntrevistaDesktop.Services;

public class UsuarioApiService
{
    private readonly HttpClient _httpClient;

    public UsuarioApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5067/api/")
        };
    }
    

    public async Task<List<Usuario>> GetUsuariosAsync()
    {
        var usuarios = await _httpClient.GetFromJsonAsync<List<Usuario>>("Usuarios");

        return usuarios ?? new List<Usuario>();
    }

    public async Task<Usuario?> CrearUsuarioAsync(Usuario usuario)
    {
        var response = await _httpClient.PostAsJsonAsync("Usuarios", usuario);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Usuario>();
    }

    public async Task<Usuario?> ActualizarUsuarioAsync(int id, Usuario usuario)
{
    var response = await _httpClient.PutAsJsonAsync(
        $"Usuarios/{id}",
        usuario);

    if (!response.IsSuccessStatusCode)
        return null;

    return await response.Content.ReadFromJsonAsync<Usuario>();
}
}