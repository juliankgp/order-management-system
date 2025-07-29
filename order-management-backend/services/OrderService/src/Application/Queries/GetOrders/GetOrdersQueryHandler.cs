using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Common.Models;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Repositories;

namespace OrderService.Application.Queries.GetOrders;

/// <summary>
/// Handler para la query de obtener órdenes con paginación
/// </summary>
public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductService _productService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetOrdersQueryHandler> _logger;

    public GetOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        IProductService productService,
        IMapper mapper,
        ILogger<GetOrdersQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _productService = productService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing get orders query with filters: CustomerId={CustomerId}, Status={Status}, Page={Page}, PageSize={PageSize}", 
            request.CustomerId, request.Status, request.Page, request.PageSize);

        try
        {
            // 1. Obtener órdenes con paginación y filtros
            var pagedOrders = await _unitOfWork.Orders.GetPagedAsync(
                page: request.Page,
                pageSize: request.PageSize,
                customerId: request.CustomerId,
                status: request.Status,
                fromDate: request.FromDate,
                toDate: request.ToDate,
                orderNumber: request.OrderNumber,
                cancellationToken: cancellationToken);

            // 2. Mapear a DTOs
            var orderDtos = _mapper.Map<List<OrderDto>>(pagedOrders.Items);

            // 3. Enriquecer con información de productos para cada orden
            foreach (var orderDto in orderDtos)
            {
                if (orderDto.Items.Any())
                {
                    var productIds = orderDto.Items.Select(i => i.ProductId).ToList();
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

                    // Actualizar el item en la lista principal
                    var index = orderDtos.IndexOf(orderDto);
                    orderDtos[index] = orderDto with { Items = enrichedItems };
                }
            }

            var result = new PagedResult<OrderDto>
            {
                Items = orderDtos,
                TotalCount = pagedOrders.TotalCount,
                CurrentPage = pagedOrders.CurrentPage,
                PageSize = pagedOrders.PageSize
            };

            _logger.LogInformation("Retrieved {Count} orders out of {Total} total orders", 
                orderDtos.Count, pagedOrders.TotalCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders with filters");
            throw;
        }
    }
}
