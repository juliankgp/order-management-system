using AutoMapper;
using LoggingService.Application.DTOs;
using LoggingService.Domain.Entities;
using LoggingService.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LoggingService.Application.Commands.CreateLog;

public class CreateLogCommandHandler : IRequestHandler<CreateLogCommand, LogEntryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateLogCommandHandler> _logger;

    public CreateLogCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateLogCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LogEntryDto> Handle(CreateLogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var logEntry = new LogEntry
            {
                Level = request.Level,
                Message = request.Message,
                ServiceName = request.ServiceName,
                Category = request.Category,
                CorrelationId = request.CorrelationId,
                UserId = request.UserId,
                Exception = request.Exception,
                StackTrace = request.StackTrace,
                Properties = request.Properties,
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                MachineName = request.MachineName ?? Environment.MachineName,
                Environment = request.Environment ?? "Development",
                ApplicationVersion = request.ApplicationVersion
            };

            var createdLogEntry = await _unitOfWork.LogEntries.AddAsync(logEntry);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<LogEntryDto>(createdLogEntry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating log entry for service {ServiceName}", request.ServiceName);
            throw;
        }
    }
}