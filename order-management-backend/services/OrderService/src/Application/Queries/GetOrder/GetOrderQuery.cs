using MediatR;
using OrderManagement.Shared.Common.Models;

namespace OrderService.Application.Queries.GetOrder;

/// <summary>
/// Query para obtener una orden por ID
/// </summary>
public record GetOrderQuery : IRequest<ApiResponse<OrderDto>>
{
    /// <summary>
    /// ID de la orden a consultar
    /// </summary>
    public required Guid OrderId { get; init; }
}

/// <summary>
/// DTO para la respuesta de orden
/// </summary>
public record OrderDto
{
    public required Guid Id { get; init; }
    public required string OrderNumber { get; init; }
    public required Guid CustomerId { get; init; }
    public required string Status { get; init; }
    public required DateTime OrderDate { get; init; }
    public required decimal TotalAmount { get; init; }
    public required decimal SubTotal { get; init; }
    public required decimal TaxAmount { get; init; }
    public required decimal ShippingCost { get; init; }
    public required string ShippingAddress { get; init; }
    public required string ShippingCity { get; init; }
    public required string ShippingZipCode { get; init; }
    public required string ShippingCountry { get; init; }
    public string? Notes { get; init; }
    public required List<OrderItemDto> Items { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO para item de orden
/// </summary>
public record OrderItemDto
{
    public required Guid Id { get; init; }
    public required Guid ProductId { get; init; }
    public required string ProductName { get; init; }
    public required string ProductSku { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal TotalPrice { get; init; }
    public required decimal Discount { get; init; }
    public string? Notes { get; init; }
}
