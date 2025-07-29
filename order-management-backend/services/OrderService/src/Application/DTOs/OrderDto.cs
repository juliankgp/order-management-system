using OrderService.Domain.Entities;

namespace OrderService.Application.DTOs;

/// <summary>
/// DTO para transferir datos de órdenes
/// </summary>
public record OrderDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime OrderDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal ShippingCost { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public ICollection<OrderItemDto> Items { get; init; } = new List<OrderItemDto>();
}

/// <summary>
/// DTO para items de la orden
/// </summary>
public record OrderItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}

/// <summary>
/// DTO para crear una nueva orden
/// </summary>
public record CreateOrderDto
{
    public Guid CustomerId { get; init; }
    public string? Notes { get; init; }
    public ICollection<CreateOrderItemDto> Items { get; init; } = new List<CreateOrderItemDto>();
}

/// <summary>
/// DTO para crear items de orden
/// </summary>
public record CreateOrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

/// <summary>
/// DTO para actualizar una orden
/// </summary>
public record UpdateOrderDto
{
    public OrderStatus? Status { get; init; }
    public string? Notes { get; init; }
    public ICollection<UpdateOrderItemDto>? Items { get; init; }
}

/// <summary>
/// DTO para actualizar items de orden
/// </summary>
public record UpdateOrderItemDto
{
    public Guid? Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

/// <summary>
/// DTO de respuesta con información de validación
/// </summary>
public record ValidationResponseDto
{
    public bool IsValid { get; init; }
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// DTO para respuesta de productos
/// </summary>
public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// DTO para respuesta de clientes
/// </summary>
public record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

/// <summary>
/// DTO para respuesta de clientes desde el servicio externo
/// </summary>
public record CustomerResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

/// <summary>
/// DTO para respuesta de productos desde el servicio externo
/// </summary>
public record ProductResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public bool IsActive { get; init; }
}
