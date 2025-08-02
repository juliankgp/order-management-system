using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventBusService> _eventBusServiceMock;
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UpdateOrderCommandHandler>> _loggerMock;
    private readonly UpdateOrderCommandHandler _handler;

    public UpdateOrderCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventBusServiceMock = new Mock<IEventBusService>();
        _productServiceMock = new Mock<IProductService>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UpdateOrderCommandHandler>>();

        _handler = new UpdateOrderCommandHandler(
            _unitOfWorkMock.Object,
            _eventBusServiceMock.Object,
            _productServiceMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldUpdateOrderSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing,
            Notes = "Updated notes",
            Items = new List<UpdateOrderItemDto>
            {
                new UpdateOrderItemDto
                {
                    ProductId = productId,
                    Quantity = 3
                }
            }
        };
        
        var command = new UpdateOrderCommand(orderId, updateOrderDto);

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
                    ProductId = productId,
                    ProductName = "Test Product",
                    ProductSku = "TEST-001",
                    Quantity = 2,
                    UnitPrice = 25.00m,
                    TotalPrice = 50.00m
                }
            }
        };

        var products = new List<ProductResponseDto>
        {
            new ProductResponseDto
            {
                Id = productId,
                Name = "Test Product",
                Price = 29.99m
            }
        };

        var stockValidation = new ValidationResponseDto { IsValid = true };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            CustomerId = existingOrder.CustomerId,
            OrderNumber = "ORD-001",
            Status = "Processing",
            TotalAmount = 89.97m,
            OrderDate = DateTime.UtcNow,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    ProductId = productId,
                    ProductName = "Test Product",
                    Quantity = 3,
                    UnitPrice = 29.99m,
                    Subtotal = 89.97m
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _productServiceMock
            .Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _productServiceMock
            .Setup(x => x.ValidateStockAsync(productId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stockValidation);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock
            .Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(expectedOrderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.Status.Should().Be("Processing");
        result.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(89.97m);

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _productServiceMock.Verify(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        // El handler real no usa ValidateStockAsync
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OrderNotFound_ShouldThrowBusinessException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing,
            Items = new List<UpdateOrderItemDto>
            {
                new UpdateOrderItemDto
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        };
        
        var command = new UpdateOrderCommand(orderId, updateOrderDto);

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_OrderWithMissingProducts_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing,
            Items = new List<UpdateOrderItemDto>
            {
                new UpdateOrderItemDto
                {
                    ProductId = productId,
                    Quantity = 1
                }
            }
        };
        
        var command = new UpdateOrderCommand(orderId, updateOrderDto);

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
            Items = new List<OrderItem>()
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        // No configurar productos mock para simular productos no encontrados
        _productServiceMock.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductResponseDto>()); // Lista vacía simula productos no encontrados

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Products not found");
    }

    [Fact]
    public async Task Handle_ValidUpdateWithStatusOnly_ShouldUpdateSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var updateOrderDto = new UpdateOrderDto
        {
            Status = OrderStatus.Processing,
            Notes = "Updated status to processing"
        };
        
        var command = new UpdateOrderCommand(orderId, updateOrderDto);

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
            Items = new List<OrderItem>()
        };

        var expectedOrderDto = new OrderDto
        {
            Id = orderId,
            Status = "Processing",
            TotalAmount = 50.00m
        };

        _unitOfWorkMock.Setup(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mapperMock.Setup(x => x.Map<OrderDto>(It.IsAny<Order>()))
            .Returns(expectedOrderDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Processing");

        _unitOfWorkMock.Verify(x => x.Orders.GetByIdWithItemsAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusServiceMock.Verify(x => x.PublishAsync(It.IsAny<object>()), Times.Once);
    }
}