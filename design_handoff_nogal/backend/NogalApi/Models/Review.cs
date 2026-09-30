namespace NogalApi.Models;

/// <summary>
/// Reseña de un cliente sobre un producto. Requiere aprobación admin
/// antes de aparecer en la ficha pública para evitar spam y ofensas —
/// el flujo es: cliente envía → estado <c>Aprobado=false</c> → admin la
/// marca como aprobada desde el panel → aparece en /producto/:slug.
/// </summary>
public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    public string Autor { get; set; } = string.Empty;

    /// <summary>Entero 1-5. Validado en el service (no en la BD para permitir cambios sin migración).</summary>
    public int Rating { get; set; }

    public string Texto { get; set; } = string.Empty;

    public bool Aprobado { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}
