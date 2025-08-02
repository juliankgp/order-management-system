using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Application.Queries.GetOrder;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using Xunit;

namespace OrderService.Tests.Application;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetOrderQueryHandler>> _loggerMock;
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productServiceMock = new Mock<IProductService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<GetOrderQueryHandler>>();

        _handler = new GetOrderQueryHandler(
            _unitOfWorkMock.Object,
            _productServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrderId_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var query = new GetOrderQuery(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            OrderNumber = "ORD-001",
            Status = OrderStatus.Pending,
            TotalAmount = 50.00m,
            OrderDate = DateTime.UtcNow.AddDays(-1),
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
                    UnitPrice = 25.00m,
                    TotalPrice = 50.00m
                }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            OrderNumber = "ORD-001",
            Status = "Pending",
            TotalAmount = 50.00m,
            OrderDate = existingOrder.OrderDate,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = existingOrder.Items.First().Id,
                    ProductId = productId,
                    ProductName = "Test Product",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    Subtotal = 50.00m
                }
            }
        };

        var products = new List<ProductResponseDto>
        {
            new ProductResponseDto
            {
                Id = productId,
                Name = "Test Product",
                Price = 25.00m
            }
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _productServiceMock.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mapperMock.Setup(x => x.Map<OrderDto>(existingOrder))
            .Returns(expectedOrderDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.CustomerId.Should().Be(customerId);
        result.OrderNumber.Should().Be("ORD-001");
        result.Status.Should().Be("Pending");
        result.TotalAmount.Should().Be(50.00m);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(productId);

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(existingOrder), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldReturnNull()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var query = new GetOrderQuery(orderId);

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OrderWithMultipleItems_ShouldReturnCompleteOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        
        var query = new GetOrderQuery(orderId);

        var existingOrder = new Order
        {
            Id = orderId,
            CustomerId = customerId,
            OrderNumber = "ORD-002",
            Status = OrderStatus.Processing,
            TotalAmount = 150.00m,
            OrderDate = DateTime.UtcNow.AddHours(-2),
            ShippingAddress = "456 Another St",
            ShippingCity = "Another City",
            ShippingZipCode = "54321",
            ShippingCountry = "Another Country",
            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId1,
                    ProductName = "Product 1",
                    ProductSku = "PROD-001",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    TotalPrice = 50.00m
                },
                new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId2,
                    ProductName = "Product 2",
                    ProductSku = "PROD-002",
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    TotalPrice = 100.00m
                }
            }
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = customerId,
            OrderNumber = "ORD-002",
            Status = "Processing",
            TotalAmount = 150.00m,
            OrderDate = existingOrder.OrderDate,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    Id = existingOrder.Items.First().Id,
                    ProductId = productId1,
                    ProductName = "Product 1",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    Subtotal = 50.00m
                },
                new OrderItemDto
                {
                    Id = existingOrder.Items.Last().Id,
                    ProductId = productId2,
                    ProductName = "Product 2",
                    Quantity = 1,
                    UnitPrice = 100.00m,
                    Subtotal = 100.00m
                }
            }
        };

        var products = new List<ProductResponseDto>
        {
            new ProductResponseDto
            {
                Id = productId1,
                Name = "Product 1",
                Price = 25.00m
            },
            new ProductResponseDto
            {
                Id = productId2,
                Name = "Product 2",
                Price = 100.00m
            }
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _productServiceMock.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _mapperMock.Setup(x => x.Map<OrderDto>(existingOrder))
            .Returns(expectedOrderDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderId);
        result.Status.Should().Be("Processing");
        result.TotalAmount.Should().Be(150.00m);
        result.Items.Should().HaveCount(2);
        result.Items.Sum(x => x.Subtotal).Should().Be(150.00m);

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<OrderDto>(existingOrder), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyGuid_ShouldReturnNull()
    {
        // Arrange
        var query = new GetOrderQuery(Guid.Empty);

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}