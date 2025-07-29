namespace ProductService.Application.DTOs;

/// <summary>
/// DTO para respuesta de producto
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public required string Category { get; set; }
    public string? Brand { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO para crear producto
/// </summary>
public class CreateProductDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int MinimumStock { get; set; }
    public required string Category { get; set; }
    public string? Brand { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO para actualizar producto
/// </summary>
public class UpdateProductDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public int? MinimumStock { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// DTO para validación de respuesta
/// </summary>
public class ValidationResponseDto
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// DTO para movimiento de stock
/// </summary>
public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public required string Reason { get; set; }
    public string? ExternalReference { get; set; }
    public Guid? UserId { get; set; }
    public string? Comments { get; set; }
    public DateTime CreatedAt { get; set; }
}