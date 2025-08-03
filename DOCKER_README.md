# 🚀 Order Management System - Dockerizado Completo

## Descripción
Sistema de gestión de órdenes completamente dockerizado que se puede ejecutar con **un solo comando**. Incluye backend con 4 microservicios (.NET 8), frontend (React + TypeScript), bases de datos (SQL Server) y message broker (RabbitMQ).

## 📋 Prerrequisitos

### ✅ Software Requerido
- **Docker Desktop** (versión 20.10+)
- **Git** para clonar el repositorio
- **10 GB** de espacio libre en disco
- **8 GB RAM** mínimo recomendado

### 🔧 Verificación Previa
```bash
# Verificar Docker
docker --version
docker-compose --version

# Verificar que Docker esté corriendo
docker info
```

## 🚀 Inicio Rápido - UN SOLO COMANDO

### Opción 1: PowerShell (Windows) - RECOMENDADO
```powershell
# Inicio simple
.\start-system.ps1

# Con limpieza previa
.\start-system.ps1 -Clean

# Con Nginx reverse proxy
.\start-system.ps1 -WithNginx

# Limpieza + Nginx
.\start-system.ps1 -Clean -WithNginx
```

### Opción 2: Bash (Linux/Mac/WSL)
```bash
# Hacer ejecutable el script
chmod +x start-system.sh

# Inicio simple
./start-system.sh

# Con limpieza previa
./start-system.sh --clean

# Con Nginx reverse proxy
./start-system.sh --clean --with-nginx
```

### Opción 3: Docker Compose Directo
```bash
# Versión completa con todas las características
docker-compose up -d

# Versión simplificada para desarrollo
docker-compose -f docker-compose.dev.yml up -d
```

## 🌐 URLs Disponibles (Después del Inicio)

### 📱 Aplicación Principal
- **Frontend React**: http://localhost:3000

### 🔧 APIs Backend (Swagger)
- **Customer Service**: http://localhost:5003/swagger
- **Product Service**: http://localhost:5002/swagger
- **Order Service**: http://localhost:5001/swagger
- **Logging Service**: http://localhost:5004/swagger

### 🛠 Herramientas de Infraestructura
- **RabbitMQ Management**: http://localhost:15672
  - Usuario: `admin`
  - Contraseña: `OrderManagement2024!`

### 🔄 Con Nginx (si se usa --with-nginx)
- **Aplicación completa**: http://localhost
- **API unificada**: http://localhost/api/*
- **Swagger UIs**: http://localhost/swagger-[service]

## 📊 Componentes del Sistema

### 🏗 Microservicios Backend
| Servicio | Puerto | Base de Datos | Descripción |
|----------|--------|---------------|-------------|
| CustomerService | 5003 | OrderManagement_Customers | Autenticación y gestión de clientes |
| ProductService | 5002 | OrderManagement_Products | Catálogo de productos e inventario |
| OrderService | 5001 | OrderManagement_Orders | Gestión del ciclo de vida de órdenes |
| LoggingService | 5004 | OrderManagement_Logs | Centralización de logs y auditoría |

### 🖥 Frontend
| Componente | Puerto | Tecnología | Descripción |
|------------|--------|------------|-------------|
| React App | 3000 | React + TypeScript + Vite | Interfaz de usuario principal |

### 🔧 Infraestructura
| Servicio | Puerto(s) | Descripción |
|----------|-----------|-------------|
| SQL Server | 1433 | Base de datos principal |
| RabbitMQ | 5672, 15672 | Message broker + Management UI |
| Nginx | 80, 443 | Reverse proxy (opcional) |

## 🔍 Comandos Útiles

### 📋 Estado y Monitoreo
```bash
# Ver estado de todos los servicios
docker-compose ps

# Ver logs en tiempo real
docker-compose logs -f

# Ver logs de un servicio específico
docker-compose logs -f order-service

# Ver uso de recursos
docker stats
```

### 🛠 Gestión de Servicios
```bash
# Parar todos los servicios (mantiene datos)
docker-compose stop

# Iniciar servicios previamente parados
docker-compose start

# Reiniciar un servicio específico
docker-compose restart order-service

# Parar y eliminar containers (mantiene volúmenes)
docker-compose down

# Parar y eliminar TODO (incluye volúmenes de base de datos)
docker-compose down -v --remove-orphans
```

### 🔄 Reconstrucción y Actualizaciones
```bash
# Reconstruir imágenes sin cache
docker-compose build --no-cache

# Reconstruir y levantar
docker-compose up -d --build

# Reconstruir un servicio específico
docker-compose build --no-cache order-service
docker-compose up -d order-service
```

## 🔧 Configuración Avanzada

### 🌍 Variables de Entorno
Puedes personalizar la configuración creando un archivo `.env`:

