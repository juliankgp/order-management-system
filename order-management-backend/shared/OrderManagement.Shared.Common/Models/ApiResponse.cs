namespace OrderManagement.Shared.Common.Models;

/// <summary>
/// Respuesta estándar para APIs
/// </summary>
/// <typeparam name="T">Tipo de datos de la respuesta</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Mensaje descriptivo de la respuesta
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Datos de la respuesta
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// Lista de errores (si los hay)
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// Timestamp de la respuesta
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

/// <summary>
/// Respuesta sin datos específicos
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Crea una respuesta exitosa sin datos
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Operation successful")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }
    
    /// <summary>
    /// Crea una respuesta de error sin datos
    /// </summary>
    public static new ApiResponse ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
