namespace ProductService.Domain.Repositories;

/// <summary>
/// Interface para Unit of Work pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    IStockMovementRepository StockMovements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}