using LoggingService.Domain.Entities;
using LoggingService.Domain.Repositories;
using LoggingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Infrastructure.Repositories;

public class LogEntryRepository : ILogEntryRepository
{
    private readonly LoggingDbContext _context;

    public LogEntryRepository(LoggingDbContext context)
    {
        _context = context;
    }

    public async Task<LogEntry?> GetByIdAsync(Guid id)
    {
        return await _context.LogEntries.FindAsync(id);
    }

    public async Task<PagedResult<LogEntry>> GetAllAsync(int page, int pageSize)
    {
        var query = _context.LogEntries.OrderByDescending(x => x.Timestamp);
        
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LogEntry>> SearchAsync(string? serviceName, LogLevel? level, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.LogEntries.AsQueryable();

        if (!string.IsNullOrEmpty(serviceName))
        {
            query = query.Where(x => x.ServiceName.Contains(serviceName));
        }

        if (level.HasValue)
        {
            query = query.Where(x => x.Level == level.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.Timestamp <= toDate.Value);
        }

        query = query.OrderByDescending(x => x.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LogEntry>> GetByServiceAsync(string serviceName, int page, int pageSize)
    {
        var query = _context.LogEntries
            .Where(x => x.ServiceName == serviceName)
            .OrderByDescending(x => x.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LogEntry>> GetByUserAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.LogEntries
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<LogEntry>> GetByCorrelationIdAsync(string correlationId, int page, int pageSize)
    {
        var query = _context.LogEntries
            .Where(x => x.CorrelationId == correlationId)
            .OrderByDescending(x => x.Timestamp);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<LogEntry>
        {
            Items = items,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize
        };
    }

    public async Task<LogEntry> AddAsync(LogEntry logEntry)
    {
        _context.LogEntries.Add(logEntry);
        await _context.SaveChangesAsync();
        return logEntry;
    }

    public async Task<IEnumerable<LogEntry>> AddRangeAsync(IEnumerable<LogEntry> logEntries)
    {
        _context.LogEntries.AddRange(logEntries);
        await _context.SaveChangesAsync();
        return logEntries;
    }

    public async Task DeleteOldLogsAsync(DateTime cutoffDate)
    {
        var oldLogs = _context.LogEntries.Where(x => x.CreatedAt < cutoffDate);
        _context.LogEntries.RemoveRange(oldLogs);
        await _context.SaveChangesAsync();
    }
}