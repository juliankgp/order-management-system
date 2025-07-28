using OrderManagement.Shared.Events.Abstractions;

namespace OrderManagement.Shared.Events.Products;

/// <summary>
/// Evento publicado cuando se actualiza el stock de un producto
/// </summary>
public record ProductStockUpdatedEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    
    /// <summary>
    /// ID del producto actualizado
    /// </summary>
    public required Guid ProductId { get; init; }
    
    /// <summary>
    /// Stock anterior
    /// </summary>
    public required int PreviousStock { get; init; }
    
    /// <summary>
    /// Nuevo stock
    /// </summary>
    public required int NewStock { get; init; }
    
    /// <summary>
    /// Razón del cambio de stock
    /// </summary>
    public required string Reason { get; init; }
    
    /// <summary>
    /// ID de la orden relacionada (si aplica)
    /// </summary>
    public Guid? OrderId { get; init; }
}
