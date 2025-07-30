using AutoMapper;
using LoggingService.Application.DTOs;
using LoggingService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Application.Queries.SearchLogs;

public class SearchLogsQueryHandler : IRequestHandler<SearchLogsQuery, PagedResult<LogEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchLogsQueryHandler> _logger;

    public SearchLogsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SearchLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<LogEntryDto>> Handle(SearchLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await _unitOfWork.LogEntries.SearchAsync(
                request.ServiceName,
                request.Level,
                request.FromDate,
                request.ToDate,
                request.Page,
                request.PageSize);
            
            var logDtos = _mapper.Map<IEnumerable<LogEntryDto>>(logs.Items);
            
            return new PagedResult<LogEntryDto>
            {
                Items = logDtos.ToList(),
                TotalCount = logs.TotalCount,
                CurrentPage = logs.CurrentPage,
                PageSize = logs.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching logs with filters: ServiceName={ServiceName}, Level={Level}", 
                request.ServiceName, request.Level);
            throw;
        }
    }
}