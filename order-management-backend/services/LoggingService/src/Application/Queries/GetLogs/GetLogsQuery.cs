using LoggingService.Application.DTOs;
using MediatR;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Application.Queries.GetLogs;

public class GetLogsQuery : IRequest<PagedResult<LogEntryDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}