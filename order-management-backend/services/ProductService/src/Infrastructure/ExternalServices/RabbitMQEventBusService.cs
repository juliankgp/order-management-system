using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProductService.Application.Interfaces;
using RabbitMQ.Client;
using System.Text;

namespace ProductService.Infrastructure.ExternalServices;

/// <summary>
/// Implementación del servicio de eventos usando RabbitMQ
/// </summary>
public class RabbitMQEventBusService : IEventBusService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQEventBusService> _logger;
    private readonly string _exchangeName;

    public RabbitMQEventBusService(ILogger<RabbitMQEventBusService> logger)
    {
        _logger = logger;
        _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "products";

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
                VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection("ProductService");
            _channel = _connection.CreateModel();

            // Declarar el exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established successfully for ProductService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection for ProductService");
            throw;
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var routingKey = GetRoutingKey<T>();
        await PublishEventAsync(@event, routingKey, cancellationToken);
    }

    private async Task PublishEventAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : class
    {
        try
        {
            var eventType = @event.GetType().Name;
            _logger.LogInformation("Publishing event {EventType} with routing key {RoutingKey}", eventType, routingKey);

            var message = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Formatting.None
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventType;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.MessageId = Guid.NewGuid().ToString();

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Event {EventType} published successfully with message ID {MessageId}", 
                eventType, properties.MessageId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", @event.GetType().Name);
            throw;
        }
    }

    private static string GetRoutingKey<T>() where T : class
    {
        var eventType = typeof(T).Name;
        
        // Convertir nombres de eventos a routing keys
        return eventType switch
        {
            "ProductStockUpdatedEvent" => "products.stock.updated",
            "ProductCreatedEvent" => "products.created",
            "ProductUpdatedEvent" => "products.updated",
            "ProductDeletedEvent" => "products.deleted",
            _ => $"products.{eventType.ToLowerInvariant()}"
        };
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQ connection disposed for ProductService");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection for ProductService");
        }
    }
}