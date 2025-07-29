using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.Commands.UpdateOrder;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class UpdateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<ICustomerService> _customerServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IEventBusService> _eventBusServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UpdateOrderCommandHandler _handler;

    public UpdateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _customerServiceMock = new Mock<ICustomerService>();
        _productServiceMock = new Mock<IProductService>();
        _eventBusServiceMock = new Mock<IEventBusService>();
        _mapperMock = new Mock<IMapper>();

        _handler = new UpdateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _customerServiceMock.Object,
            _productServiceMock.Object,
            _eventBusServiceMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUpdateOrder_ShouldUpdateOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var updateOrderDto = new UpdateOrderDto
        {
            CustomerId = customerId,
            Notes = "Updated notes",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = productId, Quantity = 3 }
            }
        };

        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Notes = "Original notes",
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 1, UnitPrice = 10.00m }
            }
        };

        var customerResponse = new CustomerResponseDto
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com"
        };

        var productResponse = new ProductResponseDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 15.00m,
            Stock = 100
        };

        var orderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            Status = "Pending",
            TotalAmount = 45.00m,
            Notes = "Updated notes",
            CreatedAt = DateTime.UtcNow,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 3,
                    UnitPrice = 15.00m,
                    Subtotal = 45.00m
                }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _customerServiceMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customerResponse);

        _productServiceMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productResponse);

        _orderRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        _mapperMock.Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(orderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.TotalAmount.Should().Be(45.00m);
        result.Notes.Should().Be("Updated notes");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _customerServiceMock.Verify(x => x.GetCustomerAsync(customerId), Times.Once);
        _productServiceMock.Verify(x => x.GetProductAsync(productId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Order with ID {orderId} not found");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderNotPending_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Shipped
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Cannot update order that is not in pending status");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CustomerNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _customerServiceMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync((CustomerResponseDto?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Customer with ID {customerId} not found");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _customerServiceMock.Verify(x => x.GetCustomerAsync(customerId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var updateOrderDto = new UpdateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = productId, Quantity = 50 }
            }
        };

        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending
        };

        var customerResponse = new CustomerResponseDto
        {
            Id = customerId,
            Name = "John Doe",
            Email = "john@example.com"
        };

        var productResponse = new ProductResponseDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 10.00m,
            Stock = 10 // Insufficient stock
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _customerServiceMock.Setup(x => x.GetCustomerAsync(customerId))
            .ReturnsAsync(customerResponse);

        _productServiceMock.Setup(x => x.GetProductAsync(productId))
            .ReturnsAsync(productResponse);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage($"Insufficient stock for product {productId}. Available: 10, Requested: 50");

        _orderRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Order>()), Times.Never);
    }
}
