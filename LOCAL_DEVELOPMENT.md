# 🏠 Local Development Guide

Esta guía explica cómo ejecutar el Order Management System localmente para desarrollo, sin usar Docker.

## 📋 Prerrequisitos

### Requerimientos del Sistema
- **Windows 10/11** con WSL o Git Bash
- **.NET 8.0 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** y **npm** - [Descargar aquí](https://nodejs.org/)
- **SQL Server** (Local DB, Express, o Developer Edition)
- **RabbitMQ** (puede ejecutarse en Docker)

### Verificar Instalaciones
```bash
# Verificar .NET
dotnet --version

# Verificar Node.js
node --version
npm --version

# Verificar SQL Server
sqlcmd -S localhost -Q "SELECT GETDATE()"
```

## 🚀 Inicio Rápido

### 1. Iniciar Infraestructura Externa

**SQL Server** (si no está instalado localmente):
```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name sql-server \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

**RabbitMQ**:
```bash
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

### 2. Iniciar Todo el Sistema
```bash
# Hacer el script ejecutable
chmod +x start-local.sh

# Iniciar todos los servicios
bash start-local.sh
```

### 3. Verificar el Sistema
```bash
# Verificación completa
bash check-local.sh

# Verificación rápida
bash check-local.sh --quick
```

### 4. Detener el Sistema
```bash
# Detener servicios ordenadamente
bash stop-local.sh

# Detener forzadamente si hay problemas
bash stop-local.sh --force
```

## 🔧 Configuración Manual

### Backend Services

Cada servicio se puede iniciar individualmente:

```bash
# Customer Service (Puerto 5003)
cd order-management-backend/services/CustomerService/src/Api
dotnet run --urls="http://localhost:5003"

# Product Service (Puerto 5002)
cd order-management-backend/services/ProductService/src/Api
dotnet run --urls="http://localhost:5002"

# Logging Service (Puerto 5004)
cd order-management-backend/services/LoggingService/src/Api
dotnet run --urls="http://localhost:5004"

# Order Service (Puerto 5001) - Iniciar al final
cd order-management-backend/services/OrderService/src/Api
dotnet run --urls="http://localhost:5001"
```

### Frontend

```bash
cd order-management-frontend

# Instalar dependencias
npm install

# Configurar variables de entorno para local
cp .env.example .env.local

# Iniciar servidor de desarrollo
npm run dev
```

## 🌐 URLs de Desarrollo

| Servicio | URL | Swagger |
|----------|-----|---------|
| **Frontend** | http://localhost:3000 | - |
| **Order Service** | http://localhost:5001 | http://localhost:5001/swagger |
| **Product Service** | http://localhost:5002 | http://localhost:5002/swagger |
| **Customer Service** | http://localhost:5003 | http://localhost:5003/swagger |
| **Logging Service** | http://localhost:5004 | http://localhost:5004/swagger |
| **RabbitMQ Management** | http://localhost:15672 | guest/guest |

## 📁 Estructura de Archivos

```
order-management-system/
├── start-local.sh          # 🚀 Iniciar todo localmente
├── stop-local.sh           # 🛑 Detener servicios locales
├── check-local.sh          # 🔍 Verificar estado del sistema
├── logs/                   # 📋 Logs de servicios locales
│   ├── customerservice.log
│   ├── productservice.log
│   ├── orderservice.log
│   ├── loggingservice.log
│   └── frontend.log
└── order-management-frontend/
    └── .env.example        # 🔧 Configuración de ejemplo
```

## 🛠️ Comandos Útiles

### Ver Logs en Tiempo Real
```bash
# Ver logs de un servicio específico
tail -f logs/customerservice.log
tail -f logs/productservice.log
tail -f logs/orderservice.log
tail -f logs/loggingservice.log
tail -f logs/frontend.log

# Ver todos los logs
tail -f logs/*.log
```

### Reiniciar Servicios
```bash
# Reiniciar todo el sistema
bash stop-local.sh && bash start-local.sh

# Reiniciar solo un servicio (ejemplo: Customer Service)
# 1. Encontrar PID
cat logs/customerservice.pid

# 2. Detener servicio
kill <PID>

# 3. Reiniciar
cd order-management-backend/services/CustomerService/src/Api
dotnet run --urls="http://localhost:5003" > ../../../../logs/customerservice.log 2>&1 &
```

### Tests
```bash
# Frontend tests
cd order-management-frontend
npm test

# Backend tests (ejemplo)
cd order-management-backend/services/CustomerService
dotnet test
```

## ⚡ Desarrollo Rápido

### Hot Reload
- **Frontend**: Vite proporciona hot reload automático
- **Backend**: Los cambios requieren reiniciar el servicio

### Debugging
- **Frontend**: Usa las DevTools del navegador
- **Backend**: Adjunta el debugger de VS Code o usa `dotnet run --launch-profile Debug`

### Base de Datos
```bash
# Conectar a SQL Server
sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# Ver bases de datos
SELECT name FROM sys.databases;

# Usar base de datos específica
USE OrderManagement_Customers;
```

## 🐛 Troubleshooting

### Problemas Comunes

**Puerto ya en uso:**
```bash
# Encontrar proceso usando el puerto
netstat -ano | findstr :5001

# Terminar proceso
taskkill /PID <PID> /F
```

**SQL Server no conecta:**
```bash
# Verificar si SQL Server está corriendo
sc query MSSQLSERVER

# Iniciar SQL Server
net start MSSQLSERVER
```

**RabbitMQ no conecta:**
```bash
# Verificar Docker container
docker ps | grep rabbitmq

# Reiniciar RabbitMQ
docker restart rabbitmq
```

**Dependencias de npm:**
```bash
# Limpiar cache y reinstalar
cd order-management-frontend
rm -rf node_modules package-lock.json
npm install
```

### Logs de Error
Los logs se guardan en el directorio `logs/` y contienen información detallada de errores.

### Verificación de Estado
```bash
# Verificación rápida de todos los servicios
bash check-local.sh --quick

# Verificación completa con tests de API
bash check-local.sh
```

## 🔄 Workflow de Desarrollo

1. **Iniciar infraestructura**: SQL Server y RabbitMQ
2. **Ejecutar**: `bash start-local.sh`
3. **Desarrollar**: Hacer cambios en el código
4. **Probar**: Usar `bash check-local.sh` para verificar
5. **Detener**: `bash stop-local.sh` cuando termines

## 📦 Docker vs Local

| Aspecto | Docker | Local |
|---------|---------|--------|
| **Comando** | `bash start-oms.sh` | `bash start-local.sh` |
| **Velocidad** | Más lento para iniciar | Más rápido |
| **Debugging** | Limitado | Completo |
| **Hot Reload** | Frontend only | Frontend only |
| **Aislamiento** | Completo | Parcial |
| **Recursos** | Más memoria/CPU | Menos recursos |

## 🚀 Próximos Pasos

- Configura tu IDE preferido (VS Code, Visual Studio)
- Explora las APIs usando Swagger
- Revisa los tests existentes
- ¡Empieza a desarrollar!

---

**¿Problemas?** Ejecuta `bash check-local.sh` para diagnóstico completo.
