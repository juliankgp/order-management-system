using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

/// <summary>
/// Interface del repositorio de movimientos de stock
/// </summary>
public interface IStockMovementRepository
{
    Task<StockMovement?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByProductIdPagedAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovement>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task AddAsync(StockMovement stockMovement, CancellationToken cancellationToken = default);
    Task<decimal> GetCurrentStockAsync(Guid productId, CancellationToken cancellationToken = default);
}