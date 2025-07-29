using MediatR;
using OrderManagement.Shared.Common.Models;
using OrderService.Application.DTOs;

namespace OrderService.Application.Queries.GetOrders;

/// <summary>
/// Query para obtener órdenes con paginación y filtros
/// </summary>
public record GetOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CustomerId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? OrderNumber { get; init; }
}
