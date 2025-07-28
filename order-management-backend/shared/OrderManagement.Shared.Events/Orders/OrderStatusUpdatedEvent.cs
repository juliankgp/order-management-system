using OrderManagement.Shared.Events.Abstractions;

namespace OrderManagement.Shared.Events.Orders;

/// <summary>
/// Evento publicado cuando se actualiza el estado de una orden
/// </summary>
public record OrderStatusUpdatedEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    
    /// <summary>
    /// ID de la orden actualizada
    /// </summary>
    public required Guid OrderId { get; init; }
    
    /// <summary>
    /// Estado anterior de la orden
    /// </summary>
    public required string PreviousStatus { get; init; }
    
    /// <summary>
    /// Nuevo estado de la orden
    /// </summary>
    public required string NewStatus { get; init; }
    
    /// <summary>
    /// ID del usuario que realizó el cambio
    /// </summary>
    public Guid? UpdatedBy { get; init; }
    
    /// <summary>
    /// Razón del cambio de estado
    /// </summary>
    public string? Reason { get; init; }
}
