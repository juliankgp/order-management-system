using OrderManagement.Shared.Common.Models;
using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

/// <summary>
/// Interface del repositorio de productos
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, string? category = null, string? searchTerm = null, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    Task UpdateAsync(Product product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasSufficientStockAsync(Guid productId, int requiredQuantity, CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid productId, int newStock, CancellationToken cancellationToken = default);
}