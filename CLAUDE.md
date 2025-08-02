# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a microservices-based order management system built with .NET 8. The system implements Clean Architecture with CQRS pattern, using separate databases per service and event-driven communication via RabbitMQ.

## Commands for Development

### Building and Running Services

```bash
# Build entire solution
dotnet build order-management-backend/OrderManagement.sln

# Run individual services (from order-management-backend directory)
dotnet run --project services/OrderService/src/Api --urls="https://localhost:5001"
dotnet run --project services/ProductService/src/Api --urls="https://localhost:5002"
dotnet run --project services/CustomerService/src/Api --urls="https://localhost:5003"
dotnet run --project services/LoggingService/src/Api --urls="https://localhost:5004"

# Start all services with PowerShell script
.\infra\scripts\start-services.ps1

# Using Docker Compose
docker-compose -f infra/docker/docker-compose.yml up -d
```

### Database Operations

```bash
# Setup databases (PowerShell)
.\infra\scripts\setup-databases.ps1

# Run migrations (PowerShell)
.\infra\scripts\run-migrations.ps1

# Create new migration
dotnet ef migrations add [MigrationName] --context [ContextName] --project services/[ServiceName]/src/Infrastructure

# Apply migrations
dotnet ef database update --context [ContextName] --project services/[ServiceName]/src/Infrastructure
```

### Testing

```bash
# Run all tests
dotnet test order-management-backend/OrderManagement.sln

# Run tests for specific service
dotnet test services/OrderService/tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Architecture Overview

### Microservices Structure
- **OrderService** (Port 5001): Order lifecycle management, CRUD operations
- **ProductService** (Port 5002): Product catalog and inventory management
- **CustomerService** (Port 5003): Customer management and authentication
- **LoggingService** (Port 5004): Centralized logging and auditing

### Communication Patterns
- **Synchronous**: HTTP/REST for direct service-to-service calls
- **Asynchronous**: RabbitMQ events for decoupled communication
- **Authentication**: JWT tokens across all services

### Clean Architecture Layers (per service)
```
Web/Api Layer (Controllers, Middleware)
    ↓
Application Layer (Commands/Queries, Handlers - CQRS + MediatR)
    ↓
Domain Layer (Entities, Repository Interfaces, Domain Events)
    ↓
Infrastructure Layer (EF Core, RabbitMQ, External Services)
```

### Key Patterns Implemented
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management  
- **CQRS**: Commands (write) and Queries (read) separation
- **Event-Driven Architecture**: Domain events via RabbitMQ
- **Mediator Pattern**: Request/response handling with MediatR

## Project Structure

### Solution Organization
```
order-management-backend/
├── services/
│   ├── OrderService/src/{Domain,Application,Infrastructure,Api}/
│   ├── ProductService/src/{Domain,Application,Infrastructure,Api}/
│   ├── CustomerService/src/{Domain,Application,Infrastructure,Api}/
│   └── LoggingService/src/{Domain,Application,Infrastructure,Api}/
├── shared/
│   ├── OrderManagement.Shared.Common/
│   ├── OrderManagement.Shared.Events/
│   └── OrderManagement.Shared.Security/
├── infra/
│   ├── docker/docker-compose.yml
│   └── scripts/{setup-databases.ps1, run-migrations.ps1, start-services.ps1}
└── OrderManagement.sln
```

### Service Boundaries
- **OrderService**: Manages Order and OrderItem entities, publishes OrderCreated/OrderStatusUpdated events
- **ProductService**: Manages Product and StockMovement entities, consumes OrderCreated events for stock updates
- **CustomerService**: Manages Customer and CustomerAddress entities, handles JWT authentication
- **LoggingService**: Manages LogEntry entity, consumes all system events for auditing

## Development Conventions

### Naming Conventions
- **PascalCase**: Classes, methods, properties, files, folders
- **camelCase**: Local variables, method parameters
- **Interfaces**: Prefix with `I` (e.g., `IOrderRepository`)
- **Services**: Suffix with `Service` (e.g., `OrderService`)
- **Repositories**: Suffix with `Repository` (e.g., `OrderRepository`)

### CQRS Command/Query Naming
```csharp
// Commands (Write operations)
CreateOrderCommand, UpdateOrderCommand, DeleteOrderCommand

// Queries (Read operations)  
GetOrderQuery, GetOrdersQuery

// Handlers
CreateOrderCommandHandler, GetOrderQueryHandler
```

### File Locations by Service
- **Domain**: `services/{ServiceName}/src/Domain/` - Entities, repository interfaces
- **Application**: `services/{ServiceName}/src/Application/` - Commands, queries, handlers, DTOs
- **Infrastructure**: `services/{ServiceName}/src/Infrastructure/` - EF DbContext, repositories, external services
- **Api**: `services/{ServiceName}/src/Api/` - Controllers, Program.cs, configuration
  - All services now use consistent `Api` folder structure

## Database Configuration

Each service uses its own SQL Server database:
- OrderManagement_Orders (OrderService)
- OrderManagement_Products (ProductService)  
- OrderManagement_Customers (CustomerService)
- OrderManagement_Logs (LoggingService)

Connection strings are configured in appsettings.json per environment.

## External Dependencies

### Required Infrastructure
- **SQL Server**: Database persistence
- **RabbitMQ**: Message broker (ports 5672, 15672 for management UI)
- **.NET 8 SDK**: Development and runtime

### Key NuGet Packages
- **Entity Framework Core**: ORM and migrations
- **MediatR**: CQRS mediator pattern
- **FluentValidation**: Command validation
- **Serilog**: Structured logging
- **RabbitMQ.Client**: Message broker integration
- **xUnit**: Unit testing framework

## Service URLs (Development)
- OrderService: https://localhost:5001/swagger
- ProductService: https://localhost:5002/swagger  
- CustomerService: https://localhost:5003/swagger
- LoggingService: https://localhost:5004/swagger
- RabbitMQ Management: http://localhost:15672 (guest/guest)

## Notes for Code Changes

When modifying services, always consider:
1. **Cross-service impacts**: Changes may affect event contracts
2. **Database migrations**: Required for entity changes
3. **Event versioning**: Maintain backward compatibility for events
4. **Clean Architecture**: Respect layer boundaries and dependency direction
5. **Testing**: Add unit tests for handlers and integration tests for APIs

The current state shows many `*_new.cs` files suggesting ongoing refactoring. Use existing patterns and maintain consistency with the established architecture.

## Working Directory Context

The main working directory is the `order-management-backend` folder. All commands should be run from this directory unless otherwise specified. The project uses Git with the current branch being `order-managment-services` and the main branch is `master`.

## Debugging and Launch Configuration

Visual Studio Code launch configurations are available in `.vscode/launch.json` for debugging individual services. Each service has its own launch profile with proper JWT configuration and debug endpoints.