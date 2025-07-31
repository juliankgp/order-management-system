# OrderManagement.Shared.Security

Este proyecto contiene utilidades compartidas para autenticación y autorización JWT entre los microservicios del sistema.

## JWT Automático en HttpClient

El proyecto incluye un `JwtDelegatingHandler` que automáticamente agrega el token JWT de la petición entrante a todas las peticiones HTTP salientes. Esto resuelve el problema de autenticación entre servicios.

### Uso

1. **Configuración básica en Program.cs:**

```csharp
using OrderManagement.Shared.Security.Extensions;

// Agregar HttpContextAccessor (requerido)
builder.Services.AddHttpContextAccessor();

// Usar HttpClient con JWT automático
builder.Services.AddHttpClientWithJwt<IMyService, MyServiceImplementation>(client =>
{
    client.BaseAddress = new Uri("https://localhost:5002");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

2. **Para HttpClient nombrado:**

```csharp
builder.Services.AddHttpClientWithJwt("MyNamedClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:5002");
});
```

### Cómo funciona

1. El `JwtDelegatingHandler` intercepta todas las peticiones HTTP salientes
2. Extrae el token JWT del header `Authorization` de la petición HTTP entrante actual
3. Automáticamente agrega ese token a la petición saliente
4. Esto permite que las llamadas entre servicios mantengan el contexto de autenticación

### Ejemplo completo en OrderService

```csharp
// Program.cs
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClientWithJwt<IProductService, ProductService>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:ProductService:BaseUrl"] ?? "http://localhost:5002";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClientWithJwt<ICustomerService, CustomerService>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:CustomerService:BaseUrl"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### Requisitos

- El servicio debe tener configurado `IHttpContextAccessor`
- Las peticiones entrantes deben incluir el header `Authorization: Bearer <token>`
- Los servicios de destino deben estar configurados para validar JWT tokens

### Beneficios

- **Automático**: No necesitas modificar cada llamada HTTP individual
- **Transparente**: Los servicios existentes funcionan sin cambios
- **Consistente**: Todas las peticiones HTTP salientes incluyen el token
- **Mantenible**: Configuración centralizada en Program.cs