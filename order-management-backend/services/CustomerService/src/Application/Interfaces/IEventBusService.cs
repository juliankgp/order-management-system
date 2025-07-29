namespace CustomerService.Application.Interfaces;

/// <summary>
/// Servicio para el manejo de eventos
/// </summary>
public interface IEventBusService
{
    /// <summary>
    /// Publica un evento
    /// </summary>
    Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default) where T : class;
}