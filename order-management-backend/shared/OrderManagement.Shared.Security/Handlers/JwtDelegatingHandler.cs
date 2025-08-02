using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace OrderManagement.Shared.Security.Handlers;

/// <summary>
/// DelegatingHandler que automáticamente agrega el JWT token a las peticiones HTTP salientes
/// </summary>
public class JwtDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Obtener el token del contexto HTTP actual
        var httpContext = _httpContextAccessor.HttpContext;
        
        if (httpContext != null)
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                // Agregar el token de autorización a la petición saliente
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationHeader.Substring("Bearer ".Length));
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}