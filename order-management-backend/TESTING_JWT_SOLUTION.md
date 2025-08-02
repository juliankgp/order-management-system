# Prueba de la Solución JWT para Comunicación entre Servicios

## Problema Resuelto

Se identificó que las comunicaciones HTTP entre servicios (especialmente desde OrderService hacia CustomerService y ProductService) fallaban porque no se incluía el token JWT en las peticiones salientes.

## Solución Implementada

1. **JwtDelegatingHandler**: Handler que automáticamente captura el JWT token de la petición entrante y lo agrega a las peticiones salientes.

2. **HttpClientExtensions**: Métodos de extensión para configurar fácilmente HttpClients con JWT automático.

3. **Configuración en OrderService**: Actualizado Program.cs para usar los nuevos HttpClients con JWT.

## Archivos Creados/Modificados

### Archivos Nuevos:
- `shared/OrderManagement.Shared.Security/Handlers/JwtDelegatingHandler.cs`
- `shared/OrderManagement.Shared.Security/Extensions/HttpClientExtensions.cs`
- `shared/OrderManagement.Shared.Security/README.md`

### Archivos Modificados:
- `services/OrderService/src/Api/Program.cs`

## Cómo Probar la Solución

### 1. Iniciar todos los servicios:

```bash
# Terminal 1 - CustomerService
dotnet run --project services/CustomerService/src/Api --urls="https://localhost:5003"

# Terminal 2 - ProductService  
dotnet run --project services/ProductService/src/Api --urls="https://localhost:5002"

# Terminal 3 - OrderService
dotnet run --project services/OrderService/src/Api --urls="https://localhost:5001"
```

### 2. Obtener un token JWT:

```bash
# Registrar/Autenticar en CustomerService
curl -X POST https://localhost:5003/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'
```

### 3. Probar comunicación entre servicios:

```bash
# Crear una orden (OrderService → CustomerService + ProductService)
curl -X POST https://localhost:5001/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "customerId": "CUSTOMER_GUID",
    "items": [{
      "productId": "PRODUCT_GUID",
      "quantity": 2,
      "unitPrice": 29.99
    }],
    "shippingAddress": "123 Test St",
    "shippingCity": "Test City",
    "shippingZipCode": "12345",
    "shippingCountry": "Test Country"
  }'
```

### 4. Verificar logs:

Revisar los logs de los servicios para confirmar que:
- OrderService recibe la petición con JWT
- OrderService hace llamadas a CustomerService/ProductService con JWT automáticamente
- Los servicios de destino validan correctamente el JWT

## Comportamiento Esperado

**ANTES (Con Error):**
- OrderService → CustomerService: Sin JWT = 401 Unauthorized
- OrderService → ProductService: Sin JWT = 401 Unauthorized

**DESPUÉS (Funcionando):**
- OrderService → CustomerService: Con JWT automático = 200 OK
- OrderService → ProductService: Con JWT automático = 200 OK

## Configuración para Otros Servicios

Si otros servicios necesitan hacer llamadas HTTP, usar la misma configuración:

```csharp
// En Program.cs del servicio
builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClientWithJwt<IExternalService, ExternalServiceImpl>(client =>
{
    client.BaseAddress = new Uri("https://otherservice:5000");
});
```

## Notas Importantes

1. **IHttpContextAccessor**: Requerido para acceder al contexto HTTP actual
2. **Orden de middleware**: El JWT middleware debe ejecutarse antes de las llamadas HTTP
3. **Testing**: En pruebas unitarias, mockar el IHttpContextAccessor o usar HttpClients sin el handler