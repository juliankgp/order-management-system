namespace LoggingService.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    ILogEntryRepository LogEntries { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}