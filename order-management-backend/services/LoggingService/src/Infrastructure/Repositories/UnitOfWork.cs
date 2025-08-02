using LoggingService.Domain.Repositories;
using LoggingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace LoggingService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LoggingDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(LoggingDbContext context)
    {
        _context = context;
        LogEntries = new LogEntryRepository(_context);
    }

    public ILogEntryRepository LogEntries { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
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