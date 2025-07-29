namespace OrderManagement.Shared.Security.Models;

/// <summary>
/// Configuración JWT
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Clave secreta para firmar tokens
    /// </summary>
    public required string Key { get; set; }
    
    /// <summary>
    /// Emisor del token
    /// </summary>
    public required string Issuer { get; set; }
    
    /// <summary>
    /// Audiencia del token
    /// </summary>
    public required string Audience { get; set; }
    
    /// <summary>
    /// Tiempo de expiración en minutos
    /// </summary>
    public int ExpireMinutes { get; set; } = 60;
}

/// <summary>
/// Claims de usuario
/// </summary>
public class UserClaims
{
    public required Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Resultado de autenticación
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserClaims? User { get; set; }
    public List<string> Errors { get; set; } = new();
}
