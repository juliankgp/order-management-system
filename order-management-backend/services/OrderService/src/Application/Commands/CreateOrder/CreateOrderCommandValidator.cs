using FluentValidation;

namespace OrderService.Application.Commands.CreateOrder;

/// <summary>
/// Validador para el comando de crear orden
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderData)
            .NotNull()
            .WithMessage("Order data is required");

        RuleFor(x => x.OrderData.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required")
            .When(x => x.OrderData != null);

        RuleFor(x => x.OrderData.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item")
            .Must(items => items.Count <= 50)
            .WithMessage("Order cannot have more than 50 items")
            .When(x => x.OrderData != null);

        RuleForEach(x => x.OrderData.Items)
            .SetValidator(new CreateOrderItemValidator())
            .When(x => x.OrderData != null);

        RuleFor(x => x.OrderData.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters")
            .When(x => x.OrderData != null);
    }
}

/// <summary>
/// Validador para items de orden
/// </summary>
public class CreateOrderItemValidator : AbstractValidator<OrderService.Application.DTOs.CreateOrderItemDto>
{
    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Quantity cannot exceed 1000");
    }
}
