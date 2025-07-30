using LoggingService.Application.Commands.CreateLog;
using LoggingService.Domain.Entities;
using MediatR;
using LogLevel = LoggingService.Domain.Entities.LogLevel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Events.Abstractions;
using OrderManagement.Shared.Events.Customers;
using OrderManagement.Shared.Events.Orders;
using OrderManagement.Shared.Events.Products;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace LoggingService.Infrastructure.ExternalServices;

public class RabbitMQEventBusService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQEventBusService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQEventBusService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQEventBusService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            InitRabbitMQ();
            await base.StartAsync(cancellationToken);
            _logger.LogInformation("RabbitMQ Event Bus Service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ Event Bus Service");
            throw;
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_connection?.IsOpen != true)
                {
                    _logger.LogWarning("RabbitMQ connection is not open, attempting to reconnect...");
                    InitRabbitMQ();
                }
                
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMQ Event Bus Service execution loop");
                await Task.Delay(10000, stoppingToken);
            }
        }
    }

    private void InitRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declarar exchanges
            _channel.ExchangeDeclare("orders", ExchangeType.Topic, true);
            _channel.ExchangeDeclare("customers", ExchangeType.Topic, true);
            _channel.ExchangeDeclare("products", ExchangeType.Topic, true);

            // Crear cola para el servicio de logging
            var queueName = "logging.events";
            _channel.QueueDeclare(queueName, true, false, false, null);

            // Bind a todos los eventos
            _channel.QueueBind(queueName, "orders", "order.created");
            _channel.QueueBind(queueName, "orders", "order.status.updated");
            _channel.QueueBind(queueName, "customers", "customer.registered");
            _channel.QueueBind(queueName, "products", "product.stock.updated");

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnEventReceived;

            _channel.BasicConsume(queueName, false, consumer);

            _logger.LogInformation("RabbitMQ connection and consumers configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw;
        }
    }

    private async void OnEventReceived(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            _logger.LogDebug("Received event: {RoutingKey} - {Message}", e.RoutingKey, message);

            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var logCommand = e.RoutingKey switch
            {
                "order.created" => await ProcessOrderCreatedEvent(message),
                "order.status.updated" => await ProcessOrderStatusUpdatedEvent(message),
                "customer.registered" => await ProcessCustomerRegisteredEvent(message),
                "product.stock.updated" => await ProcessProductStockUpdatedEvent(message),
                _ => null
            };

            if (logCommand != null)
            {
                await mediator.Send(logCommand);
                _channel?.BasicAck(e.DeliveryTag, false);
                _logger.LogDebug("Successfully processed event: {RoutingKey}", e.RoutingKey);
            }
            else
            {
                _logger.LogWarning("Unknown event type: {RoutingKey}", e.RoutingKey);
                _channel?.BasicNack(e.DeliveryTag, false, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event: {RoutingKey}", e.RoutingKey);
            _channel?.BasicNack(e.DeliveryTag, false, true);
        }
    }

    private async Task<CreateLogCommand?> ProcessOrderCreatedEvent(string message)
    {
        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
            if (orderEvent == null) return null;

            return new CreateLogCommand
            {
                Level = LogLevel.Information,
                Message = $"Order created: {orderEvent.OrderId}",
                ServiceName = "OrderService",
                Category = "OrderCreated",
                CorrelationId = orderEvent.EventId.ToString(),
                UserId = orderEvent.CustomerId,
                Properties = JsonSerializer.Serialize(new
                {
                    orderEvent.OrderId,
                    orderEvent.CustomerId,
                    orderEvent.TotalAmount,
                    ItemCount = orderEvent.Items?.Count ?? 0
                }),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ApplicationVersion = "1.0.0"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderCreatedEvent");
            return null;
        }
    }

    private async Task<CreateLogCommand?> ProcessOrderStatusUpdatedEvent(string message)
    {
        try
        {
            var orderEvent = JsonSerializer.Deserialize<OrderStatusUpdatedEvent>(message);
            if (orderEvent == null) return null;

            return new CreateLogCommand
            {
                Level = LogLevel.Information,
                Message = $"Order status updated: {orderEvent.OrderId} -> {orderEvent.NewStatus}",
                ServiceName = "OrderService",
                Category = "OrderStatusUpdated",
                CorrelationId = orderEvent.EventId.ToString(),
                Properties = JsonSerializer.Serialize(new
                {
                    orderEvent.OrderId,
                    orderEvent.PreviousStatus,
                    orderEvent.NewStatus,
                    orderEvent.UpdatedBy,
                    orderEvent.Reason
                }),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ApplicationVersion = "1.0.0"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OrderStatusUpdatedEvent");
            return null;
        }
    }

    private async Task<CreateLogCommand?> ProcessCustomerRegisteredEvent(string message)
    {
        try
        {
            var customerEvent = JsonSerializer.Deserialize<CustomerRegisteredEvent>(message);
            if (customerEvent == null) return null;

            return new CreateLogCommand
            {
                Level = LogLevel.Information,
                Message = $"Customer registered: {customerEvent.Email}",
                ServiceName = "CustomerService",
                Category = "CustomerRegistered",
                CorrelationId = customerEvent.EventId.ToString(),
                UserId = customerEvent.CustomerId,
                Properties = JsonSerializer.Serialize(new
                {
                    customerEvent.CustomerId,
                    customerEvent.Email,
                    customerEvent.FullName,
                    customerEvent.RegisteredAt
                }),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ApplicationVersion = "1.0.0"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CustomerRegisteredEvent");
            return null;
        }
    }

    private async Task<CreateLogCommand?> ProcessProductStockUpdatedEvent(string message)
    {
        try
        {
            var productEvent = JsonSerializer.Deserialize<ProductStockUpdatedEvent>(message);
            if (productEvent == null) return null;

            return new CreateLogCommand
            {
                Level = LogLevel.Information,
                Message = $"Product stock updated: {productEvent.ProductId}",
                ServiceName = "ProductService",
                Category = "ProductStockUpdated",
                CorrelationId = productEvent.EventId.ToString(),
                Properties = JsonSerializer.Serialize(new
                {
                    productEvent.ProductId,
                    productEvent.NewStock,
                    productEvent.Reason
                }),
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                ApplicationVersion = "1.0.0"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ProductStockUpdatedEvent");
            return null;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ Event Bus Service...");
        
        _channel?.Close();
        _connection?.Close();
        
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("RabbitMQ Event Bus Service stopped");
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}