# Order Management System - Backend

## Arquitectura de Microservicios

Este proyecto implementa un sistema de gestión de órdenes basado en microservicios utilizando .NET 8, Entity Framework Core, RabbitMQ y SQL Server.

## Estructura del Proyecto

```
/order-management-backend
├─ /services                    # Microservicios independientes
│   ├─ OrderService/            # Gestión de órdenes
│   ├─ ProductService/          # Gestión de productos
│   ├─ CustomerService/         # Gestión de clientes
│   └─ LoggingService/          # Servicio de logs centralizado
├─ /shared                      # Librerías compartidas
│   ├─ OrderManagement.Shared.Events/     # Eventos compartidos
│   ├─ OrderManagement.Shared.Common/     # Utilidades comunes
│   └─ OrderManagement.Shared.Security/   # Configuración JWT
├─ /tests                       # Pruebas de integración
├─ /infra                       # Infraestructura
│   ├─ docker/                  # Dockerfiles y docker-compose
│   └─ scripts/                 # Scripts de configuración
└─ README.md
```

## Microservicios

### 1. OrderService
- **Puerto**: 5001
- **Base de datos**: OrderManagement_Orders
- **Responsabilidades**: Crear, actualizar, consultar órdenes

### 2. ProductService
- **Puerto**: 5002
- **Base de datos**: OrderManagement_Products
- **Responsabilidades**: Gestión de catálogo de productos, inventario

### 3. CustomerService
- **Puerto**: 5003
- **Base de datos**: OrderManagement_Customers
- **Responsabilidades**: Gestión de clientes, autenticación

### 4. LoggingService
- **Puerto**: 5004
- **Base de datos**: OrderManagement_Logs
- **Responsabilidades**: Centralización de logs del sistema

## Comunicación entre Servicios

### HTTP (Síncrono)
- Cliente → APIs de microservicios
- Consultas entre servicios (cuando se requiere respuesta inmediata)

### RabbitMQ (Asíncrono)
- Eventos de dominio entre microservicios
- Notificaciones y actualizaciones de estado

### Eventos Principales
- `OrderCreated`: Cuando se crea una nueva orden
- `ProductStockUpdated`: Cuando se actualiza el inventario
- `CustomerRegistered`: Cuando se registra un nuevo cliente

## Patrones Implementados

- **Repository Pattern**: Abstracción de acceso a datos
- **Unit of Work**: Gestión de transacciones
- **CQRS**: Separación de comandos y consultas
- **Domain Events**: Comunicación entre contextos
- **Dependency Injection**: Inversión de control

## Tecnologías

- **.NET 8**: Framework principal
- **Entity Framework Core**: ORM
- **SQL Server**: Base de datos
- **RabbitMQ**: Message broker
- **Serilog**: Logging
- **xUnit**: Testing
- **Docker**: Containerización
- **JWT**: Autenticación

## Configuración del Entorno de Desarrollo

### Prerrequisitos
- .NET 8 SDK
- SQL Server (LocalDB o instancia completa)
- RabbitMQ
- Docker Desktop (opcional)

### Configuración Rápida

1. **Clonar el repositorio**
```bash
git clone <repository-url>
cd order-management-backend
```

2. **Configurar bases de datos**
```bash
# Ejecutar script de configuración
./infra/scripts/setup-databases.ps1
```

3. **Configurar RabbitMQ**
```bash
# Con Docker
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# O instalar localmente desde https://www.rabbitmq.com/download.html
```

4. **Ejecutar microservicios**
```bash
# Desde la raíz del proyecto
dotnet run --project services/OrderService/src/Web
dotnet run --project services/ProductService/src/Web
dotnet run --project services/CustomerService/src/Web
dotnet run --project services/LoggingService/src/Web
```

### Configuración con Docker

```bash
# Desde la raíz del proyecto
docker-compose -f infra/docker/docker-compose.yml up -d
```

## Testing

### Ejecutar Tests Unitarios
```bash
# Todos los tests
dotnet test

# Por servicio
dotnet test services/OrderService/tests/
dotnet test services/ProductService/tests/
dotnet test services/CustomerService/tests/
dotnet test services/LoggingService/tests/
```

### Tests de Integración
```bash
dotnet test tests/Integration.Tests/
```

## APIs

### OrderService (Puerto 5001)
- `GET /api/orders` - Listar órdenes
- `GET /api/orders/{id}` - Obtener orden por ID
- `POST /api/orders` - Crear nueva orden
- `PUT /api/orders/{id}` - Actualizar orden
- `DELETE /api/orders/{id}` - Eliminar orden

### ProductService (Puerto 5002)
- `GET /api/products` - Listar productos
- `GET /api/products/{id}` - Obtener producto por ID
- `POST /api/products` - Crear nuevo producto
- `PUT /api/products/{id}` - Actualizar producto
- `PUT /api/products/{id}/stock` - Actualizar stock

