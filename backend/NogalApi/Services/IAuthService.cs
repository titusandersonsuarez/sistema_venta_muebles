using NogalApi.Models.Auth;

namespace NogalApi.Services;

public interface IAuthService
{
    Task<AuthenticationResult?> LoginAsync(string nombreUsuario, string password);
}
