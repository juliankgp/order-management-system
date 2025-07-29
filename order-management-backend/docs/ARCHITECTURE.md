# Diagrama de Arquitectura del Sistema

## Arquitectura General de Microservicios

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                      CLIENTE/APLICACIÓN                                         │
│                    (HTTP/HTTPS + JWT)                                          │
└─────────────────┬───────────────────────────────────────────────────────────────┘
                  │ HTTP/HTTPS + JWT
                  │
┌─────────────────▼───────────────────────────────────────────────────────────────┐
│                        API GATEWAY (Opcional)                                   │
│                     Nginx / Ocelot / YARP                                      │
└─────────────────┬───────────────────────────────────────────────────────────────┘
                  │
         ┌────────┴────────┬────────────────┬────────────────┐
         │                 │                │                │
┌────────▼────────┐ ┌──────▼──────┐ ┌───────▼───────┐ ┌──────▼──────┐
│  OrderService   │ │ProductService│ │CustomerService│ │LoggingService│
│   (Port 5001)   │ │ (Port 5002)  │ │ (Port 5003)   │ │ (Port 5004) │
└────────┬────────┘ └──────┬──────┘ └───────┬───────┘ └──────┬──────┘
         │                 │                │                │
         └─────────────────┼────────────────┼────────────────┘
                          │                │
                   ┌──────▼──────┐        │
                   │  RabbitMQ   │        │
                   │(Port 5672)  │        │
                   │Management   │        │
                   │(Port 15672) │        │
                   └─────────────┘        │
                                         │
         ┌────────────────────────────────┼─────────────────────────────────┐
         │                               │                                 │
┌────────▼────────┐ ┌────────────────┐ ┌▼─────────────┐ ┌─────────────────┐
│OrderManagement_ │ │OrderManagement_│ │OrderManagement│ │OrderManagement_ │
│    Orders       │ │   Products     │ │  Customers    │ │     Logs        │
│  (SQL Server)   │ │ (SQL Server)   │ │ (SQL Server)  │ │  (SQL Server)   │
└─────────────────┘ └────────────────┘ └───────────────┘ └─────────────────┘
```

## Flujo de Comunicación

### 1. Comunicación Síncrona (HTTP/REST)
```
Cliente ──HTTP──► CustomerService (Autenticación)
Cliente ──HTTP──► OrderService (CRUD Órdenes)
Cliente ──HTTP──► ProductService (CRUD Productos)
Cliente ──HTTP──► LoggingService (Consulta Logs)

OrderService ──HTTP──► ProductService (Validar Stock)
OrderService ──HTTP──► CustomerService (Validar Cliente)
```

### 2. Comunicación Asíncrona (Events via RabbitMQ)
```
OrderService ──Event──► RabbitMQ ──► ProductService (OrderCreated)
ProductService ──Event──► RabbitMQ ──► LoggingService (StockUpdated)
CustomerService ──Event──► RabbitMQ ──► LoggingService (CustomerRegistered)
All Services ──Event──► RabbitMQ ──► LoggingService (SystemLogs)
```

## Estructura de Microservicio (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                      Web Layer                              │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │   Controllers   │ │   Middleware    │ │  Configuration  ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                  Application Layer                          │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │    Commands     │ │     Queries     │ │    Handlers     ││
│  │     (CQRS)      │ │     (CQRS)      │ │   (MediatR)     ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                   Domain Layer                              │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │    Entities     │ │   Repositories  │ │  Domain Events  ││
│  │   (Business)    │ │  (Interfaces)   │ │   (Events)      ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
└─────────────────┬───────────────────────────────────────────┘
                  │
┌─────────────────▼───────────────────────────────────────────┐
│                Infrastructure Layer                         │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐│
│  │  Entity Framework│ │   RabbitMQ     │ │    Serilog      ││
│  │   (Data Access) │ │  (Messaging)    │ │   (Logging)     ││
│  └─────────────────┘ └─────────────────┘ └─────────────────┘│
└─────────────────────────────────────────────────────────────┘
```

## Patrones Implementados

