using FluentAssertions;
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
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IEventBusService> _eventBusServiceMock;
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _eventBusServiceMock = new Mock<IEventBusService>();

        _handler = new DeleteOrderCommandHandler(
            _orderRepositoryMock.Object,
            _eventBusServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrderId_ShouldDeleteOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            Notes = "Test order",
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.00m }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock.Setup(x => x.DeleteAsync(orderId))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Order with ID {orderId} not found");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task Handle_OrderNotPending_ShouldThrowBusinessException(OrderStatus status)
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = status
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<BusinessException>()
            .WithMessage("Cannot delete order that is not in pending status");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var command = new DeleteOrderCommand(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(existingOrder);

        _orderRepositoryMock.Setup(x => x.DeleteAsync(orderId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await _handler.Invoking(x => x.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.DeleteAsync(orderId), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Never);
    }
}
