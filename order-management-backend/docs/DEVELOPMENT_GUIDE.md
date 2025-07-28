# Guía de Desarrollo - Order Management System

## Configuración del Entorno de Desarrollo

### Prerrequisitos
1. **.NET 8 SDK**
   ```bash
   # Verificar instalación
   dotnet --version
   ```

2. **SQL Server** (LocalDB o instancia completa)
   ```bash
   # Verificar conectividad
   sqlcmd -S localhost -E -Q "SELECT @@VERSION"
   ```

3. **Visual Studio 2022** o **VS Code** con extensiones:
   - C# Dev Kit
   - REST Client
   - Docker (opcional)

4. **Git**
   ```bash
   git --version
   ```

### Configuración Inicial

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/[usuario]/order-management-backend.git
   cd order-management-backend
   ```

2. **Configurar bases de datos**
   ```powershell
   .\infra\scripts\setup-databases.ps1
   ```

3. **Ejecutar migraciones**
   ```powershell
   .\infra\scripts\run-migrations.ps1
   ```

4. **Instalar RabbitMQ**
   ```bash
   # Con Docker
   docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

## Estructura del Proyecto

### Convenciones de Nomenclatura

#### Archivos y Carpetas
- **PascalCase** para carpetas: `OrderService`, `Application`
- **PascalCase** para archivos: `OrderController.cs`, `CreateOrderCommand.cs`
- **camelCase** para variables locales: `var orderItems = ...`

#### Clases y Métodos
```csharp
// ✅ Correcto
public class OrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        // ...
    }
}

// ❌ Incorrecto
public class orderservice
{
    public async Task<Order> createOrder(createOrderCommand command)
    {
        // ...
    }
}
```

#### Interfaces
```csharp
// ✅ Correcto - Prefijo 'I'
public interface IOrderRepository
{
    Task<Order> GetByIdAsync(Guid id);
}
```

#### Comandos y Queries (CQRS)
```csharp
// Comandos (escritura)
public record CreateOrderCommand : IRequest<ApiResponse<CreateOrderResponse>>
public record UpdateOrderStatusCommand : IRequest<ApiResponse>
public record DeleteOrderCommand : IRequest<ApiResponse>

// Queries (lectura)
public record GetOrderQuery : IRequest<ApiResponse<OrderDto>>
public record GetOrdersQuery : IRequest<ApiResponse<PagedResult<OrderDto>>>
```

### Separación por Contextos de Dominio

#### OrderService - Contexto de Órdenes
```
Responsabilidades:
- Gestión del ciclo de vida de órdenes
- Cálculo de totales y impuestos
- Validación de reglas de negocio de órdenes
- Coordinación entre productos y clientes

Entities:
- Order, OrderItem, OrderStatusHistory

Events Published:
- OrderCreated, OrderStatusUpdated, OrderCancelled

Events Consumed:
- ProductStockUpdated, CustomerRegistered
```

#### ProductService - Contexto de Productos
```
Responsabilidades:
- Gestión de catálogo de productos
- Control de inventario y stock
- Historial de precios
- Categorización de productos

Entities:
- Product, StockMovement, PriceHistory

Events Published:
- ProductStockUpdated, ProductCreated, ProductPriceChanged

Events Consumed:
- OrderCreated (para reservar stock)
```

#### CustomerService - Contexto de Clientes
```
Responsabilidades:
- Autenticación y autorización
- Gestión de perfiles de cliente
- Direcciones de envío y facturación
- Preferencias del cliente

Entities:
- Customer, CustomerAddress

Events Published:
- CustomerRegistered, CustomerUpdated

Events Consumed:
- OrderCreated (para actualizar historial)
```

#### LoggingService - Contexto de Auditoría
```
Responsabilidades:
- Centralización de logs del sistema
- Auditoría de acciones
- Métricas y monitoreo
- Almacenamiento de eventos del sistema

Entities:
- LogEntry

Events Published:
- Ninguno (consumer únicamente)

Events Consumed:
- Todos los eventos del sistema para auditoría
```

## Conectividad entre Servicios

### 1. Comunicación HTTP (Síncrona)

#### Configuración de Cliente HTTP
```csharp
// En Program.cs de cada servicio
builder.Services.AddHttpClient<IProductService, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:ProductService:BaseUrl"]);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

#### Implementación de Cliente
```csharp
public class ProductServiceClient : IProductService
{
    private readonly HttpClient _httpClient;
    
    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<ProductDto>> GetProductsAsync(List<Guid> productIds)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products/batch", productIds);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
        return result?.Data ?? new List<ProductDto>();
    }
}
```

### 2. Comunicación RabbitMQ (Asíncrona)

#### Configuración del Event Bus
```csharp
// En Program.cs
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<IEventBus, RabbitMQEventBus>();
```

#### Publicación de Eventos
```csharp
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponse<CreateOrderResponse>>
{
    private readonly IEventBus _eventBus;
    
    public async Task<ApiResponse<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // ... lógica de creación de orden
        
        // Publicar evento
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new OrderItemCreated
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
        
        await _eventBus.PublishAsync(orderCreatedEvent, cancellationToken);
        
        return ApiResponse<CreateOrderResponse>.SuccessResponse(response);
    }
}
```

#### Consumo de Eventos
```csharp
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IProductRepository _productRepository;
    
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken)
    {
        // Actualizar stock de productos
        foreach (var item in @event.Items)
        {
            await _productRepository.UpdateStockAsync(item.ProductId, -item.Quantity, "Order Created", @event.OrderId);
        }
    }
}

