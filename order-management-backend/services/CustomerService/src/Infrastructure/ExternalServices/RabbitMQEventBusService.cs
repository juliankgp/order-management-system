using CustomerService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace CustomerService.Infrastructure.ExternalServices;

/// <summary>
/// Servicio de eventos usando RabbitMQ
/// </summary>
public class RabbitMQEventBusService : IEventBusService, IDisposable
{
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private readonly string _exchangeName;
    private readonly ILogger<RabbitMQEventBusService> _logger;

    public RabbitMQEventBusService(IConfiguration configuration, ILogger<RabbitMQEventBusService> logger)
    {
        _logger = logger;
        _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "order_management_exchange";

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

            _connection = factory.CreateConnection("CustomerService-Publisher");
            _channel = _connection.CreateModel();

            // Declarar el exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQEventBusService initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize RabbitMQEventBusService - Events will not be published");
        }
    }

    public async Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken = default) where T : class
    {
        if (_channel == null || _connection == null)
        {
            _logger.LogWarning("Cannot publish event - RabbitMQ connection not available");
            return;
        }

        try
        {
            var message = JsonConvert.SerializeObject(@event, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _exchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Event published successfully with routing key: {RoutingKey}", routingKey);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event with routing key: {RoutingKey}", routingKey);
            throw;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("RabbitMQEventBusService disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQEventBusService");
        }
    }
}