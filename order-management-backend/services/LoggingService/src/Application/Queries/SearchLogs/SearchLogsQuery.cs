using LoggingService.Application.DTOs;
using LoggingService.Domain.Entities;
using MediatR;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Application.Queries.SearchLogs;

public class SearchLogsQuery : IRequest<PagedResult<LogEntryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? ServiceName { get; set; }
    public LogLevel? Level { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Category { get; set; }
    public string? CorrelationId { get; set; }
    public Guid? UserId { get; set; }
}