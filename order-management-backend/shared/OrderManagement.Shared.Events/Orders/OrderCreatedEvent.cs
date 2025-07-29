using OrderManagement.Shared.Events.Abstractions;

namespace OrderManagement.Shared.Events.Orders;

/// <summary>
/// Evento publicado cuando se crea una nueva orden
/// </summary>
public record OrderCreatedEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    
    /// <summary>
    /// ID de la orden creada
    /// </summary>
    public required Guid OrderId { get; init; }
    
    /// <summary>
    /// ID del cliente que realizó la orden
    /// </summary>
    public required Guid CustomerId { get; init; }
    
    /// <summary>
    /// Total de la orden
    /// </summary>
    public required decimal TotalAmount { get; init; }
    
    /// <summary>
    /// Items de la orden con sus cantidades
    /// </summary>
    public required List<OrderItemCreated> Items { get; init; }
}

/// <summary>
/// Representa un item en la orden creada
/// </summary>
public record OrderItemCreated
{
    public required Guid ProductId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
}
