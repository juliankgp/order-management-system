using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.Commands.DeleteOrder;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventBusService> _eventBusServiceMock;
    private readonly Mock<ILogger<DeleteOrderCommandHandler>> _loggerMock;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventBusServiceMock = new Mock<IEventBusService>();
        _loggerMock = new Mock<ILogger<DeleteOrderCommandHandler>>();

        _handler = new DeleteOrderCommandHandler(
            _unitOfWorkMock.Object,
            _eventBusServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = OrderStatus.Pending,
            TotalAmount = 50.00m,
            ShippingAddress = "123 Test St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    ProductSku = "TEST-001",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    TotalPrice = 50.00m
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Orders.Update(existingOrder), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // El handler real no publica eventos para eliminación
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnFalse()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderNotPending_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            OrderNumber = "ORD-001",
            Status = OrderStatus.Delivered, // Cannot be deleted
            TotalAmount = 50.00m,
            ShippingAddress = "123 Test St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country",
            Items = new List<OrderItem>()
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Cannot delete order with status");
    }

    [Fact]
    public async Task Handle_ValidPendingOrder_ShouldMarkAsDeleted()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            OrderNumber = "ORD-002",
            Status = OrderStatus.Pending,
            TotalAmount = 100.00m,
            ShippingAddress = "456 Another St",
            ShippingCity = "Another City",
            ShippingZipCode = "54321",
            ShippingCountry = "Another Country",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(),
                    ProductName = "Another Product",
                    ProductSku = "TEST-002",
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    TotalPrice = 100.00m
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        existingOrder.IsDeleted.Should().BeTrue();
        existingOrder.DeletedAt.Should().NotBeNull();

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        // El handler real no publica eventos para eliminación
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Never);
    }
}