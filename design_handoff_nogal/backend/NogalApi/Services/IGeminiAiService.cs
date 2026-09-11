namespace NogalApi.Services;

public interface IGeminiAiService
{
    Task<string?> GenerarRespuestaAsync(string mensajeUsuario, string contextoCatalogo, CancellationToken cancellationToken = default);
}
