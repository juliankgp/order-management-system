using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Shared.Security.Models;
using OrderManagement.Shared.Security.Services;
using System.Security.Claims;
using System.Text;

namespace OrderManagement.Shared.Security.Extensions;

/// <summary>
/// Extensiones para configuración JWT unificada
/// </summary>
public static class JwtExtensions
{
    /// <summary>
    /// Configura JWT de manera consistente para todos los servicios
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar JwtSettings
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        
        // Registrar JwtService
        services.AddScoped<IJwtService, JwtService>();
        
        // Configurar Authentication con JWT
        var key = configuration["Jwt:Key"] ?? JwtService.Constants.SECRET_KEY;
        var issuer = configuration["Jwt:Issuer"] ?? JwtService.Constants.ISSUER;
        var audience = configuration["Jwt:Audience"] ?? JwtService.Constants.AUDIENCE;
        
        var keyBytes = Encoding.UTF8.GetBytes(key);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Solo para desarrollo
                options.SaveToken = true;
                
                // Configurar para usar SecurityTokenValidators (compatibilidad) 
                options.UseSecurityTokenValidators = true;
                
                // Configurar SecurityTokenValidators explícitamente
                options.SecurityTokenValidators.Clear();
                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                tokenHandler.InboundClaimTypeMap.Clear(); // Evitar mapeo automático de claims
                options.SecurityTokenValidators.Add(tokenHandler);
                
                var signingKey = new SymmetricSecurityKey(keyBytes) { KeyId = "OrderManagementKey" };
                
                options.TokenValidationParameters = new TokenValidationParameters
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
                    // Configuración para manejar KeyIds correctamente
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogError(context.Exception, "JWT Authentication failed. Path: {Path}", context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogDebug("JWT Token validated successfully for user: {User}", context.Principal?.Identity?.Name);
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        var hasToken = !string.IsNullOrEmpty(context.Request.Headers.Authorization.FirstOrDefault());
                        logger.LogDebug("JWT Token received: {HasToken}, Path: {Path}", hasToken, context.Request.Path);
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerHandler>>();
                        logger.LogWarning("JWT Challenge issued. Error: {Error}, ErrorDescription: {ErrorDesc}, Path: {Path}", 
                            context.Error, context.ErrorDescription, context.Request.Path);
                        return Task.CompletedTask;
                    }
                };
            });
            
        services.AddAuthorization();
        
        return services;
    }
}