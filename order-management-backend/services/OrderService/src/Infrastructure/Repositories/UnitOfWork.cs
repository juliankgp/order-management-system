using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

/// <summary>
/// Implementación del patrón Unit of Work
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _context;
    private IDbContextTransaction? _transaction;
    private IOrderRepository? _orders;

    public UnitOfWork(OrderDbContext context)
    {
        _context = context;
    }

    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
