using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Commands.CreateOrder;

/// <summary>
/// Comando para crear una nueva orden
/// </summary>
public record CreateOrderCommand(CreateOrderDto OrderData) : IRequest<OrderDto>;
