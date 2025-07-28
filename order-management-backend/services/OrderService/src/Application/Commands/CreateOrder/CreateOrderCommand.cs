using MediatR;
using OrderManagement.Shared.Common.Models;

namespace OrderService.Application.Commands.CreateOrder;

/// <summary>
/// Comando para crear una nueva orden
/// </summary>
public record CreateOrderCommand : IRequest<ApiResponse<CreateOrderResponse>>
{
    /// <summary>
    /// ID del cliente
    /// </summary>
    public required Guid CustomerId { get; init; }
    
    /// <summary>
    /// Items de la orden
    /// </summary>
    public required List<CreateOrderItemRequest> Items { get; init; }
    
    /// <summary>
    /// Dirección de envío
    /// </summary>
    public required string ShippingAddress { get; init; }
    
    /// <summary>
    /// Ciudad de envío
    /// </summary>
    public required string ShippingCity { get; init; }
    
    /// <summary>
    /// Código postal
    /// </summary>
    public required string ShippingZipCode { get; init; }
    
    /// <summary>
    /// País de envío
    /// </summary>
    public required string ShippingCountry { get; init; }
    
    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request para item de orden
/// </summary>
public record CreateOrderItemRequest
{
    /// <summary>
    /// ID del producto
    /// </summary>
    public required Guid ProductId { get; init; }
    
    /// <summary>
    /// Cantidad
    /// </summary>
    public required int Quantity { get; init; }
}

/// <summary>
/// Respuesta de creación de orden
/// </summary>
public record CreateOrderResponse
{
    /// <summary>
    /// ID de la orden creada
    /// </summary>
    public required Guid OrderId { get; init; }
    
    /// <summary>
    /// Número de orden
    /// </summary>
    public required string OrderNumber { get; init; }
    
    /// <summary>
    /// Total de la orden
    /// </summary>
    public required decimal TotalAmount { get; init; }
    
    /// <summary>
    /// Estado de la orden
    /// </summary>
    public required string Status { get; init; }
}
