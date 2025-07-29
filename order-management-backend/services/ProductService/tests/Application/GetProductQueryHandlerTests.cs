using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Mappings;
using ProductService.Application.Queries.GetProduct;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using Xunit;

namespace ProductService.Tests.Application;

public class GetProductQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IProductRepository> _mockProductRepository;
    private readonly Mock<ILogger<GetProductQueryHandler>> _mockLogger;
    private readonly IMapper _mapper;
    private readonly GetProductQueryHandler _handler;

    public GetProductQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<GetProductQueryHandler>>();

        // Setup UnitOfWork to return the mocked repository
        _mockUnitOfWork.Setup(x => x.Products).Returns(_mockProductRepository.Object);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetProductQueryHandler(
            _mockUnitOfWork.Object,
            _mapper,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST-001",
            Price = 99.99m,
            Stock = 10,
            MinimumStock = 2,
            Category = "Electronics",
            Brand = "TestBrand",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Test Product");
        result.Sku.Should().Be("TEST-001");
        result.Price.Should().Be(99.99m);
        result.Stock.Should().Be(10);
        result.Category.Should().Be("Electronics");
        
        _mockProductRepository.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var query = new GetProductQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        
        _mockProductRepository.Verify(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidProduct_ShouldMapAllProperties()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;
        
        var product = new Product
        {
            Id = productId,
            Name = "Advanced Laptop",
            Description = "High-performance laptop for professionals",
            Sku = "LAPTOP-PRO-001",
            Price = 1299.99m,
            Stock = 25,
            MinimumStock = 5,
            Category = "Electronics",
            Brand = "TechCorp",
            Weight = 1500,
            Dimensions = "35x25x2 cm",
            IsActive = true,
            Tags = "laptop, computer, professional",
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _mockProductRepository.Setup(x => x.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var query = new GetProductQuery(productId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.Name.Should().Be("Advanced Laptop");
        result.Description.Should().Be("High-performance laptop for professionals");
        result.Sku.Should().Be("LAPTOP-PRO-001");
        result.Price.Should().Be(1299.99m);
        result.Stock.Should().Be(25);
        result.MinimumStock.Should().Be(5);
        result.Category.Should().Be("Electronics");
        result.Brand.Should().Be("TechCorp");
        result.Weight.Should().Be(1500);
        result.Dimensions.Should().Be("35x25x2 cm");
        result.IsActive.Should().BeTrue();
        result.Tags.Should().Be("laptop, computer, professional");
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }
}