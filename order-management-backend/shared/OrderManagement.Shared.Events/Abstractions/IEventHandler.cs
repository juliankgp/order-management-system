namespace OrderManagement.Shared.Events.Abstractions;

/// <summary>
/// Interface para handlers de eventos
/// </summary>
/// <typeparam name="TEvent">Tipo del evento a manejar</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Maneja el evento recibido
    /// </summary>
    /// <param name="event">Evento a procesar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
