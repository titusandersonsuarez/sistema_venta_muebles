using NogalApi.Models.Products;
using NogalApi.Models.Reviews;

namespace NogalApi.Services;

public interface IReviewService
{
    /// <summary>Reseñas aprobadas de un producto identificado por slug, más recientes primero.</summary>
    Task<IReadOnlyList<ReviewDto>> ListarPorSlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Crea una reseña con <c>Aprobado=false</c>. Devuelve false si el
    /// producto no existe o está inactivo.
    /// </summary>
    Task<bool> CrearParaSlugAsync(string slug, CreateReviewDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista paginada para el panel. <paramref name="aprobado"/>:
    /// null = todas, true = solo aprobadas, false = solo pendientes.
    /// </summary>
    Task<PagedResult<AdminReviewDto>> ListarAdminAsync(bool? aprobado, int pagina, int tamano);

    Task<AdminReviewDto?> AprobarAsync(int id);

    /// <summary>Elimina físicamente (para spam). No hay soft delete de reseñas.</summary>
    Task<bool> EliminarAsync(int id);
}