### CustomerService (Puerto 5003)
- `GET /api/customers` - Listar clientes
- `GET /api/customers/{id}` - Obtener cliente por ID
- `POST /api/customers` - Registrar nuevo cliente
- `POST /api/auth/login` - Autenticación
- `POST /api/auth/register` - Registro

### LoggingService (Puerto 5004)
- `GET /api/logs` - Consultar logs
- `GET /api/logs/search` - Búsqueda de logs
- `POST /api/logs` - Crear log

## Configuración

### Variables de Entorno

Cada microservicio utiliza las siguientes variables:

```env
# Base de datos
ConnectionStrings__DefaultConnection=Server=localhost;Database=OrderManagement_{ServiceName};Trusted_Connection=true;

# RabbitMQ
RabbitMQ__HostName=localhost
RabbitMQ__UserName=guest
RabbitMQ__Password=guest

# JWT
Jwt__Key=your-secret-key-here
Jwt__Issuer=OrderManagementSystem
Jwt__Audience=OrderManagementSystem
Jwt__ExpireMinutes=60

# Serilog
Serilog__MinimumLevel=Information
```

## Flujo de una Orden

1. **Cliente crea orden** → OrderService
2. **OrderService publica evento** → `OrderCreated`
3. **ProductService escucha evento** → Valida stock y actualiza inventario
4. **ProductService publica evento** → `ProductStockUpdated`
5. **LoggingService registra** → Todas las acciones del flujo

## Contribución

1. Fork el proyecto
2. Crear feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la branch (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

## Convenciones de Código

- Usar PascalCase para clases, métodos y propiedades
- Usar camelCase para variables locales y parámetros
- Prefijo `I` para interfaces
- Sufijo `Service` para servicios
- Sufijo `Repository` para repositorios
- Sufijo `Command` para comandos CQRS
- Sufijo `Query` para consultas CQRS

## Arquitectura del Sistema

Para más información detallada sobre la arquitectura, consulta [ARCHITECTURE.md](docs/ARCHITECTURE.md)

### Diagrama de Alto Nivel
```
Cliente/Aplicación ──HTTP/JWT──► API Gateway (Opcional)
                                      │
                    ┌─────────────────┼─────────────────┐
                    │                 │                 │
            OrderService      ProductService    CustomerService    LoggingService
            (Port 5001)       (Port 5002)      (Port 5003)       (Port 5004)
                    │                 │                 │              │
                    └─────────────────┼─────────────────┼──────────────┘
                                     │                 │
                              RabbitMQ (Events)        │
                              (Port 5672)              │
                                                       │
            ┌──────────────────────────────────────────┼─────────────────┐
            │                 │                │       │                 │
    OrderManagement_   OrderManagement_  OrderManagement_ OrderManagement_
        Orders           Products         Customers         Logs
    (SQL Server)      (SQL Server)     (SQL Server)     (SQL Server)
```

## Guía de Desarrollo

Para desarrolladores nuevos en el proyecto, consulta la [Guía de Desarrollo](docs/DEVELOPMENT_GUIDE.md) que incluye:
- Configuración del entorno
- Convenciones de código
- Flujo de desarrollo
- Testing
- Troubleshooting

## Próximos Pasos Recomendados

### 1. Configuración Inicial
```powershell
# 1. Ejecutar configuración de bases de datos
.\infra\scripts\setup-databases.ps1

# 2. Ejecutar migraciones
.\infra\scripts\run-migrations.ps1

# 3. Iniciar todos los servicios
.\infra\scripts\start-services.ps1
```

### 2. Verificación del Sistema
- Acceder a Swagger UI de cada servicio
- Verificar RabbitMQ Management UI: http://localhost:15672
- Probar endpoints básicos de cada microservicio

### 3. Desarrollo de Cliente
Desarrollar una aplicación cliente (web, móvil, desktop) que consuma las APIs REST de los microservicios.

## Mejoras Futuras

### Infraestructura
- [ ] Implementar API Gateway (Ocelot/YARP)
- [ ] Agregar cache distribuido (Redis)
- [ ] Implementar health checks
- [ ] Configurar CI/CD pipelines
- [ ] Agregar monitoreo con Application Insights

### Funcionalidades
- [ ] Sistema de notificaciones
- [ ] Reportes y analytics
- [ ] Integración con sistemas de pago
- [ ] Gestión de inventario avanzada
- [ ] Sistema de devoluciones

### Seguridad
- [ ] Implementar refresh tokens
- [ ] Rate limiting
- [ ] Audit logging completo
- [ ] Encryption at rest

## Licencia

[MIT License](LICENSE)
