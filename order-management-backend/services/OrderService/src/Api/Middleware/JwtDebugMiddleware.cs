using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Shared.Security.Services;

namespace OrderService.Api.Middleware;

public class JwtDebugMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<JwtDebugMiddleware> _logger;
    private readonly IConfiguration _configuration;

    public JwtDebugMiddleware(RequestDelegate next, ILogger<JwtDebugMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Solo log para requests con Authorization header
        if (context.Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (authHeader.StartsWith("Bearer "))
            {
                var token = authHeader["Bearer ".Length..];
                _logger.LogInformation("=== JWT DEBUG START ===");
                _logger.LogInformation("Path: {Path}", context.Request.Path);
                _logger.LogInformation("Token Length: {Length}", token.Length);
                
                // Verificar configuración
                var key = _configuration["Jwt:Key"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                
                _logger.LogInformation("Config - Key Length: {KeyLength}, Issuer: {Issuer}, Audience: {Audience}", 
                    key?.Length ?? 0, issuer, audience);
                
                // Intentar validar manualmente
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var keyBytes = Encoding.UTF8.GetBytes(key ?? JwtService.Constants.SECRET_KEY);
                    
                    var signingKey = new SymmetricSecurityKey(keyBytes) { KeyId = "OrderManagementKey" };
                    
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = new[] { signingKey },
                        ValidateIssuer = true,
                        ValidIssuer = issuer ?? JwtService.Constants.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = audience ?? JwtService.Constants.AUDIENCE,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5),
                        RequireExpirationTime = true,
                        RequireSignedTokens = true
                    };
                    
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                    _logger.LogInformation("Manual validation: SUCCESS - User: {User}", principal.Identity?.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Manual validation: FAILED - {Message}", ex.Message);
                }
                
                _logger.LogInformation("=== JWT DEBUG END ===");
            }
        }

        await _next(context);
    }
}