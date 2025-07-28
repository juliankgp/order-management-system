using OrderManagement.Shared.Common.Models;

namespace OrderService.Domain.Entities;

/// <summary>
/// Entidad que representa un item dentro de una orden
/// </summary>
public class OrderItem : BaseEntity
{
    /// <summary>
    /// ID de la orden a la que pertenece este item
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Navegación hacia la orden
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// ID del producto
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// Nombre del producto en el momento de la orden
    /// </summary>
    public required string ProductName { get; set; }
    
    /// <summary>
    /// SKU del producto
    /// </summary>
    public required string ProductSku { get; set; }
    
    /// <summary>
    /// Cantidad del producto
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Precio unitario en el momento de la orden
    /// </summary>
    public decimal UnitPrice { get; set; }
    
    /// <summary>
    /// Precio total del item (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice { get; set; }
    
    /// <summary>
    /// Descuento aplicado al item
    /// </summary>
    public decimal Discount { get; set; }
    
    /// <summary>
    /// Notas específicas del item
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Historial de cambios de estado de una orden
/// </summary>
public class OrderStatusHistory : BaseEntity
{
    /// <summary>
    /// ID de la orden
    /// </summary>
    public Guid OrderId { get; set; }
    
    /// <summary>
    /// Navegación hacia la orden
    /// </summary>
    public virtual Order Order { get; set; } = null!;
    
    /// <summary>
    /// Estado anterior
    /// </summary>
    public OrderStatus PreviousStatus { get; set; }
    
    /// <summary>
    /// Nuevo estado
    /// </summary>
    public OrderStatus NewStatus { get; set; }
    
    /// <summary>
    /// Fecha del cambio
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Usuario que realizó el cambio
    /// </summary>
    public Guid? ChangedBy { get; set; }
    
    /// <summary>
    /// Razón del cambio
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Comentarios adicionales
    /// </summary>
    public string? Comments { get; set; }
}
