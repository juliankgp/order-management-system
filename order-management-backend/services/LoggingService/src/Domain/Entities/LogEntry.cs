using OrderManagement.Shared.Common.Models;

namespace LoggingService.Domain.Entities;

/// <summary>
/// Entidad que representa un log en el sistema
/// </summary>
public class LogEntry : BaseEntity
{
    /// <summary>
    /// Nivel del log
    /// </summary>
    public LogLevel Level { get; set; }
    
    /// <summary>
    /// Mensaje del log
    /// </summary>
    public required string Message { get; set; }
    
    /// <summary>
    /// Servicio que generó el log
    /// </summary>
    public required string ServiceName { get; set; }
    
    /// <summary>
    /// Categoría del log
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// ID de correlación para seguimiento de requests
    /// </summary>
    public string? CorrelationId { get; set; }
    
    /// <summary>
    /// ID del usuario asociado (si aplica)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Detalles de la excepción (si aplica)
    /// </summary>
    public string? Exception { get; set; }
    
    /// <summary>
    /// Stack trace de la excepción
    /// </summary>
    public string? StackTrace { get; set; }
    
    /// <summary>
    /// Propiedades adicionales en formato JSON
    /// </summary>
    public string? Properties { get; set; }
    
    /// <summary>
    /// Timestamp del log
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Información del host/servidor
    /// </summary>
    public string? MachineName { get; set; }
    
    /// <summary>
    /// Nombre del ambiente (Development, Production, etc.)
    /// </summary>
    public string? Environment { get; set; }
    
    /// <summary>
    /// Versión de la aplicación
    /// </summary>
    public string? ApplicationVersion { get; set; }
}

/// <summary>
/// Niveles de log
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Información de seguimiento detallada
    /// </summary>
    Trace = 0,
    
    /// <summary>
    /// Información de depuración
    /// </summary>
    Debug = 1,
    
    /// <summary>
    /// Información general
    /// </summary>
    Information = 2,
    
    /// <summary>
    /// Advertencia
    /// </summary>
    Warning = 3,
    
    /// <summary>
    /// Error
    /// </summary>
    Error = 4,
    
    /// <summary>
    /// Error crítico
    /// </summary>
    Critical = 5
}
