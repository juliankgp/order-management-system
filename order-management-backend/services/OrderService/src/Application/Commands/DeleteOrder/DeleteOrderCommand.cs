using MediatR;

namespace OrderService.Application.Commands.DeleteOrder;

/// <summary>
/// Comando para eliminar una orden (soft delete)
/// </summary>
public record DeleteOrderCommand(Guid OrderId) : IRequest<bool>;
