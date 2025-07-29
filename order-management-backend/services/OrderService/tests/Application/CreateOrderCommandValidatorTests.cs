using FluentAssertions;
using OrderService.Application.Commands.CreateOrder;
using OrderService.Application.DTOs;
using Xunit;

namespace OrderService.Tests.Application;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validate_ValidCommand_ShouldBeValid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Notes = "Test order",
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_NullOrderData_ShouldBeInvalid()
    {
        // Arrange
        var command = new CreateOrderCommand(null!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Order data is required");
    }

    [Fact]
    public void Validate_EmptyCustomerId_ShouldBeInvalid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.Empty,
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Customer ID is required");
    }

    [Fact]
    public void Validate_EmptyItems_ShouldBeInvalid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>()
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Order must have at least one item");
    }

    [Fact]
    public void Validate_TooManyItems_ShouldBeInvalid()
    {
        // Arrange
        var items = new List<CreateOrderItemDto>();
        for (int i = 0; i < 51; i++)
        {
            items.Add(new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 });
        }

        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = items
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Order cannot have more than 50 items");
    }

    [Fact]
    public void Validate_NotesTooLong_ShouldBeInvalid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Notes = new string('a', 501), // 501 characters
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Notes must not exceed 500 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_InvalidQuantity_ShouldBeInvalid(int quantity)
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = quantity }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity must be greater than 0");
    }

    [Fact]
    public void Validate_QuantityTooHigh_ShouldBeInvalid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.NewGuid(), Quantity = 1001 }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity cannot exceed 1000");
    }

    [Fact]
    public void Validate_EmptyProductId_ShouldBeInvalid()
    {
        // Arrange
        var orderData = new CreateOrderDto
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto { ProductId = Guid.Empty, Quantity = 1 }
            }
        };

        var command = new CreateOrderCommand(orderData);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Product ID is required");
    }
}
