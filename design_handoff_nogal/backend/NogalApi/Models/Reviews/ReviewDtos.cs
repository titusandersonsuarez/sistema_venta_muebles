namespace NogalApi.Models.Reviews;

/// <summary>Vista pública de una reseña aprobada.</summary>
public class ReviewDto
{
    public int Id { get; set; }
    public string Autor { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Texto { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// Vista admin: incluye estado de moderación y datos del producto para
/// que la tabla no tenga que hacer joins en el frontend.
/// </summary>
public class AdminReviewDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public string ProductoSlug { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Texto { get; set; } = string.Empty;
    public bool Aprobado { get; set; }
    public DateTime FechaCreacion { get; set; }
}

/// <summary>Payload del formulario público en /producto/:slug.</summary>
public class CreateReviewDto
{
    public string Autor { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Texto { get; set; } = string.Empty;
}

/// <summary>Respuesta al POST público: no expone Id ni Aprobado.</summary>
public class ReviewSubmittedDto
{
    public string Mensaje { get; set; } = string.Empty;
}
