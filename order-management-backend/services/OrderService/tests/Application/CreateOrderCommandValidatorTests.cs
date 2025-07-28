using FluentAssertions;
using FluentValidation.TestHelper;
using OrderService.Application.Commands.CreateOrder;
using Xunit;

namespace OrderService.Tests.Application;

/// <summary>
/// Tests para la validación del comando CreateOrder
/// </summary>
public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.Empty,
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Have_Error_When_Items_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>(),
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_Have_Error_When_ShippingAddress_Is_Empty()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = "",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress);
    }

    [Fact]
    public void Should_Have_Error_When_ShippingAddress_Exceeds_MaxLength()
    {
        // Arrange
        var longAddress = new string('A', 201); // Más de 200 caracteres
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = longAddress,
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShippingAddress);
    }

    [Fact]
    public void Should_Have_Error_When_Item_Quantity_Is_Zero_Or_Negative()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 0 }
            },
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 },
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "USA",
            Notes = "Handle with care"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Notes_Exceeds_MaxLength()
    {
        // Arrange
        var longNotes = new string('A', 501); // Más de 500 caracteres
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = "123 Main St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country",
            Notes = longNotes
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}
