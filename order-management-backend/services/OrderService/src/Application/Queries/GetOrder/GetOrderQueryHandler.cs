using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Queries.GetOrder;

/// <summary>
/// Handler para la query de obtener orden por ID
/// </summary>
public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(
        IUnitOfWork unitOfWork,
        IProductService productService,
        IMapper mapper,
        ILogger<GetOrderQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing get order query for order {OrderId}", request.OrderId);

        try
        {
            // 1. Obtener la orden con sus items
            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                return null;
            }

            // 2. Mapear a DTO
            var orderDto = _mapper.Map<OrderDto>(order);

            // 3. Enriquecer con información de productos
            if (order.Items.Any())
            {
                var productIds = order.Items.Select(i => i.ProductId).ToList();
                var products = await _productService.GetProductsAsync(productIds, cancellationToken);
                var productDict = products.ToDictionary(p => p.Id);

                // Crear una nueva lista con los items enriquecidos
                var enrichedItems = new List<OrderItemDto>();
                foreach (var itemDto in orderDto.Items)
                {
                    var enrichedItem = itemDto;
                    if (productDict.TryGetValue(itemDto.ProductId, out var product))
                    {
                        enrichedItem = itemDto with { ProductName = product.Name };
                    }
                    enrichedItems.Add(enrichedItem);
                }

                // Actualizar el DTO con los items enriquecidos
                orderDto = orderDto with { Items = enrichedItems };
            }

            _logger.LogInformation("Order {OrderId} retrieved successfully", request.OrderId);
            return orderDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", request.OrderId);
            throw;
        }
    }
}
