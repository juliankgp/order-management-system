using OrderManagement.Shared.Common.Models;

namespace ProductService.Domain.Entities;

/// <summary>
/// Entidad que representa un movimiento de stock
/// </summary>
public class StockMovement : BaseEntity
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Navegación al producto
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Tipo de movimiento
    /// </summary>
    public StockMovementType MovementType { get; set; }
    
    /// <summary>
    /// Cantidad del movimiento (positiva para entradas, negativa para salidas)
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Stock anterior al movimiento
    /// </summary>
    public int PreviousStock { get; set; }
    
    /// <summary>
    /// Stock posterior al movimiento
    /// </summary>
    public int NewStock { get; set; }
    
    /// <summary>
    /// Razón del movimiento
    /// </summary>
    public required string Reason { get; set; }
    
    /// <summary>
    /// Referencia externa (ID de orden, documento, etc.)
    /// </summary>
    public string? ExternalReference { get; set; }
    
    /// <summary>
    /// Usuario que realizó el movimiento
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Comentarios adicionales
    /// </summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Entidad que representa el historial de precios de un producto
/// </summary>
public class PriceHistory : BaseEntity
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Navegación al producto
    /// </summary>
    public virtual Product Product { get; set; } = null!;
    
    /// <summary>
    /// Precio anterior
    /// </summary>
    public decimal PreviousPrice { get; set; }
    
    /// <summary>
    /// Nuevo precio
    /// </summary>
    public decimal NewPrice { get; set; }
    
    /// <summary>
    /// Fecha efectiva del cambio
    /// </summary>
    public DateTime EffectiveDate { get; set; }
    
    /// <summary>
    /// Usuario que realizó el cambio
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Razón del cambio de precio
    /// </summary>
    public string? Reason { get; set; }
}
