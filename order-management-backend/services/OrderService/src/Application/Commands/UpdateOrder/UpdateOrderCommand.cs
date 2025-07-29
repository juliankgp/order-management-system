using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands.UpdateOrder;

/// <summary>
/// Comando para actualizar una orden existente
/// </summary>
public record UpdateOrderCommand(Guid OrderId, UpdateOrderDto OrderData) : IRequest<OrderDto>;
