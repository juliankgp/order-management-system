using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using OrderManagement.Shared.Security.Models;

namespace OrderManagement.Shared.Security.Services;

/// <summary>
/// Interface para el servicio de tokens JWT
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Genera un token JWT para el usuario
    /// </summary>
    string GenerateToken(UserClaims user);
    
    /// <summary>
    /// Valida un token JWT
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);
    
    /// <summary>
    /// Extrae claims del usuario desde un token
    /// </summary>
    UserClaims? GetUserClaims(string token);
    
    /// <summary>
    /// Obtiene los parámetros de validación de tokens
    /// </summary>
    TokenValidationParameters GetValidationParameters();
}

/// <summary>
/// Implementación robusta del servicio JWT
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<JwtService> _logger;

    // Configuración estática para garantizar consistencia
    public static class Constants
    {
        public const string SECRET_KEY = "OrderManagement-JWT-Secret-Key-2025-Super-Secure-At-Least-256-Bits-Long";
        public const string ISSUER = "OrderManagementSystem";
        public const string AUDIENCE = "OrderManagementSystem";
        public const int EXPIRE_MINUTES = 60;
    }

    public JwtService(IOptions<JwtSettings> jwtSettings, ILogger<JwtService> logger)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        
        // Usar configuración estática como fallback para garantizar consistencia
        var key = !string.IsNullOrEmpty(_jwtSettings.Key) ? _jwtSettings.Key : Constants.SECRET_KEY;
        var issuer = !string.IsNullOrEmpty(_jwtSettings.Issuer) ? _jwtSettings.Issuer : Constants.ISSUER;
        var audience = !string.IsNullOrEmpty(_jwtSettings.Audience) ? _jwtSettings.Audience : Constants.AUDIENCE;
        
        _logger.LogInformation("JWT Service initialized - Key length: {KeyLength}, Issuer: {Issuer}, Audience: {Audience}", 
            key.Length, issuer, audience);
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) { KeyId = "OrderManagementKey" };
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = new[] { signingKey },
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }

    public string GenerateToken(UserClaims user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = !string.IsNullOrEmpty(_jwtSettings.Key) ? _jwtSettings.Key : Constants.SECRET_KEY;
            var keyBytes = Encoding.UTF8.GetBytes(key);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Agregar roles como claims
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var issuer = !string.IsNullOrEmpty(_jwtSettings.Issuer) ? _jwtSettings.Issuer : Constants.ISSUER;
            var audience = !string.IsNullOrEmpty(_jwtSettings.Audience) ? _jwtSettings.Audience : Constants.AUDIENCE;
            var expireMinutes = _jwtSettings.ExpireMinutes > 0 ? _jwtSettings.ExpireMinutes : Constants.EXPIRE_MINUTES;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes) { KeyId = "OrderManagementKey" }, 
                    SecurityAlgorithms.HmacSha256
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            
            _logger.LogDebug("JWT token generated for user {UserId} with expiration {Expiration}", 
                user.UserId, tokenDescriptor.Expires);
            
            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.UserId);
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Empty or null token provided for validation");
                return null;
            }
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);
            
            _logger.LogDebug("JWT token validated successfully");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("JWT token expired: {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenValidationException ex)
        {
            _logger.LogWarning("JWT token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JWT token validation");
            return null;
        }
    }

    public UserClaims? GetUserClaims(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
        var nameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
        var roleClaims = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (userIdClaim == null || emailClaim == null || nameClaim == null ||
            !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Invalid claims in JWT token");
            return null;
        }

        return new UserClaims
        {
            UserId = userId,
            Email = emailClaim,
            FullName = nameClaim,
            Roles = roleClaims
        };
    }
    
    public TokenValidationParameters GetValidationParameters()
    {
        return _tokenValidationParameters;
    }
}
