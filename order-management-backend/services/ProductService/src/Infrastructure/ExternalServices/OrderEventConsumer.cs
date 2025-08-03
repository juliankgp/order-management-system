using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderManagement.Shared.Events.Orders;
using ProductService.Application.Commands.UpdateStock;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace ProductService.Infrastructure.ExternalServices;

/// <summary>
/// Servicio para consumir eventos de órdenes desde RabbitMQ
/// </summary>
public class OrderEventConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderEventConsumer> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly string _queueName;

    public OrderEventConsumer(
        IServiceProvider serviceProvider,
        ILogger<OrderEventConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _exchangeName = Environment.GetEnvironmentVariable("RABBITMQ_EXCHANGE") ?? "orders";
        _queueName = "product_service_queue";

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

            _connection = factory.CreateConnection("ProductService-Consumer");
            _channel = _connection.CreateModel();

            // Declarar el exchange
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            // Declarar la cola
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            // Bind para eventos de órdenes
            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: "orders.created");

            _channel.QueueBind(
                queue: _queueName,
                exchange: _exchangeName,
                routingKey: "orders.status.updated");

            _logger.LogInformation("OrderEventConsumer initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initialize OrderEventConsumer - RabbitMQ may not be available. Service will continue without event consumption.");
            // Don't throw - allow service to start without RabbitMQ
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_connection == null || _channel == null)
        {
            _logger.LogWarning("OrderEventConsumer is not connected to RabbitMQ. Event consumption disabled.");
            
            // Mantener el servicio corriendo pero sin consumir mensajes
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(5000, stoppingToken);
            }
            return;
        }

        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var routingKey = ea.RoutingKey;

                _logger.LogInformation("Received message with routing key: {RoutingKey}", routingKey);

                await ProcessMessage(message, routingKey);

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("OrderEventConsumer started consuming messages");

        // Mantener el servicio corriendo
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessage(string message, string routingKey)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            switch (routingKey)
            {
                case "orders.created":
                    var orderCreatedEvent = JsonConvert.DeserializeObject<OrderCreatedEvent>(message);
                    if (orderCreatedEvent != null)
                    {
                        await HandleOrderCreatedEvent(orderCreatedEvent, mediator);
                    }
                    break;

                case "orders.status.updated":
                    var orderStatusUpdatedEvent = JsonConvert.DeserializeObject<OrderStatusUpdatedEvent>(message);
                    if (orderStatusUpdatedEvent != null)
                    {
                        await HandleOrderStatusUpdatedEvent(orderStatusUpdatedEvent, mediator);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown routing key: {RoutingKey}", routingKey);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message for routing key: {RoutingKey}", routingKey);
            throw;
        }
    }

    private async Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent, IMediator mediator)
    {
        _logger.LogInformation("Processing OrderCreatedEvent for order {OrderId}", orderCreatedEvent.OrderId);

        foreach (var item in orderCreatedEvent.Items)
        {
            // Reducir stock del producto
            var updateStockCommand = new UpdateStockCommand(
                item.ProductId,
                -item.Quantity, // Cantidad negativa para reducir stock
                $"Order created: {orderCreatedEvent.OrderId}",
                orderCreatedEvent.OrderId.ToString(),
                null);

            var result = await mediator.Send(updateStockCommand);
            
            if (!result)
            {
                _logger.LogWarning("Failed to update stock for product {ProductId} in order {OrderId}", 
                    item.ProductId, orderCreatedEvent.OrderId);
            }
            else
            {
                _logger.LogInformation("Stock updated successfully for product {ProductId}, quantity: {Quantity}", 
                    item.ProductId, -item.Quantity);
            }
        }
    }

    private async Task HandleOrderStatusUpdatedEvent(OrderStatusUpdatedEvent orderStatusUpdatedEvent, IMediator mediator)
    {
        _logger.LogInformation("Processing OrderStatusUpdatedEvent for order {OrderId}, new status: {Status}", 
            orderStatusUpdatedEvent.OrderId, orderStatusUpdatedEvent.NewStatus);

        // Por ahora solo logeamos el evento - se puede implementar lógica adicional más tarde
        _logger.LogInformation("Order status updated event processed for order {OrderId}", orderStatusUpdatedEvent.OrderId);
        
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _logger.LogInformation("OrderEventConsumer disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing OrderEventConsumer");
        }
        finally
        {
            base.Dispose();
        }
    }
}