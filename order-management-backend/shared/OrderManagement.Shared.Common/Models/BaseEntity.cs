namespace OrderManagement.Shared.Common.Models;

/// <summary>
/// Clase base para entidades de dominio
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Identificador único de la entidad
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Fecha de creación de la entidad
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Indica si la entidad fue eliminada (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// Fecha de eliminación (si aplica)
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
