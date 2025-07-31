using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries.GetOrders;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Models;
using Xunit;

namespace OrderService.Tests.Application;

public class GetOrdersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetOrdersQueryHandler>> _loggerMock;
    private readonly GetOrdersQueryHandler _handler;

    public GetOrdersQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productServiceMock = new Mock<IProductService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GetOrdersQueryHandler>>();

        _handler = new GetOrdersQueryHandler(
            _unitOfWorkMock.Object,
            _productServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
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
                OrderNumber = "ORD-001",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                ShippingAddress = "123 Test St",
                ShippingCity = "Test City",
                ShippingZipCode = "12345",
                ShippingCountry = "Test Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test Product", ProductSku = "TEST-001", Quantity = 1, UnitPrice = 10.00m }
                }
            },
            new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                OrderNumber = "ORD-002",
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                ShippingAddress = "456 Test Ave",
                ShippingCity = "Test City",
                ShippingZipCode = "54321",
                ShippingCountry = "Test Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test Product 2", ProductSku = "TEST-002", Quantity = 2, UnitPrice = 20.00m }
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
                        Id = orders[0].Items.First().Id,
                        ProductId = orders[0].Items.First().ProductId,
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
                        Id = orders[1].Items.First().Id,
                        ProductId = orders[1].Items.First().ProductId,
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
            CurrentPage = 1,
            PageSize = 10
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetPagedAsync(1, 10, null, null, null, null, null, CancellationToken.None))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);

        result.Items.First().Status.Should().Be("Pending");
        result.Items.Last().Status.Should().Be("Processing");

        _unitOfWorkMock.Verify(x => x.Orders.GetPagedAsync(1, 10, null, null, null, null, null, CancellationToken.None), Times.Once);
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
                OrderNumber = "ORD-003",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                ShippingAddress = "789 Test Blvd",
                ShippingCity = "Test City",
                ShippingZipCode = "67890",
                ShippingCountry = "Test Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test Product", ProductSku = "TEST-001", Quantity = 1, UnitPrice = 10.00m }
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
                        Id = orders[0].Items.First().Id,
                        ProductId = orders[0].Items.First().ProductId,
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
            CurrentPage = 1,
            PageSize = 10
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetPagedAsync(1, 10, customerId, null, null, null, null, CancellationToken.None))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CustomerId.Should().Be(customerId);

        _unitOfWorkMock.Verify(x => x.Orders.GetPagedAsync(1, 10, customerId, null, null, null, null, CancellationToken.None), Times.Once);
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
                OrderNumber = "ORD-004",
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                ShippingAddress = "321 Test Lane",
                ShippingCity = "Test City",
                ShippingZipCode = "13579",
                ShippingCountry = "Test Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test Product", ProductSku = "TEST-001", Quantity = 1, UnitPrice = 10.00m }
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
                        Id = orders[0].Items.First().Id,
                        ProductId = orders[0].Items.First().ProductId,
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
            CurrentPage = 1,
            PageSize = 10
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetPagedAsync(1, 10, null, "Processing", null, null, null, CancellationToken.None))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("Processing");

        _unitOfWorkMock.Verify(x => x.Orders.GetPagedAsync(1, 10, null, "Processing", null, null, null, CancellationToken.None), Times.Once);
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
            CurrentPage = 1,
            PageSize = 10
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetPagedAsync(1, 10, null, null, null, null, null, CancellationToken.None))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(It.IsAny<List<Order>>()))
            .Returns(new List<OrderDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(0);

        _unitOfWorkMock.Verify(x => x.Orders.GetPagedAsync(1, 10, null, null, null, null, null, CancellationToken.None), Times.Once);
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
                OrderNumber = "ORD-005",
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ShippingAddress = "654 Test Way",
                ShippingCity = "Test City",
                ShippingZipCode = "24680",
                ShippingCountry = "Test Country",
                Items = new List<OrderItem>
                {
                    new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test Product", ProductSku = "TEST-001", Quantity = 1, UnitPrice = 10.00m }
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
                        Id = orders[0].Items.First().Id,
                        ProductId = orders[0].Items.First().ProductId,
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
            CurrentPage = 1,
            PageSize = 10
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetPagedAsync(1, 10, null, null, fromDate, toDate, null, CancellationToken.None))
            .ReturnsAsync(pagedResult);

        _mapperMock.Setup(x => x.Map<List<OrderDto>>(orders))
            .Returns(orderDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().CreatedAt.Should().BeAfter(fromDate).And.BeBefore(toDate);

        _unitOfWorkMock.Verify(x => x.Orders.GetPagedAsync(1, 10, null, null, fromDate, toDate, null, CancellationToken.None), Times.Once);
        _mapperMock.Verify(x => x.Map<List<OrderDto>>(orders), Times.Once);
    }
}