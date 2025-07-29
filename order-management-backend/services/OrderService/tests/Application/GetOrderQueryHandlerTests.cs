using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Queries.GetOrder;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetOrderQueryHandler(
            _orderRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrderId_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            Notes = "Test order",
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 15.00m
                }
            }
        };

        var orderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            Status = "Pending",
            Notes = "Test order",
            TotalAmount = 30.00m,
            CreatedAt = order.CreatedAt,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = order.OrderItems.First().Id,
                    ProductId = productId,
                    Quantity = 2,
                    UnitPrice = 15.00m,
                    Subtotal = 30.00m
                }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mapperMock.Setup(x => x.Map<OrderDto>(order))
            .Returns(orderDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.Status.Should().Be("Pending");
        result.TotalAmount.Should().Be(30.00m);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(productId);
        result.Items.First().Quantity.Should().Be(2);
        result.Items.First().Subtotal.Should().Be(30.00m);

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(order), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await _handler.Invoking(x => x.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Order with ID {orderId} not found");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderWithMultipleItems_ShouldReturnCompleteOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        var order = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Status = OrderStatus.Processing,
            Notes = "Multi-item order",
            CreatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product1Id,
                    Quantity = 1,
                    UnitPrice = 10.00m
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product2Id,
                    Quantity = 3,
                    UnitPrice = 20.00m
                }
            }
        };

        var orderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            Status = "Processing",
            Notes = "Multi-item order",
            TotalAmount = 70.00m,
            CreatedAt = order.CreatedAt,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = order.OrderItems.First().Id,
                    ProductId = product1Id,
                    Quantity = 1,
                    UnitPrice = 10.00m,
                    Subtotal = 10.00m
                },
                new OrderItemDto
                {
                    Id = order.OrderItems.Last().Id,
                    ProductId = product2Id,
                    Quantity = 3,
                    UnitPrice = 20.00m,
                    Subtotal = 60.00m
                }
            }
        };

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(order);

        _mapperMock.Setup(x => x.Map<OrderDto>(order))
            .Returns(orderDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalAmount.Should().Be(70.00m);
        result.Status.Should().Be("Processing");

        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(order), Times.Once);
    }
}
