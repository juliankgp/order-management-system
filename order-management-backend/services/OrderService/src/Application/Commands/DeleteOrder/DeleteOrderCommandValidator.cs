using FluentValidation;

namespace OrderService.Application.Commands.DeleteOrder;

/// <summary>
/// Validador para el comando de eliminar orden
/// </summary>
public class DeleteOrderCommandValidator : AbstractValidator<DeleteOrderCommand>
{
    public DeleteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");
    }
}
