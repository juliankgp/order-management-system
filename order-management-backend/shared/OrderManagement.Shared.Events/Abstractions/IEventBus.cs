namespace OrderManagement.Shared.Events.Abstractions;

/// <summary>
/// Interface para el bus de eventos
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publica un evento en el bus
    /// </summary>
    /// <typeparam name="TEvent">Tipo del evento</typeparam>
    /// <param name="event">Evento a publicar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
    
    /// <summary>
    /// Suscribe un handler a un tipo de evento
    /// </summary>
    /// <typeparam name="TEvent">Tipo del evento</typeparam>
    /// <typeparam name="THandler">Tipo del handler</typeparam>
    void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>;
}
