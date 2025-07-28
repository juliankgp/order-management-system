using MediatR;
using OrderManagement.Shared.Common.Exceptions;
using OrderManagement.Shared.Common.Models;
using OrderManagement.Shared.Events.Abstractions;
using OrderManagement.Shared.Events.Orders;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Commands.CreateOrder;

/// <summary>
/// Handler para el comando de crear orden
/// </summary>
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<CreateOrderResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IProductService _productService;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEventBus eventBus,
        IProductService productService)
    {
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _productService = productService;
    }

    public async Task<ApiResponse<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Generar número de orden único
            var orderNumber = await GenerateOrderNumberAsync();

            // Validar productos y obtener información
            var productDetails = await _productService.GetProductsAsync(
                request.Items.Select(i => i.ProductId).ToList(), 
                cancellationToken);

            if (productDetails.Count != request.Items.Count)
            {
                throw new ValidationException("One or more products not found");
            }

            // Crear la orden
            var order = new Order
            {
                CustomerId = request.CustomerId,
                OrderNumber = orderNumber,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = request.ShippingAddress,
                ShippingCity = request.ShippingCity,
                ShippingZipCode = request.ShippingZipCode,
                ShippingCountry = request.ShippingCountry,
                Notes = request.Notes
            };

            // Agregar items y calcular totales
            decimal subTotal = 0;
            foreach (var itemRequest in request.Items)
            {
                var product = productDetails.First(p => p.Id == itemRequest.ProductId);
                
                // Validar stock disponible
                if (product.Stock < itemRequest.Quantity)
                {
                    throw new BusinessRuleException($"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {itemRequest.Quantity}");
                }

                var totalPrice = itemRequest.Quantity * product.Price;
                subTotal += totalPrice;

                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = itemRequest.ProductId,
                    ProductName = product.Name,
                    ProductSku = product.Sku,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = totalPrice,
                    Discount = 0
                };

                order.Items.Add(orderItem);
            }

            // Calcular totales
            order.SubTotal = subTotal;
            order.TaxAmount = CalculateTax(subTotal);
            order.ShippingCost = CalculateShipping(subTotal);
            order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCost;

            // Guardar en base de datos
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            var savedOrder = await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // Publicar evento
            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = savedOrder.Id,
                CustomerId = savedOrder.CustomerId,
                TotalAmount = savedOrder.TotalAmount,
                Items = savedOrder.Items.Select(i => new OrderItemCreated
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            await _eventBus.PublishAsync(orderCreatedEvent, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var response = new CreateOrderResponse
            {
                OrderId = savedOrder.Id,
                OrderNumber = savedOrder.OrderNumber,
                TotalAmount = savedOrder.TotalAmount,
                Status = savedOrder.Status.ToString()
            };

            return ApiResponse<CreateOrderResponse>.SuccessResponse(response, "Order created successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private decimal CalculateTax(decimal subTotal)
    {
        // Implementar lógica de cálculo de impuestos
        return subTotal * 0.15m; // 15% de impuesto
    }

    private decimal CalculateShipping(decimal subTotal)
    {
        // Implementar lógica de cálculo de envío
        if (subTotal > 100) return 0; // Envío gratis para órdenes mayores a $100
        return 10; // $10 de costo de envío
    }
}

/// <summary>
/// Interface para comunicación con ProductService
/// </summary>
public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(List<Guid> productIds, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO para producto
/// </summary>
public record ProductDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Sku { get; init; }
    public required decimal Price { get; init; }
    public required int Stock { get; init; }
}
