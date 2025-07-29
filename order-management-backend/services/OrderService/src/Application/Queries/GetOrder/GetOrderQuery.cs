using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries.GetOrder;

/// <summary>
/// Query para obtener una orden por ID
/// </summary>
public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto?>;
