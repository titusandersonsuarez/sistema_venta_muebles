using NogalApi.Models.Auth;

namespace NogalApi.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string nombreUsuario, string password);
}
