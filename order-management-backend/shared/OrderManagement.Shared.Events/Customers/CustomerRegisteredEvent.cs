using OrderManagement.Shared.Events.Abstractions;

namespace OrderManagement.Shared.Events.Customers;

/// <summary>
/// Evento publicado cuando se registra un nuevo cliente
/// </summary>
public record CustomerRegisteredEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
    
    /// <summary>
    /// ID del cliente registrado
    /// </summary>
    public required Guid CustomerId { get; init; }
    
    /// <summary>
    /// Email del cliente
    /// </summary>
    public required string Email { get; init; }
    
    /// <summary>
    /// Nombre completo del cliente
    /// </summary>
    public required string FullName { get; init; }
    
    /// <summary>
    /// Fecha de registro
    /// </summary>
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
}
