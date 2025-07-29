namespace ProductService.Application.Interfaces;

/// <summary>
/// Interface para el servicio de eventos
/// </summary>
public interface IEventBusService
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}