using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderService.Application.DTOs;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderManagement.Shared.Common.Exceptions;
using OrderManagement.Shared.Events.Orders;

namespace OrderService.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IEventBusService _eventBusService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ICustomerService customerService,
        IProductService productService,
        IEventBusService eventBusService,
        IMapper mapper,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerService = customerService;
        _productService = productService;
        _eventBusService = eventBusService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing create order command for customer {CustomerId}", request.OrderData.CustomerId);

        // Validar que el cliente existe
        var customer = await _customerService.GetCustomerAsync(request.OrderData.CustomerId);
        if (customer == null)
        {
            _logger.LogWarning("Customer not found: {CustomerId}", request.OrderData.CustomerId);
            throw new EntityNotFoundException("Customer", request.OrderData.CustomerId);
        }

        // Validar productos y stock
        var productIds = request.OrderData.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = new List<ProductResponseDto>();
        
        foreach (var productId in productIds)
        {
            var product = await _productService.GetProductAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found: {ProductId}", productId);
                throw new EntityNotFoundException("Product", productId);
            }
            products.Add(product);
        }

        // Validar stock suficiente
        foreach (var item in request.OrderData.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            if (product.Stock < item.Quantity)
            {
                _logger.LogWarning("Insufficient stock for product {ProductId}. Available: {Available}, Requested: {Requested}", 
                    item.ProductId, product.Stock, item.Quantity);
                throw new BusinessRuleException($"Insufficient stock for product {item.ProductId}. Available: {product.Stock}, Requested: {item.Quantity}");
            }
        }

        // Crear la orden
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.OrderData.CustomerId,
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            Status = OrderStatus.Pending,
            Notes = request.OrderData.Notes,
            ShippingAddress = "Default Address", // TODO: Add to DTO
            ShippingCity = "Default City", // TODO: Add to DTO
            ShippingZipCode = "00000", // TODO: Add to DTO
            ShippingCountry = "Default Country", // TODO: Add to DTO
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Crear items de la orden
        var orderItems = new List<OrderItem>();
        foreach (var itemDto in request.OrderData.Items)
        {
            var product = products.First(p => p.Id == itemDto.ProductId);
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = itemDto.ProductId,
                ProductName = product.Name,
                ProductSku = product.Name, // Using name as SKU for now
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                TotalPrice = itemDto.Quantity * product.Price,
                Discount = 0,
                Notes = null
            };
            orderItems.Add(orderItem);
        }

        order.Items = orderItems;

        // Calcular totales
        order.SubTotal = orderItems.Sum(item => item.Quantity * item.UnitPrice);
        order.TaxAmount = CalculateTax(order.SubTotal);
        order.ShippingCost = CalculateShipping(order.SubTotal);
        order.TotalAmount = order.SubTotal + order.TaxAmount + order.ShippingCost;

        // Guardar en base de datos
        await _orderRepository.AddAsync(order);

        _logger.LogInformation("Order {OrderId} created successfully with total amount {TotalAmount}", 
            order.Id, order.TotalAmount);

        // Publicar evento
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item => new OrderItemCreated
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _eventBusService.PublishAsync(orderCreatedEvent);

        return _mapper.Map<OrderDto>(order);
    }

    private decimal CalculateTax(decimal subTotal)
    {
        return subTotal * 0.10m; // 10% de impuestos
    }

    private decimal CalculateShipping(decimal subTotal)
    {
        if (subTotal > 100) return 0; // Envío gratis para órdenes mayores a $100
        return 10; // $10 de costo de envío
    }
}
