# 🐳 Docker Deployment Guide

## Overview
Your Order Management System is completely dockerized and ready to use with a single command! This comprehensive guide covers everything from quick start to advanced configuration and troubleshooting.

## 🚀 Quick Start

To start the entire system with one command:

```bash
bash scripts/docker/start-oms.sh
```

This command automatically:
- 🧹 Cleans previous containers
- 🔨 Builds all Docker images
- 🏗️ Starts infrastructure services (SQL Server, RabbitMQ)
- 🚀 Starts all backend microservices
- 🌐 Starts the React frontend
- 📊 Verifies all services are running

## 🌟 Available Endpoints

Once the system is started, you can access:

### 📱 FRONTEND
- **Web Application**: [http://localhost:3000](http://localhost:3000)

### 🔧 BACKEND SERVICES
- **Customer Service**: [http://localhost:5003](http://localhost:5003) | [Swagger](http://localhost:5003/swagger)
- **Product Service**: [http://localhost:5002](http://localhost:5002) | [Swagger](http://localhost:5002/swagger)
- **Order Service**: [http://localhost:5001](http://localhost:5001) | [Swagger](http://localhost:5001/swagger)
- **Logging Service**: [http://localhost:5004](http://localhost:5004) | [Swagger](http://localhost:5004/swagger)

### 🏗️ INFRASTRUCTURE
- **RabbitMQ Management**: [http://localhost:15672](http://localhost:15672) (admin/OrderManagement2024!)
- **SQL Server**: localhost:1433 (sa/OrderManagement2024!)

## 📋 Prerequisites

### ✅ Required Software
- **Docker Desktop** (version 20.10+)
- **Git** for cloning the repository
- **10 GB** free disk space
- **8 GB RAM** minimum recommended

### 🔧 Pre-deployment Verification
```bash
# Verify Docker
docker --version
docker-compose --version

# Verify Docker is running
docker info
```

## 🚀 Deployment Options

### Option 1: PowerShell (Windows) - RECOMMENDED
```powershell
# Simple start
.\start-system.ps1

# With cleanup
.\start-system.ps1 -Clean

# With Nginx reverse proxy
.\start-system.ps1 -WithNginx

# Cleanup + Nginx
.\start-system.ps1 -Clean -WithNginx
```

### Option 2: Bash (Linux/Mac/WSL)
```bash
# Make script executable
chmod +x start-system.sh

# Simple start
./start-system.sh

# With cleanup
./start-system.sh --clean

# With Nginx reverse proxy
./start-system.sh --clean --with-nginx
```

### Option 3: Direct Docker Compose
```bash
# Complete production setup
docker-compose up -d

# Development mode
docker-compose -f docker-compose.dev.yml up -d
```

## 🌐 URLs with Nginx (Optional)

When using `--with-nginx` flag:
- **Main Application**: http://localhost
- **Unified API**: http://localhost/api/*
- **Swagger UIs**: http://localhost/swagger-[service]

## 📊 System Components

### 🏗 Backend Microservices
| Service | Port | Database | Description |
|---------|------|----------|-------------|
| CustomerService | 5003 | OrderManagement_Customers | Authentication and customer management |
| ProductService | 5002 | OrderManagement_Products | Product catalog and inventory |
| OrderService | 5001 | OrderManagement_Orders | Order lifecycle management |
| LoggingService | 5004 | OrderManagement_Logs | Centralized logging and auditing |

### 🖥 Frontend
| Component | Port | Technology | Description |
|-----------|------|------------|-------------|
| React App | 3000 | React + TypeScript + Vite | Main user interface |

### 🔧 Infrastructure
| Service | Port(s) | Description |
|---------|---------|-------------|
| SQL Server | 1433 | Main database |
| RabbitMQ | 5672, 15672 | Message broker + Management UI |
| Nginx | 80, 443 | Reverse proxy (optional) |

## 🛠️ Useful Commands

### 📋 Status and Monitoring
```bash
# View all services status
docker-compose ps

# View real-time logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f order-service

# View resource usage
docker stats
```

### 🛠 Service Management
```bash
# Stop all services (preserve data)
docker-compose stop

# Start previously stopped services
docker-compose start

# Restart specific service
docker-compose restart order-service

# Stop and remove containers (preserve volumes)
docker-compose down

# Stop and remove everything (including database volumes)
docker-compose down -v --remove-orphans
```

### 🔄 Rebuilding and Updates
```bash
# Rebuild images without cache
docker-compose build --no-cache

# Rebuild and start
docker-compose up -d --build

# Rebuild specific service
docker-compose build --no-cache order-service
docker-compose up -d order-service
```

## 🔧 Advanced Configuration

### 🌍 Environment Variables
Create a `.env` file to customize configuration:

```env
# Database Configuration
SA_PASSWORD=YourSecurePassword2024!

# RabbitMQ Configuration
RABBITMQ_USER=your_user
RABBITMQ_PASSWORD=your_password

# JWT Configuration
JWT_SECRET_KEY=your-super-secret-key-at-least-32-characters

# Port Configuration (if needed)
FRONTEND_PORT=3000
ORDER_SERVICE_PORT=5001
PRODUCT_SERVICE_PORT=5002
CUSTOMER_SERVICE_PORT=5003
LOGGING_SERVICE_PORT=5004
```

### 🔄 Environment-Specific Configurations
```bash
# Development environment
docker-compose -f docker-compose.dev.yml up -d

# Production environment
docker-compose up -d

# Infrastructure only (for local service development)
docker-compose up -d sqlserver rabbitmq
```

## 🧪 Testing and Development

### 🔍 Health Checks
All services include automatic health checks:
```bash
# Check specific service health
curl http://localhost:5001/health

# View health check status
docker-compose ps
```

### 📊 Structured Logging
Logs are centralized and structured:
```bash
# Complete system logs
docker-compose logs -f

# Filter logs by level
docker-compose logs -f | grep "ERROR"

# JSON formatted logs for processing
docker-compose logs --no-color -f logging-service
```

### 🛡 Security Features
- JWT tokens with secure keys
- Inter-service authenticated communication
- Non-root SQL Server user
- Security headers with Nginx
- Non-privileged container users

## 🔄 Development Workflow

### 1. Hybrid Local Development
```bash
# Start only infrastructure
docker-compose up -d sqlserver rabbitmq

# Develop services locally
dotnet run --project order-management-backend/services/OrderService/src/Api
```

### 2. Fully Dockerized Development
```bash
# Everything in containers with hot-reload
docker-compose -f docker-compose.dev.yml up -d

# Watch changes in real-time
docker-compose logs -f frontend
```

### 3. Integration Testing
```bash
# Start complete system
.\start-system.ps1

# Run tests
npm test # Frontend
dotnet test # Backend
```

## 🚨 Troubleshooting

### ❌ Common Issues

#### Docker Not Running
```bash
# Windows: Start Docker Desktop
# Linux: sudo systemctl start docker
# Mac: Start Docker Desktop
```

#### Port Conflicts
```bash
# Find process using port
netstat -ano | findstr :5001  # Windows
lsof -i :5001                 # Linux/Mac

# Change ports in docker-compose.yml
ports:
  - "5011:5001"  # Map external port 5011 to internal 5001
```

#### SQL Server Connection Issues
```bash
# Check SQL Server logs
docker-compose logs sqlserver

# Connect manually for testing
docker exec -it oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "OrderManagement2024!"
```

#### RabbitMQ Not Responding
```bash
# Restart RabbitMQ
docker-compose restart rabbitmq

# Check logs
docker-compose logs rabbitmq

# Verify management UI
# http://localhost:15672 (admin/OrderManagement2024!)
```

#### Frontend Not Loading
```bash
# Check frontend logs
docker-compose logs frontend

# Verify backend services are running
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
```

### 🧹 Complete System Reset
If everything fails:
```bash
# PowerShell
.\start-system.ps1 -Clean

# Or manually
docker-compose down -v --remove-orphans
docker system prune -a -f --volumes
docker volume prune -f
```

## 📈 Production and Scaling

### 🔄 Horizontal Scaling
```bash
# Scale specific services
docker-compose up -d --scale order-service=3 --scale product-service=2
```

### 🌩 Cloud Deployment
The system is ready for deployment on:
- **Docker Swarm**
- **Kubernetes**
- **Azure Container Apps**
- **AWS ECS**
- **Google Cloud Run**

### 📊 Monitoring for Production
Consider adding:
- **Prometheus + Grafana** for metrics
- **ELK Stack** for centralized logging
- **Jaeger** for distributed tracing

## 📁 Project Structure

```
order-management-system/
├── scripts/
│   └── docker/
│       └── start-oms.sh               # ⭐ Main command (SINGLE COMMAND)
│       └── test-integration.sh        # 🧪 Testing script
├── docker-compose.yml                 # 🐳 Complete Docker configuration
├── docker-compose.dev.yml             # 🔧 Development configuration
├── DOCKER_DEPLOYMENT.md               # 📚 This documentation
├── config/                            # ⚙️ Configurations (RabbitMQ, Nginx)
├── order-management-backend/          # 🔧 .NET Microservices
│   ├── services/
│   │   ├── CustomerService/
│   │   ├── ProductService/
│   │   ├── OrderService/
│   │   └── LoggingService/
│   └── shared/
└── order-management-frontend/         # 🌐 React Application
```

## 🎯 What We've Achieved

✅ **Single command** to start the entire system  
✅ **Complete dockerization** of all components  
✅ **Automatic orchestration** with docker-compose  
✅ **Health checks** for monitoring  
✅ **Automated testing** scripts  
✅ **Optimized configuration** for development and production  

## 💡 Important Notes

- Ensure Docker Desktop is running
- Ports 3000, 5001-5004, 1433, 5672, and 15672 must be available
- First execution may take longer (image building)
- Subsequent runs will be much faster (cached images)

## 🤝 Contributing

1. Fork the project
2. Create a feature branch
3. Use Docker environment for development
4. Run all tests
5. Submit a Pull Request

## 📄 License

MIT License - See LICENSE file

## 📞 Support

- **Issues**: GitHub Issues
- **Documentation**: See `/docs` in each service
- **API Docs**: Swagger UIs in each service

---

**🎉 Your Order Management System is production-ready with Docker!**