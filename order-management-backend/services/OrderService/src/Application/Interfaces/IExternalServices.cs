using OrderService.Application.DTOs;

namespace OrderService.Application.Interfaces;

/// <summary>
/// Interface for communicating with CustomerService
/// </summary>
public interface ICustomerService
{
    Task<CustomerResponseDto?> GetCustomerAsync(Guid customerId);
    Task<ValidationResponseDto> ValidateCustomerExistsAsync(Guid customerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for communicating with ProductService  
/// </summary>
public interface IProductService
{
    Task<ProductResponseDto?> GetProductAsync(Guid productId);
    Task<IEnumerable<ProductResponseDto>> GetProductsAsync(IEnumerable<Guid> productIds, CancellationToken cancellationToken = default);
    Task<ValidationResponseDto> ValidateProductExistsAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<ValidationResponseDto> ValidateStockAsync(Guid productId, int requiredQuantity, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for event bus service
/// </summary>
public interface IEventBusService
{
    Task PublishAsync<T>(T @event) where T : class;
}