### Repository Pattern
```
IOrderRepository ←─ OrderRepository (Infrastructure)
     ▲
     │
Domain Layer (Interface)
Infrastructure Layer (Implementation)
```

### Unit of Work Pattern
```
IUnitOfWork
├── IOrderRepository Orders
├── SaveChangesAsync()
├── BeginTransactionAsync()
├── CommitTransactionAsync()
└── RollbackTransactionAsync()
```

### CQRS (Command Query Responsibility Segregation)
```
Commands (Write)           Queries (Read)
├── CreateOrderCommand     ├── GetOrderQuery
├── UpdateOrderCommand     ├── GetOrdersQuery
└── DeleteOrderCommand     └── SearchOrdersQuery
```

### Event-Driven Architecture
```
OrderService ──OrderCreated──► RabbitMQ
                                  │
ProductService ◄──────────────────┘
     │
     └──ProductStockUpdated──► RabbitMQ
                                  │
LoggingService ◄──────────────────┘
```

## Tecnologías por Capa

### Backend
- **.NET 8**: Framework principal
- **Entity Framework Core**: ORM y migraciones
- **SQL Server**: Base de datos relacional
- **RabbitMQ**: Message broker para eventos
- **MediatR**: Patrón mediator para CQRS
- **FluentValidation**: Validación de comandos
- **Serilog**: Logging estructurado
- **JWT**: Autenticación y autorización
- **Docker**: Containerización
- **xUnit**: Testing unitario

### Shared Libraries
- **OrderManagement.Shared.Common**: Modelos base, excepciones, utilidades
- **OrderManagement.Shared.Events**: Eventos de dominio compartidos
- **OrderManagement.Shared.Security**: Configuración JWT y seguridad

## Bases de Datos

### OrderManagement_Orders
```sql
Orders
├── Id (uniqueidentifier)
├── OrderNumber (nvarchar(50))
├── CustomerId (uniqueidentifier)
├── Status (int)
├── TotalAmount (decimal(18,2))
└── ...

OrderItems
├── Id (uniqueidentifier)
├── OrderId (uniqueidentifier) FK
├── ProductId (uniqueidentifier)
├── Quantity (int)
└── ...
```

### OrderManagement_Products
```sql
Products
├── Id (uniqueidentifier)
├── Name (nvarchar(100))
├── Sku (nvarchar(50))
├── Price (decimal(18,2))
├── Stock (int)
└── ...

StockMovements
├── Id (uniqueidentifier)
├── ProductId (uniqueidentifier) FK
├── MovementType (int)
├── Quantity (int)
└── ...
```

### OrderManagement_Customers
```sql
Customers
├── Id (uniqueidentifier)
├── Email (nvarchar(255))
├── PasswordHash (nvarchar(255))
├── FirstName (nvarchar(50))
├── LastName (nvarchar(50))
└── ...

CustomerAddresses
├── Id (uniqueidentifier)
├── CustomerId (uniqueidentifier) FK
├── Type (int)
├── AddressLine1 (nvarchar(200))
└── ...
```

### OrderManagement_Logs
```sql
LogEntries
├── Id (uniqueidentifier)
├── Level (int)
├── Message (nvarchar(max))
├── ServiceName (nvarchar(100))
├── Timestamp (datetime2)
└── ...
```

## Seguridad

### Autenticación
- JWT tokens para autenticación stateless
- Claims-based authorization
- Refresh tokens para renovación automática

### Autorización
- Role-based access control (RBAC)
- Claims personalizados por servicio
- Middleware de autorización en cada API

### Comunicación
- HTTPS para todas las comunicaciones externas
- Certificados SSL/TLS
- API Keys para comunicación entre servicios internos

## Escalabilidad

### Horizontal Scaling
- Cada microservicio puede escalarse independientemente
- Load balancing con múltiples instancias
- Base de datos por servicio (Database per Service)

### Caching
- Redis para cache distribuido (futuro)
- Memory cache para datos frecuentemente accedidos
- Cache de autenticación JWT

### Performance
- Async/await en todas las operaciones I/O
- Connection pooling para bases de datos
- Lazy loading en Entity Framework
