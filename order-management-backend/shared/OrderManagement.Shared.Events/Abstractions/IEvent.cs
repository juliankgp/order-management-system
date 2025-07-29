namespace OrderManagement.Shared.Events.Abstractions;

/// <summary>
/// Interface base para todos los eventos de dominio
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp de cuando ocurrió el evento
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Versión del evento para versionado
    /// </summary>
    int Version { get; }
}
