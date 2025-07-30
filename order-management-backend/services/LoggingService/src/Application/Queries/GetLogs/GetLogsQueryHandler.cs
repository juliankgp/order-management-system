using AutoMapper;
using LoggingService.Application.DTOs;
using LoggingService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Application.Queries.GetLogs;

public class GetLogsQueryHandler : IRequestHandler<GetLogsQuery, PagedResult<LogEntryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLogsQueryHandler> _logger;

    public GetLogsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetLogsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<LogEntryDto>> Handle(GetLogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var logs = await _unitOfWork.LogEntries.GetAllAsync(request.Page, request.PageSize);
            
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
            _logger.LogError(ex, "Error retrieving logs");
            throw;
        }
    }
}