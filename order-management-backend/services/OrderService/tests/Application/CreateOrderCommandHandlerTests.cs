using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Shared.Events.Orders;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.DTOs;
using OrderService.Application.Mappings;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using Xunit;

namespace OrderService.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IOrderRepository> _mockOrderRepository;
    private readonly Mock<IEventBusService> _mockEventBus;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<ILogger<CreateOrderCommandHandler>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockEventBus = new Mock<IEventBusService>();
        _mockProductService = new Mock<IProductService>();
        _mockCustomerService = new Mock<ICustomerService>();
        _mockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();

        // Setup UnitOfWork to return the mocked repository
        _mockUnitOfWork.Setup(x => x.Orders).Returns(_mockOrderRepository.Object);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new CreateOrderCommandHandler(
            _mockUnitOfWork.Object,
            _mockEventBus.Object,
            _mockProductService.Object,
            _mockCustomerService.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldCreateOrderSuccessfully()
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
                new CreateOrderItemDto { ProductId = productId, Quantity = 2 }
            }
        };

        var command = new CreateOrderCommand(createOrderDto);

        // Mock customer validation
        _mockCustomerService.Setup(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Customer exists" });

        // Mock product service
        var productDto = new ProductDto { Id = productId, Name = "Test Product", Price = 50.00m, Stock = 10 };
        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { productDto });

        _mockProductService.Setup(x => x.ValidateStockAsync(productId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Stock available" });

        // Mock repository
        _mockOrderRepository.Setup(x => x.CountTodayOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mockOrderRepository.Setup(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(customerId);
        result.Items.Should().HaveCount(1);
        result.Items.First().ProductId.Should().Be(productId);
        result.Items.First().Quantity.Should().Be(2);
        result.TotalAmount.Should().Be(110.00m); // 100 + 10 tax + 0 shipping (over $100)

        // Verify calls
        _mockCustomerService.Verify(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockProductService.Verify(x => x.ValidateStockAsync(productId, 2, It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCustomer_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand(createOrderDto);

        _mockCustomerService.Setup(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = false, Message = "Customer not found" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Customer validation failed");
    }

    [Fact]
    public async Task Handle_ProductNotFound_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = productId, Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand(createOrderDto);

        _mockCustomerService.Setup(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Customer exists" });

        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto>()); // Empty list - product not found

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Products not found");
    }

    [Fact]
    public async Task Handle_InsufficientStock_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = productId, Quantity = 100 }
            }
        };

        var command = new CreateOrderCommand(createOrderDto);

        _mockCustomerService.Setup(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Customer exists" });

        var productDto = new ProductDto { Id = productId, Name = "Test Product", Price = 50.00m, Stock = 5 };
        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { productDto });

        _mockProductService.Setup(x => x.ValidateStockAsync(productId, 100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = false, Message = "Insufficient stock" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Stock validation failed");
    }

    [Theory]
    [InlineData(50.00, 2, 55.00)]   // 100 + 10% tax + 0 shipping (over $100)
    [InlineData(10.00, 1, 21.00)]   // 10 + 1 tax + 10 shipping (under $100)
    [InlineData(25.00, 3, 92.50)]   // 75 + 7.5 tax + 10 shipping (under $100)
    public async Task Handle_ValidOrder_ShouldCalculateTotalsCorrectly(decimal unitPrice, int quantity, decimal expectedTotal)
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var createOrderDto = new CreateOrderDto
        {
            CustomerId = customerId,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = productId, Quantity = quantity }
            }
        };

        var command = new CreateOrderCommand(createOrderDto);

        _mockCustomerService.Setup(x => x.ValidateCustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Customer exists" });

        var productDto = new ProductDto { Id = productId, Name = "Test Product", Price = unitPrice, Stock = 10 };
        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductDto> { productDto });

        _mockProductService.Setup(x => x.ValidateStockAsync(productId, quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResponseDto { IsValid = true, Message = "Stock available" });

        _mockOrderRepository.Setup(x => x.CountTodayOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.TotalAmount.Should().Be(expectedTotal);
    }
}
