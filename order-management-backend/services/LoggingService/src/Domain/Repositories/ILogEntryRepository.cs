using LoggingService.Domain.Entities;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Domain.Repositories;

public interface ILogEntryRepository
{
    Task<LogEntry?> GetByIdAsync(Guid id);
    Task<PagedResult<LogEntry>> GetAllAsync(int page, int pageSize);
    Task<PagedResult<LogEntry>> SearchAsync(string? serviceName, LogLevel? level, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<PagedResult<LogEntry>> GetByServiceAsync(string serviceName, int page, int pageSize);
    Task<PagedResult<LogEntry>> GetByUserAsync(Guid userId, int page, int pageSize);
    Task<PagedResult<LogEntry>> GetByCorrelationIdAsync(string correlationId, int page, int pageSize);
    Task<LogEntry> AddAsync(LogEntry logEntry);
    Task<IEnumerable<LogEntry>> AddRangeAsync(IEnumerable<LogEntry> logEntries);
    Task DeleteOldLogsAsync(DateTime cutoffDate);
}