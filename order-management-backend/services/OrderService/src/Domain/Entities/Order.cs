using OrderManagement.Shared.Common.Models;

namespace OrderService.Domain.Entities;

/// <summary>
/// Entidad que representa una orden en el sistema
/// </summary>
public class Order : BaseEntity
{
    /// <summary>
    /// ID del cliente que realizó la orden
    /// </summary>
    public Guid CustomerId { get; set; }
    
    /// <summary>
    /// Número de orden único
    /// </summary>
    public required string OrderNumber { get; set; }
    
    /// <summary>
    /// Estado actual de la orden
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    /// <summary>
    /// Fecha de la orden
    /// </summary>
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Total de la orden
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// Subtotal antes de impuestos
    /// </summary>
    public decimal SubTotal { get; set; }
    
    /// <summary>
    /// Monto de impuestos
    /// </summary>
    public decimal TaxAmount { get; set; }
    
    /// <summary>
    /// Costo de envío
    /// </summary>
    public decimal ShippingCost { get; set; }
    
    /// <summary>
    /// Dirección de envío
    /// </summary>
    public required string ShippingAddress { get; set; }
    
    /// <summary>
    /// Ciudad de envío
    /// </summary>
    public required string ShippingCity { get; set; }
    
    /// <summary>
    /// Código postal de envío
    /// </summary>
    public required string ShippingZipCode { get; set; }
    
    /// <summary>
    /// País de envío
    /// </summary>
    public required string ShippingCountry { get; set; }
    
    /// <summary>
    /// Notas adicionales de la orden
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Items incluidos en la orden
    /// </summary>
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    /// <summary>
    /// Historial de cambios de estado
    /// </summary>
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}

/// <summary>
/// Estados posibles de una orden
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Orden pendiente de procesamiento
    /// </summary>
    Pending = 1,
    
    /// <summary>
    /// Orden confirmada
    /// </summary>
    Confirmed = 2,
    
    /// <summary>
    /// Orden en procesamiento
    /// </summary>
    Processing = 3,
    
    /// <summary>
    /// Orden enviada
    /// </summary>
    Shipped = 4,
    
    /// <summary>
    /// Orden entregada
    /// </summary>
    Delivered = 5,
    
    /// <summary>
    /// Orden cancelada
    /// </summary>
    Cancelled = 6
}
