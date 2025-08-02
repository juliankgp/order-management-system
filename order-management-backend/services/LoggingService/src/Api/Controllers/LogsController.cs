using LoggingService.Application.Commands.CreateLog;
using LoggingService.Application.DTOs;
using LoggingService.Application.Queries.GetLogs;
using LoggingService.Application.Queries.SearchLogs;
using LoggingService.Domain.Entities;
using MediatR;
using LogLevel = LoggingService.Domain.Entities.LogLevel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Shared.Common.Models;

namespace LoggingService.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LogsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<LogsController> _logger;

    public LogsController(IMediator mediator, ILogger<LogsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los logs con paginación
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<LogEntryDto>>>> GetLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new GetLogsQuery { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(query);

            return Ok(ApiResponse<PagedResult<LogEntryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return StatusCode(500, ApiResponse<PagedResult<LogEntryDto>>.ErrorResponse("Error retrieving logs"));
        }
    }

    /// <summary>
    /// Busca logs con filtros específicos
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<PagedResult<LogEntryDto>>>> SearchLogs(
        [FromQuery] string? serviceName,
        [FromQuery] LogLevel? level,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string? category,
        [FromQuery] string? correlationId,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new SearchLogsQuery
            {
                ServiceName = serviceName,
                Level = level,
                FromDate = fromDate,
                ToDate = toDate,
                Category = category,
                CorrelationId = correlationId,
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<LogEntryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching logs with filters");
            return StatusCode(500, ApiResponse<PagedResult<LogEntryDto>>.ErrorResponse("Error searching logs"));
        }
    }

    /// <summary>
    /// Obtiene logs de un servicio específico
    /// </summary>
    [HttpGet("service/{serviceName}")]
    public async Task<ActionResult<ApiResponse<PagedResult<LogEntryDto>>>> GetLogsByService(
        string serviceName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new SearchLogsQuery
            {
                ServiceName = serviceName,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<LogEntryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for service {ServiceName}", serviceName);
            return StatusCode(500, ApiResponse<PagedResult<LogEntryDto>>.ErrorResponse($"Error retrieving logs for service {serviceName}"));
        }
    }

    /// <summary>
    /// Obtiene logs por correlationId para seguimiento de requests
    /// </summary>
    [HttpGet("correlation/{correlationId}")]
    public async Task<ActionResult<ApiResponse<PagedResult<LogEntryDto>>>> GetLogsByCorrelationId(
        string correlationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new SearchLogsQuery
            {
                CorrelationId = correlationId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<LogEntryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for correlationId {CorrelationId}", correlationId);
            return StatusCode(500, ApiResponse<PagedResult<LogEntryDto>>.ErrorResponse($"Error retrieving logs for correlationId {correlationId}"));
        }
    }

    /// <summary>
    /// Obtiene logs de un usuario específico
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponse<PagedResult<LogEntryDto>>>> GetLogsByUser(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var query = new SearchLogsQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return Ok(ApiResponse<PagedResult<LogEntryDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs for user {UserId}", userId);
            return StatusCode(500, ApiResponse<PagedResult<LogEntryDto>>.ErrorResponse($"Error retrieving logs for user {userId}"));
        }
    }

    /// <summary>
    /// Crea una nueva entrada de log (usado principalmente por otros servicios)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LogEntryDto>>> CreateLog([FromBody] CreateLogEntryDto createLogDto)
    {
        try
        {
            var command = new CreateLogCommand
            {
                Level = createLogDto.Level,
                Message = createLogDto.Message,
                ServiceName = createLogDto.ServiceName,
                Category = createLogDto.Category,
                CorrelationId = createLogDto.CorrelationId,
                UserId = createLogDto.UserId,
                Exception = createLogDto.Exception,
                StackTrace = createLogDto.StackTrace,
                Properties = createLogDto.Properties,
                Timestamp = createLogDto.Timestamp,
                MachineName = createLogDto.MachineName,
                Environment = createLogDto.Environment,
                ApplicationVersion = createLogDto.ApplicationVersion
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetLogs), new { }, ApiResponse<LogEntryDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating log entry");
            return StatusCode(500, ApiResponse<LogEntryDto>.ErrorResponse("Error creating log entry"));
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "LoggingService", timestamp = DateTime.UtcNow });
    }
}