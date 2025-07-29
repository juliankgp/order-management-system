using AutoMapper;
using FluentAssertions;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Queries.GetOrders;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Models;
using Xunit;

namespace OrderService.Tests.Application;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetOrdersQueryHandler(
            _orderRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.00m }
                }
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 20.00m }
                }
            }
        };

        var orderDtos = new List<OrderDto>
        {
            new OrderDto
            {
                Id = orders[0].Id,
                CustomerId = orders[0].CustomerId,
                Status = "Pending",
                TotalAmount = 10.00m,
                CreatedAt = orders[0].CreatedAt,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = orders[0].OrderItems.First().Id,
                        ProductId = orders[0].OrderItems.First().ProductId,
                        Quantity = 1,
                        UnitPrice = 10.00m,
                        Subtotal = 10.00m
                    }
                }
            },
            new OrderDto
            {
                Id = orders[1].Id,
                CustomerId = orders[1].CustomerId,
                Status = "Processing",
                TotalAmount = 40.00m,
                CreatedAt = orders[1].CreatedAt,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = orders[1].OrderItems.First().Id,
                        ProductId = orders[1].OrderItems.First().ProductId,
                        Quantity = 2,
                        UnitPrice = 20.00m,
                        Subtotal = 40.00m
                    }
                }
            }
        };

        var pagedResult = new PagedResult<Order>
        {
            Items = orders,
            TotalCount = 2,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(1, 10, null, null, null))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);

        result.Items.First().Status.Should().Be("Pending");
        result.Items.Last().Status.Should().Be("Processing");

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, null, null, null), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(orders), Times.Once);
    }

    [Fact]
    public async Task Handle_QueryWithCustomerId_ShouldFilterByCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10,
            CustomerId = customerId
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.00m }
                }
            }
        };

        var orderDtos = new List<OrderDto>
        {
            new OrderDto
            {
                Id = orders[0].Id,
                CustomerId = customerId,
                Status = "Pending",
                TotalAmount = 10.00m,
                CreatedAt = orders[0].CreatedAt,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = orders[0].OrderItems.First().Id,
                        ProductId = orders[0].OrderItems.First().ProductId,
                        Quantity = 1,
                        UnitPrice = 10.00m,
                        Subtotal = 10.00m
                    }
                }
            }
        };

        var pagedResult = new PagedResult<Order>
        {
            Items = orders,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(1, 10, customerId, null, null))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CustomerId.Should().Be(customerId);

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, customerId, null, null), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(orders), Times.Once);
    }

    [Fact]
    public async Task Handle_QueryWithStatus_ShouldFilterByStatus()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10,
            Status = "Processing"
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.00m }
                }
            }
        };

        var orderDtos = new List<OrderDto>
        {
            new OrderDto
            {
                Id = orders[0].Id,
                CustomerId = orders[0].CustomerId,
                Status = "Processing",
                TotalAmount = 10.00m,
                CreatedAt = orders[0].CreatedAt,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = orders[0].OrderItems.First().Id,
                        ProductId = orders[0].OrderItems.First().ProductId,
                        Quantity = 1,
                        UnitPrice = 10.00m,
                        Subtotal = 10.00m
                    }
                }
            }
        };

        var pagedResult = new PagedResult<Order>
        {
            Items = orders,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(1, 10, null, OrderStatus.Processing, null))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("Processing");

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, null, OrderStatus.Processing, null), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(orders), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyResult_ShouldReturnEmptyPagedResult()
    {
        // Arrange
        var query = new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10
        };

        var pagedResult = new PagedResult<Order>
        {
            Items = new List<Order>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            TotalPages = 0
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(1, 10, null, null, null))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(It.IsAny<List<Order>>()))
            .Returns(new List<OrderDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(0);

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, null, null, null), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(It.IsAny<List<Order>>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QueryWithDateRange_ShouldFilterByDateRange()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-7);
        var toDate = DateTime.UtcNow;
        
        var query = new GetOrdersQuery
        {
            Page = 1,
            PageSize = 10,
            FromDate = fromDate,
            ToDate = toDate
        };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 10.00m }
                }
            }
        };

        var orderDtos = new List<OrderDto>
        {
            new OrderDto
            {
                Id = orders[0].Id,
                CustomerId = orders[0].CustomerId,
                Status = "Pending",
                TotalAmount = 10.00m,
                CreatedAt = orders[0].CreatedAt,
                Items = new List<OrderItemDto>
                {
                    new OrderItemDto
                    {
                        Id = orders[0].OrderItems.First().Id,
                        ProductId = orders[0].OrderItems.First().ProductId,
                        Quantity = 1,
                        UnitPrice = 10.00m,
                        Subtotal = 10.00m
                    }
                }
            }
        };

        var pagedResult = new PagedResult<Order>
        {
            Items = orders,
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _orderRepositoryMock.Setup(x => x.GetPagedAsync(1, 10, null, null, It.Is<(DateTime From, DateTime To)?>(
                range => range!.Value.From == fromDate && range.Value.To == toDate)))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CreatedAt.Should().BeAfter(fromDate).And.BeBefore(toDate);

        _orderRepositoryMock.Verify(x => x.GetPagedAsync(1, 10, null, null, It.IsAny<(DateTime From, DateTime To)?>()), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(orders), Times.Once);
    }
}
