using MediatR;
using OrderManagement.Shared.Common.Exceptions;
using OrderManagement.Shared.Common.Models;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Queries.GetOrder;

/// <summary>
/// Handler para obtener una orden por ID
/// </summary>
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, ApiResponse<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrderQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<OrderDto>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        
        if (order == null)
        {
            throw new EntityNotFoundException(nameof(Domain.Entities.Order), request.OrderId);
        }

        var orderDto = new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount,
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            ShippingCost = order.ShippingCost,
            ShippingAddress = order.ShippingAddress,
            ShippingCity = order.ShippingCity,
            ShippingZipCode = order.ShippingZipCode,
            ShippingCountry = order.ShippingCountry,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                ProductSku = item.ProductSku,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
                Discount = item.Discount,
                Notes = item.Notes
            }).ToList()
        };

        return ApiResponse<OrderDto>.SuccessResponse(orderDto);
    }
}