// Configuración en Program.cs
var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<OrderCreatedEvent, OrderCreatedEventHandler>();
```

## Configuración de Conexiones

### appsettings.json por Ambiente

#### Development
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrderManagement_Orders;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest",
    "Port": 5672
  },
  "Services": {
    "ProductService": {
      "BaseUrl": "https://localhost:5002"
    },
    "CustomerService": {
      "BaseUrl": "https://localhost:5003"
    }
  },
  "Jwt": {
    "Key": "OrderManagementSystemSuperSecretKeyForDevelopment2024!",
    "Issuer": "OrderManagementSystem",
    "Audience": "OrderManagementSystem",
    "ExpireMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

#### Production
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=OrderManagement_Orders;User Id=app_user;Password=${DB_PASSWORD};TrustServerCertificate=true;"
  },
  "RabbitMQ": {
    "HostName": "prod-rabbitmq",
    "UserName": "${RABBITMQ_USER}",
    "Password": "${RABBITMQ_PASSWORD}",
    "Port": 5672
  },
  "Jwt": {
    "Key": "${JWT_SECRET_KEY}",
    "Issuer": "OrderManagementSystem",
    "Audience": "OrderManagementSystem",
    "ExpireMinutes": 30
  }
}
```

## Flujo de Desarrollo

### 1. Agregar Nueva Funcionalidad

#### Ejemplo: Agregar descuentos a órdenes

1. **Modificar Entity (Domain)**
   ```csharp
   // En Order.cs
   public decimal DiscountAmount { get; set; }
   public string? DiscountCode { get; set; }
   ```

2. **Crear Migración**
   ```bash
   dotnet ef migrations add AddDiscountToOrder --context OrderDbContext
   ```

3. **Agregar Command (Application)**
   ```csharp
   public record ApplyDiscountCommand : IRequest<ApiResponse>
   {
       public required Guid OrderId { get; init; }
       public required string DiscountCode { get; init; }
   }
   ```

4. **Implementar Handler**
   ```csharp
   public class ApplyDiscountCommandHandler : IRequestHandler<ApplyDiscountCommand, ApiResponse>
   {
       // ... implementación
   }
   ```

5. **Agregar Controller Action**
   ```csharp
   [HttpPost("{id}/discount")]
   public async Task<ActionResult<ApiResponse>> ApplyDiscount(Guid id, ApplyDiscountRequest request)
   {
       var command = new ApplyDiscountCommand { OrderId = id, DiscountCode = request.DiscountCode };
       var result = await _mediator.Send(command);
       return Ok(result);
   }
   ```

6. **Agregar Tests**
   ```csharp
   [Fact]
   public async Task ApplyDiscount_ValidCode_ShouldUpdateOrder()
   {
       // Arrange
       // Act
       // Assert
   }
   ```

### 2. Testing

#### Test Unitario
```csharp
public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<IProductService> _mockProductService;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockEventBus = new Mock<IEventBus>();
        _mockProductService = new Mock<IProductService>();
        _handler = new CreateOrderCommandHandler(_mockUnitOfWork.Object, _mockEventBus.Object, _mockProductService.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOrder()
    {
        // Arrange
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            },
            ShippingAddress = "123 Test St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        var products = new List<ProductDto>
        {
            new() { Id = command.Items[0].ProductId, Name = "Test Product", Sku = "TEST-001", Price = 10.00m, Stock = 5 }
        };

        _mockProductService.Setup(x => x.GetProductsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(products);

        _mockUnitOfWork.Setup(x => x.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Order o, CancellationToken ct) => o);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        _mockEventBus.Verify(x => x.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

#### Test de Integración
```csharp
public class OrderControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrderControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            CustomerId = Guid.NewGuid(),
            Items = new List<CreateOrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 1 }
            },
            ShippingAddress = "123 Test St",
            ShippingCity = "Test City",
            ShippingZipCode = "12345",
            ShippingCountry = "Test Country"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
```

## Comandos Útiles

### Desarrollo Local
```bash
# Compilar solución completa
dotnet build

# Ejecutar tests
dotnet test

# Ejecutar servicio específico
dotnet run --project services/OrderService/src/Web

# Crear nueva migración
dotnet ef migrations add [MigrationName] --context [ContextName]

# Aplicar migraciones
dotnet ef database update --context [ContextName]

# Ver migraciones pendientes
dotnet ef migrations list --context [ContextName]
```

### Docker
```bash
# Construir imagen
docker build -t order-service -f services/OrderService/Dockerfile .

# Ejecutar con docker-compose
docker-compose -f infra/docker/docker-compose.yml up -d

# Ver logs
docker-compose -f infra/docker/docker-compose.yml logs order-service

# Detener servicios
docker-compose -f infra/docker/docker-compose.yml down
```

### PowerShell Scripts
```powershell
# Configurar bases de datos
.\infra\scripts\setup-databases.ps1

# Ejecutar migraciones
.\infra\scripts\run-migrations.ps1

# Iniciar todos los servicios
.\infra\scripts\start-services.ps1
```

## Troubleshooting

### Problemas Comunes

1. **Error de conexión a SQL Server**
   ```
   Solución: Verificar que SQL Server esté ejecutándose y la cadena de conexión sea correcta
   ```

2. **RabbitMQ no disponible**
   ```
   Solución: docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
   ```

3. **Puerto en uso**
   ```
   Solución: Cambiar puerto en launchSettings.json o matar proceso que usa el puerto
   netstat -ano | findstr :5001
   taskkill /PID [PID] /F
   ```

4. **Migraciones pendientes**
   ```
   Solución: dotnet ef database update --context [ContextName]
   ```

### Logs y Debugging

#### Configurar Serilog para debugging
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/debug-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

#### Usar logging en handlers
```csharp
public class CreateOrderCommandHandler
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    
    public async Task<ApiResponse<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);
        
        try
        {
            // ... lógica
            _logger.LogInformation("Order created successfully with ID {OrderId}", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
```
