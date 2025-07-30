using LoggingService.Application.DTOs;
using LoggingService.Domain.Entities;
using MediatR;

namespace LoggingService.Application.Commands.CreateLog;

public class CreateLogCommand : IRequest<LogEntryDto>
{
    public LogLevel Level { get; set; }
    public required string Message { get; set; }
    public required string ServiceName { get; set; }
    public required string Category { get; set; }
    public string? CorrelationId { get; set; }
    public Guid? UserId { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? Properties { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? MachineName { get; set; }
    public string? Environment { get; set; }
    public string? ApplicationVersion { get; set; }
}