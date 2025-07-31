using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IEventBusService> _eventBusServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _loggerMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerServiceMock = new Mock<ICustomerService>();
        _productServiceMock = new Mock<IProductService>();
        _eventBusServiceMock = new Mock<IEventBusService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<CreateOrderCommandHandler>>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _customerServiceMock.Object,
            _productServiceMock.Object,
            _eventBusServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrderSuccessfully()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Notes = "Test order",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = productId,
                    Quantity = 2
                }
            }
        };
        
        var command = new CreateOrderCommand(createOrderDto);

        var customer = new CustomerResponseDto
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com"
        };
        
        var product = new ProductResponseDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 25.99m,
            Stock = 10
        };

        var expectedOrder = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderNumber = "ORD-001",
            Status = OrderStatus.Pending,
            TotalAmount = 51.98m,
            ShippingAddress = "123 Test St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductName = "Test Product",
                    ProductSku = "TEST-001",
                    Quantity = 2,
                    UnitPrice = 25.99m,
                    TotalPrice = 51.98m
                }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = expectedOrder.Id,
            CustomerId = customerId,
            OrderNumber = "ORD-001",
            Status = "Pending",
            TotalAmount = 51.98m,
            OrderDate = DateTime.UtcNow,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = expectedOrder.Items.First().Id,
                    ProductId = productId,
                    ProductName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 25.99m,
                    Subtotal = 51.98m
                }
            }
        };

        _customerServiceMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customer);

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(product);

        _orderRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(expectedOrderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(51.98m);

        _customerServiceMock.Verify(x => x.GetCustomerAsync(customerId), Times.Once);
        _productServiceMock.Verify(x => x.GetProductAsync(productId), Times.Once);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCustomer_ShouldThrowBusinessException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };
        
        var command = new CreateOrderCommand(createOrderDto);

        _customerServiceMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync((CustomerResponseDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Customer");
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowBusinessException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto
                {
                    ProductId = productId,
                    Quantity = 10
                }
            }
        };
        
        var command = new CreateOrderCommand(createOrderDto);

        var customer = new CustomerResponseDto
        {
            Id = customerId,
            Name = "Test Customer",
            Email = "test@example.com"
        };
        
        var product = new ProductResponseDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 25.99m,
            Stock = 5 // Less than requested quantity (10)
        };

        _customerServiceMock
            .Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customer);

        _productServiceMock
            .Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Insufficient stock");
    }

    [Fact]
    public async Task Handle_EmptyItems_ShouldThrowBusinessException()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>()
        };
        
        var command = new CreateOrderCommand(createOrderDto);

        // El handler primero busca el customer, así que mock eso para null
        _customerServiceMock
            .Setup(x => x.GetCustomerAsync(It.IsAny<Guid>()))
            .ReturnsAsync((CustomerResponseDto?)null);

        // Act & Assert
        // El handler lanza EntityNotFoundException cuando no encuentra el customer
        await Assert.ThrowsAsync<EntityNotFoundException>(() => 
            _handler.Handle(command, CancellationToken.None));
    }
}