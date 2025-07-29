using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Events.Orders;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Commands.UpdateOrder;

/// <summary>
/// Handler para el comando de actualizar orden
/// </summary>
public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBusService _eventBus;
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateOrderCommandHandler> _logger;

    public UpdateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IEventBusService eventBus,
        IProductService productService,
        IMapper mapper,
        ILogger<UpdateOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing update order command for order {OrderId}", request.OrderId);

        try
        {
            // 1. Obtener la orden existente
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                throw new InvalidOperationException($"Order {request.OrderId} not found");
            }

            var previousStatus = order.Status;
            var previousTotalAmount = order.TotalAmount;

            // 2. Actualizar propiedades básicas
            if (request.OrderData.Status.HasValue)
            {
                order.Status = request.OrderData.Status.Value;
            }

            if (!string.IsNullOrEmpty(request.OrderData.Notes))
            {
                order.Notes = request.OrderData.Notes;
            }

            // 3. Actualizar items si se proporcionaron
            if (request.OrderData.Items != null && request.OrderData.Items.Any())
            {
                await UpdateOrderItemsAsync(order, request.OrderData.Items, cancellationToken);
                
                // Recalcular totales
                RecalculateOrderTotals(order);
            }

            order.UpdatedAt = DateTime.UtcNow;

            // 4. Guardar cambios
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Order {OrderId} updated successfully", order.Id);

            // 5. Publicar evento si el estado cambió
            if (request.OrderData.Status.HasValue && previousStatus != order.Status)
            {
                var statusUpdatedEvent = new OrderStatusUpdatedEvent
                {
                    OrderId = order.Id,
                    PreviousStatus = previousStatus.ToString(),
                    NewStatus = order.Status.ToString(),
                    UpdatedBy = null,
                    Reason = "Order status updated"
                };

                await _eventBus.PublishAsync(statusUpdatedEvent);
                _logger.LogInformation("OrderStatusUpdatedEvent published for order {OrderId}", order.Id);
            }

            // 6. Mapear a DTO y retornar
            var orderDto = _mapper.Map<OrderDto>(order);

            // Enriquecer con nombres de productos
            if (order.Items.Any())
            {
                var productIds = order.Items.Select(i => i.ProductId).ToList();
                var products = await _productService.GetProductsAsync(productIds, cancellationToken);
                var productDict = products.ToDictionary(p => p.Id);

                var updatedItems = new List<OrderItemDto>();
                foreach (var itemDto in orderDto.Items)
                {
                    if (productDict.TryGetValue(itemDto.ProductId, out var product))
                    {
                        updatedItems.Add(itemDto with { ProductName = product.Name });
                    }
                    else
                    {
                        updatedItems.Add(itemDto);
                    }
                }
                orderDto = orderDto with { Items = updatedItems };
            }

            return orderDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", request.OrderId);
            throw;
        }
    }

    private async Task UpdateOrderItemsAsync(Order order, ICollection<UpdateOrderItemDto> newItems, CancellationToken cancellationToken)
    {
        // Validar productos
        var productIds = newItems.Select(i => i.ProductId).ToList();
        var products = await _productService.GetProductsAsync(productIds, cancellationToken);
        var productDict = products.ToDictionary(p => p.Id);

        if (products.Count() != productIds.Distinct().Count())
        {
            var foundProductIds = products.Select(p => p.Id).ToHashSet();
            var missingProductIds = productIds.Where(id => !foundProductIds.Contains(id));
            throw new InvalidOperationException($"Products not found: {string.Join(", ", missingProductIds)}");
        }

        // Validar stock para nuevos items o cantidades aumentadas
        foreach (var newItem in newItems)
        {
            var existingItem = order.Items.FirstOrDefault(i => i.Id == newItem.Id);
            var quantityDifference = existingItem == null ? newItem.Quantity : newItem.Quantity - existingItem.Quantity;

            // Note: Stock validation could be implemented here if needed
            // For now, we assume the stock check is done elsewhere or not required for updates
        }

        // Actualizar items existentes y agregar nuevos
        var existingItemIds = order.Items.Select(i => i.Id).ToHashSet();
        var newItemIds = newItems.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Eliminar items que ya no están en la lista
        var itemsToRemove = order.Items.Where(i => !newItemIds.Contains(i.Id)).ToList();
        foreach (var item in itemsToRemove)
        {
            order.Items.Remove(item);
        }

        // Actualizar o agregar items
        foreach (var newItemDto in newItems)
        {
            var product = productDict[newItemDto.ProductId];

            if (newItemDto.Id.HasValue)
            {
                // Actualizar item existente
                var existingItem = order.Items.FirstOrDefault(i => i.Id == newItemDto.Id.Value);
                if (existingItem != null)
                {
                    existingItem.ProductId = newItemDto.ProductId;
                    existingItem.Quantity = newItemDto.Quantity;
                    existingItem.UnitPrice = product.Price;
                    existingItem.TotalPrice = product.Price * newItemDto.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                // Agregar nuevo item
                var newItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = newItemDto.ProductId,
                    ProductName = product.Name,
                    ProductSku = product.Name, // Using name as SKU for now
                    Quantity = newItemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * newItemDto.Quantity,
                    Discount = 0,
                    Notes = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                order.Items.Add(newItem);
            }
        }
    }

    private static void RecalculateOrderTotals(Order order)
    {
        order.SubTotal = order.Items.Sum(i => i.TotalPrice);
        order.TaxAmount = Math.Round(order.SubTotal * 0.10m, 2); // 10% tax
        order.ShippingCost = order.SubTotal >= 100m ? 0m : 10m; // Free shipping over $100
        order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCost;
    }
}