```env
# Configuración de Base de Datos
SA_PASSWORD=TuPasswordSegura2024!

# Configuración de RabbitMQ
RABBITMQ_USER=tu_usuario
RABBITMQ_PASSWORD=tu_password

# Configuración JWT
JWT_SECRET_KEY=tu-clave-super-secreta-de-al-menos-32-caracteres

# Puertos (si necesitas cambiarlos)
FRONTEND_PORT=3000
ORDER_SERVICE_PORT=5001
PRODUCT_SERVICE_PORT=5002
CUSTOMER_SERVICE_PORT=5003
LOGGING_SERVICE_PORT=5004
```

### 🔄 Configuraciones por Entorno
El sistema soporta diferentes configuraciones:

```bash
# Desarrollo (por defecto)
docker-compose -f docker-compose.dev.yml up -d

# Producción completa
docker-compose up -d

# Solo infraestructura (para desarrollo local de servicios)
docker-compose up -d sqlserver rabbitmq
```

## 🧪 Testing y Desarrollo

### 🔍 Health Checks
Todos los servicios incluyen health checks automáticos:
```bash
# Verificar salud de un servicio específico
curl http://localhost:5001/health

# Ver estado de health checks
docker-compose ps
```

### 📊 Logs Estructurados
Los logs están centralizados y estructurados:
```bash
# Ver logs del sistema completo
docker-compose logs -f

# Filtrar logs por nivel
docker-compose logs -f | grep "ERROR"

# Logs en formato JSON para procesamiento
docker-compose logs --no-color -f logging-service
```

### 🛡 Seguridad
- JWT tokens con claves seguras
- Comunicación entre servicios autenticada
- SQL Server con usuario no-root
- Nginx con headers de seguridad
- Containers con usuarios no-privilegiados

## 🔄 Flujo de Trabajo de Desarrollo

### 1. Desarrollo Local Híbrido
```bash
# Levantar solo infraestructura
docker-compose up -d sqlserver rabbitmq

# Desarrollar servicios localmente
dotnet run --project order-management-backend/services/OrderService/src/Api
```

### 2. Desarrollo Completamente Dockerizado
```bash
# Todo en containers con hot-reload
docker-compose -f docker-compose.dev.yml up -d

# Ver cambios en tiempo real
docker-compose logs -f frontend
```

### 3. Testing de Integración
```bash
# Levantar todo el sistema
.\start-system.ps1

# Ejecutar tests
npm test # Frontend
dotnet test # Backend
```

## 🚨 Troubleshooting

### ❌ Problemas Comunes

#### Docker no está corriendo
```bash
# Windows: Iniciar Docker Desktop
# Linux: sudo systemctl start docker
# Mac: Iniciar Docker Desktop
```

#### Puerto en uso
```bash
# Ver qué proceso usa el puerto
netstat -ano | findstr :5001  # Windows
lsof -i :5001                 # Linux/Mac

# Cambiar puertos en docker-compose.yml
ports:
  - "5011:5001"  # Mapear puerto 5011 externo al 5001 interno
```

#### SQL Server no se conecta
```bash
# Verificar logs de SQL Server
docker-compose logs sqlserver

# Conectar manualmente para testing
docker exec -it oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "OrderManagement2024!"
```

#### RabbitMQ no responde
```bash
# Reiniciar RabbitMQ
docker-compose restart rabbitmq

# Verificar logs
docker-compose logs rabbitmq

# Acceder al management UI
# http://localhost:15672 (admin/OrderManagement2024!)
```

#### Frontend no carga
```bash
# Verificar logs del frontend
docker-compose logs frontend

# Verificar que los servicios backend estén corriendo
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

### 🧹 Limpieza Completa
Si algo falla completamente:
```bash
# PowerShell
.\start-system.ps1 -Clean

# O manualmente
docker-compose down -v --remove-orphans
docker system prune -a -f --volumes
docker volume prune -f
```

## 📈 Escalabilidad y Producción

### 🔄 Scaling Horizontal
```bash
# Escalar servicios específicos
docker-compose up -d --scale order-service=3 --scale product-service=2
```

### 🌩 Deploy en Cloud
El sistema está preparado para deploy en:
- **Docker Swarm**
- **Kubernetes**
- **Azure Container Apps**
- **AWS ECS**
- **Google Cloud Run**

### 📊 Monitoreo
Para producción, considera agregar:
- **Prometheus + Grafana** para métricas
- **ELK Stack** para logs centralizados
- **Jaeger** para distributed tracing

## 🤝 Contribución
1. Fork el proyecto
2. Crea una rama para tu feature
3. Usa el entorno Docker para desarrollo
4. Ejecuta todos los tests
5. Submit un Pull Request

## 📄 Licencia
MIT License - Ver archivo LICENSE

## 📞 Soporte
- **Issues**: GitHub Issues
- **Documentación**: Ver `/docs` en cada servicio
- **API Docs**: Swagger UIs en cada servicio
