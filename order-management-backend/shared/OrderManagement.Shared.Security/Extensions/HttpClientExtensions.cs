using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Shared.Security.Handlers;

namespace OrderManagement.Shared.Security.Extensions;

/// <summary>
/// Métodos de extensión para configurar HttpClient con JWT automático
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Agrega un HttpClient con JWT automático a la colección de servicios
    /// </summary>
    /// <typeparam name="TClient">Tipo del cliente</typeparam>
    /// <typeparam name="TImplementation">Tipo de la implementación</typeparam>
    /// <param name="services">Colección de servicios</param>
    /// <param name="configureClient">Configuración adicional del cliente</param>
    /// <returns>IHttpClientBuilder para configuraciones adicionales</returns>
    public static IHttpClientBuilder AddHttpClientWithJwt<TClient, TImplementation>(
        this IServiceCollection services,
        Action<HttpClient>? configureClient = null)
        where TClient : class
        where TImplementation : class, TClient
    {
        // Registrar el DelegatingHandler
        services.AddTransient<JwtDelegatingHandler>();
        
        // Registrar el HttpClient con el handler JWT
        var builder = services.AddHttpClient<TClient, TImplementation>(client =>
        {
            configureClient?.Invoke(client);
        })
        .AddHttpMessageHandler<JwtDelegatingHandler>();

        return builder;
    }

    /// <summary>
    /// Agrega un HttpClient nombrado con JWT automático a la colección de servicios
    /// </summary>
    /// <param name="services">Colección de servicios</param>
    /// <param name="name">Nombre del cliente</param>
    /// <param name="configureClient">Configuración del cliente</param>
    /// <returns>IHttpClientBuilder para configuraciones adicionales</returns>
    public static IHttpClientBuilder AddHttpClientWithJwt(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null)
    {
        // Registrar el DelegatingHandler
        services.AddTransient<JwtDelegatingHandler>();
        
        // Registrar el HttpClient nombrado con el handler JWT
        var builder = services.AddHttpClient(name, client =>
        {
            configureClient?.Invoke(client);
        })
        .AddHttpMessageHandler<JwtDelegatingHandler>();

        return builder;
    }
}