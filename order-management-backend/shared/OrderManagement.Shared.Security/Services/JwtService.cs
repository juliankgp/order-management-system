using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
}

/// <summary>
/// Implementación del servicio JWT
/// </summary>
public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public string GenerateToken(UserClaims user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        
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

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
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
}
