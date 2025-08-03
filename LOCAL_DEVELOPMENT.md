# 🏠 Local Development Guide

This guide explains how to run the Order Management System locally for development, without using Docker.

## 📋 Prerequisites

### System Requirements
- **Windows 10/11** with WSL or Git Bash
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** and **npm** - [Download here](https://nodejs.org/)
- **SQL Server** (Local DB, Express, or Developer Edition)
- **RabbitMQ** (can run in Docker)

### Verify Installations
```bash
# Verify .NET
dotnet --version

# Verify Node.js
node --version
npm --version

# Verify SQL Server
sqlcmd -S localhost -Q "SELECT GETDATE()"
```

## 🚀 Quick Start

### 1. Start External Infrastructure

**SQL Server** (if not installed locally):
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

### 2. Start the Complete System
```bash
# Start all services
bash scripts/local/start-local.sh
```

### 3. Verify the System
```bash
# Complete verification
bash scripts/local/check-local.sh

# Quick verification
bash scripts/local/check-local.sh --quick
```

### 4. Stop the System
```bash
# Stop services gracefully
bash scripts/local/stop-local.sh

# Force stop if there are issues
bash scripts/local/stop-local.sh --force
```

## 🔧 Manual Configuration

### Backend Services

Each service can be started individually:

```bash
# Customer Service (Port 5003)
cd order-management-backend/services/CustomerService/src/Api
dotnet run --urls="http://localhost:5003"

# Product Service (Port 5002)
cd order-management-backend/services/ProductService/src/Api
dotnet run --urls="http://localhost:5002"

# Logging Service (Port 5004)
cd order-management-backend/services/LoggingService/src/Api
dotnet run --urls="http://localhost:5004"

# Order Service (Port 5001) - Start last
cd order-management-backend/services/OrderService/src/Api
dotnet run --urls="http://localhost:5001"
```

### Frontend

```bash
cd order-management-frontend

# Install dependencies
npm install

# Configure environment variables for local development
cp .env.example .env.local

# Start development server
npm run dev
```

## 🌐 Development URLs

| Service | URL | Swagger |
|---------|-----|---------|
| **Frontend** | http://localhost:3000 | - |
| **Order Service** | http://localhost:5001 | http://localhost:5001/swagger |
| **Product Service** | http://localhost:5002 | http://localhost:5002/swagger |
| **Customer Service** | http://localhost:5003 | http://localhost:5003/swagger |
| **Logging Service** | http://localhost:5004 | http://localhost:5004/swagger |
| **RabbitMQ Management** | http://localhost:15672 | guest/guest |

## 📁 File Structure

```
order-management-system/
├── start-local.sh          # 🚀 Start everything locally
├── stop-local.sh           # 🛑 Stop local services
├── check-local.sh          # 🔍 Verify system status
├── logs/                   # 📋 Local service logs
│   ├── customerservice.log
│   ├── productservice.log
│   ├── orderservice.log
│   ├── loggingservice.log
│   └── frontend.log
└── order-management-frontend/
    └── .env.example        # 🔧 Example configuration
```

## 🛠️ Useful Commands

### View Real-time Logs
```bash
# View logs for a specific service
tail -f logs/customerservice.log
tail -f logs/productservice.log
tail -f logs/orderservice.log
tail -f logs/loggingservice.log
tail -f logs/frontend.log

# View all logs
tail -f logs/*.log
```

### Restart Services
```bash
# Restart entire system
bash scripts/local/stop-local.sh && bash scripts/local/start-local.sh

# Restart specific service (example: Customer Service)
# 1. Find PID
cat logs/customerservice.pid

# 2. Stop service
kill <PID>

# 3. Restart
cd order-management-backend/services/CustomerService/src/Api
dotnet run --urls="http://localhost:5003" > ../../../../logs/customerservice.log 2>&1 &
```

### Testing
```bash
# Frontend tests
cd order-management-frontend
npm test

# Backend tests (example)
cd order-management-backend/services/CustomerService
dotnet test
```

## ⚡ Rapid Development

### Hot Reload
- **Frontend**: Vite provides automatic hot reload
- **Backend**: Changes require service restart

### Debugging
- **Frontend**: Use browser DevTools
- **Backend**: Attach VS Code debugger or use `dotnet run --launch-profile Debug`

### Database
```bash
# Connect to SQL Server
sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# View databases
SELECT name FROM sys.databases;

# Use specific database
USE OrderManagement_Customers;
```

## 🐛 Troubleshooting

### Common Issues

**Port already in use:**
```bash
# Find process using the port
netstat -ano | findstr :5001

# Terminate process
taskkill /PID <PID> /F
```

**SQL Server not connecting:**
```bash
# Check if SQL Server is running
sc query MSSQLSERVER

# Start SQL Server
net start MSSQLSERVER
```

**RabbitMQ not connecting:**
```bash
# Check Docker container
docker ps | grep rabbitmq

# Restart RabbitMQ
docker restart rabbitmq
```

**npm dependencies:**
```bash
# Clear cache and reinstall
cd order-management-frontend
rm -rf node_modules package-lock.json
npm install
```

### Error Logs
Logs are saved in the `logs/` directory and contain detailed error information.

### Status Verification
```bash
# Quick verification of all services
bash scripts/local/check-local.sh --quick

# Complete verification with API tests
bash scripts/local/check-local.sh
```

## 🔄 Development Workflow

1. **Start infrastructure**: SQL Server and RabbitMQ  
2. **Execute**: `bash scripts/local/start-local.sh`  
3. **Develop**: Make code changes  
4. **Test**: Use `bash scripts/local/check-local.sh` to verify  
5. **Stop**: `bash scripts/local/stop-local.sh` when finished  

## 📦 Docker vs Local

| Aspect | Docker | Local |
|--------|--------|-------|
| **Command** | `bash scripts/docker/start-oms.sh` | `bash scripts/local/start-local.sh` |
| **Speed** | Slower to start | Faster |
| **Debugging** | Limited | Complete |
| **Hot Reload** | Frontend only | Frontend only |
| **Isolation** | Complete | Partial |
| **Resources** | More memory/CPU | Fewer resources |

## 🔧 Environment Configuration

### Backend Environment Variables
```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=OrderManagement_{Service};Trusted_Connection=true;

# RabbitMQ
RabbitMQ__HostName=localhost
RabbitMQ__UserName=guest
RabbitMQ__Password=guest

# JWT
Jwt__Key=your-secret-key
Jwt__Issuer=OrderManagementSystem
Jwt__Audience=OrderManagementSystem
```

### Frontend Environment Variables (.env.local)
```env
# API URLs
VITE_ORDER_SERVICE_URL=http://localhost:5001
VITE_PRODUCT_SERVICE_URL=http://localhost:5002
VITE_CUSTOMER_SERVICE_URL=http://localhost:5003
VITE_LOGGING_SERVICE_URL=http://localhost:5004

# Development mode
VITE_DOCKER_MODE=false
```

## 🧪 Testing and Quality Assurance

### Running Tests
```bash
# Frontend tests
cd order-management-frontend
npm test                    # Unit tests
npm run test:e2e           # End-to-end tests
npm run test:coverage      # Coverage report

# Backend tests
cd order-management-backend
dotnet test                # All service tests
dotnet test --logger:trx   # Generate test reports
dotnet test --collect:"XPlat Code Coverage"  # Coverage
```

### Code Quality
```bash
# Frontend linting and formatting
cd order-management-frontend
npm run lint               # ESLint
npm run format             # Prettier

# Backend code analysis
cd order-management-backend
dotnet build --configuration Release  # Build check
```

## 🚀 IDE Setup

### Visual Studio Code
```json
// .vscode/settings.json
{
  "dotnet.defaultSolution": "order-management-backend/OrderManagement.sln",
  "typescript.preferences.quoteStyle": "single",
  "editor.formatOnSave": true
}
```

### Recommended Extensions
- **C# Dev Kit** - .NET development
- **ES7+ React/Redux/React-Native snippets** - React development
- **REST Client** - API testing
- **GitLens** - Git integration

## 📊 Performance Monitoring

### Local Monitoring
```bash
# Monitor service performance
dotnet-counters monitor --process-id <service-pid>

# Monitor frontend build
npm run build:analyze

# Database performance
sqlcmd -S localhost -Q "SELECT * FROM sys.dm_exec_requests"
```

## 🚀 Next Steps

- Configure your preferred IDE (VS Code, Visual Studio)  
- Explore the APIs using Swagger  
- Review existing tests  
- Start developing!  

## 🔄 Transitioning Between Modes

### From Local to Docker
```bash
# Stop local services
bash scripts/local/stop-local.sh

# Start Docker environment
bash scripts/docker/start-oms.sh
```

### From Docker to Local
```bash
# Stop Docker services
docker-compose down

# Start local environment
bash scripts/local/start-local.sh
```

## 📞 Support

**Issues?** Run `bash scripts/local/check-local.sh` for complete system diagnosis.

---

**🎉 Happy local development with your Order Management System!**