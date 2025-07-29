using FluentValidation;

namespace OrderService.Application.Commands.UpdateOrder;

/// <summary>
/// Validador para el comando de actualizar orden
/// </summary>
public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.OrderData)
            .NotNull()
            .WithMessage("Order data is required");

        RuleFor(x => x.OrderData.Notes)
            .MaximumLength(500)
            .WithMessage("Notes must not exceed 500 characters");

        When(x => x.OrderData.Items != null && x.OrderData.Items.Any(), () =>
        {
            RuleFor(x => x.OrderData.Items)
                .Must(items => items!.Count <= 50)
                .WithMessage("Order cannot have more than 50 items");

            RuleForEach(x => x.OrderData.Items)
                .SetValidator(new UpdateOrderItemValidator());
        });
    }
}

/// <summary>
/// Validador para items de orden al actualizar
/// </summary>
public class UpdateOrderItemValidator : AbstractValidator<OrderService.Application.DTOs.UpdateOrderItemDto>
{
    public UpdateOrderItemValidator()
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
