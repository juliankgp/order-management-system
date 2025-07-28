using FluentValidation;
using OrderService.Application.Commands.CreateOrder;

namespace OrderService.Application.Commands.CreateOrder;

/// <summary>
/// Validador para el comando de crear orden
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new CreateOrderItemValidator());

        RuleFor(x => x.ShippingAddress)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("Shipping address is required and must not exceed 200 characters");

        RuleFor(x => x.ShippingCity)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Shipping city is required and must not exceed 100 characters");

        RuleFor(x => x.ShippingZipCode)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Shipping zip code is required and must not exceed 20 characters");

        RuleFor(x => x.ShippingCountry)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Shipping country is required and must not exceed 50 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");
    }
}

/// <summary>
/// Validador para items de orden
/// </summary>
public class CreateOrderItemValidator : AbstractValidator<CreateOrderItemRequest>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
    }
}
