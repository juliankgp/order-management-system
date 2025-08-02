using LoggingService.Domain.Entities;

namespace LoggingService.Application.DTOs;

public class LogEntryDto
{
    public Guid Id { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public Guid? UserId { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public string? Properties { get; set; }
    public DateTime Timestamp { get; set; }
    public string? MachineName { get; set; }
    public string? Environment { get; set; }
    public string? ApplicationVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateLogEntryDto
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

public class LogSearchDto
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
    public string? SearchTerm { get; set; }
}