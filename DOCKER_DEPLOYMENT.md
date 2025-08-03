# Order Management System - Docker Deployment

¡Tu Order Management System está completamente dockerizado y listo para usar con un solo comando!

## 🚀 Inicio Rápido

Para iniciar todo el sistema con un solo comando:

```bash
bash start-oms.sh
```

Este comando automáticamente:
- 🧹 Limpia contenedores previos
- 🔨 Construye todas las imágenes Docker
- 🏗️ Inicia servicios de infraestructura (SQL Server, RabbitMQ)
- 🚀 Inicia todos los microservicios del backend
- 🌐 Inicia el frontend React
- 📊 Verifica que todos los servicios estén funcionando

## 🌟 Endpoints Disponibles

Una vez iniciado el sistema, podrás acceder a:

### 📱 FRONTEND
- **Aplicación Web**: [http://localhost:3000](http://localhost:3000)

### 🔧 BACKEND SERVICES
- **Customer Service**: [http://localhost:5003](http://localhost:5003)
- **Product Service**: [http://localhost:5002](http://localhost:5002)
- **Order Service**: [http://localhost:5001](http://localhost:5001)
- **Logging Service**: [http://localhost:5004](http://localhost:5004)

### 🏗️ INFRASTRUCTURE
- **RabbitMQ Management**: [http://localhost:15672](http://localhost:15672) (guest/guest)
- **SQL Server**: localhost:1433 (sa/YourPassword123!)

## 🛠️ Comandos Útiles

### Detener todo el sistema
```bash
docker-compose down
```

### Ver logs de un servicio específico
```bash
docker-compose logs -f [service-name]
```

Servicios disponibles:
- `customer-service`
- `product-service`
- `order-service`
- `logging-service`
- `frontend`
- `sqlserver`
- `rabbitmq`

### Ver estado de todos los contenedores
```bash
docker ps
```

### Reiniciar un servicio específico
```bash
docker-compose restart [service-name]
```

### Construir solo las imágenes (sin iniciar)
```bash
docker-compose build
```

## 🧪 Verificar que Todo Funciona

Ejecuta las pruebas de integración:

```bash
bash test-integration.sh
```

## 📁 Estructura del Proyecto

```
order-management-system/
├── start-oms.sh                       # ⭐ Comando principal (UN SOLO COMANDO)
├── test-integration.sh                # 🧪 Script de pruebas
├── docker-compose.yml                 # 🐳 Configuración completa de Docker
├── DOCKER_DEPLOYMENT.md               # 📚 Esta documentación
├── config/                            # ⚙️ Configuraciones (RabbitMQ)
├── order-management-backend/          # 🔧 Microservicios .NET
│   ├── services/
│   │   ├── CustomerService/
│   │   ├── ProductService/
│   │   ├── OrderService/
│   │   └── LoggingService/
│   └── shared/
└── order-management-frontend/         # 🌐 Aplicación React
```

## 🐳 Información Técnica

### Servicios Backend (.NET 8)
- **Puerto 5001**: OrderService
- **Puerto 5002**: ProductService  
- **Puerto 5003**: CustomerService
- **Puerto 5004**: LoggingService

### Frontend (React + TypeScript + Vite)
- **Puerto 3000**: Aplicación web

### Infraestructura
- **Puerto 1433**: SQL Server
- **Puerto 5672**: RabbitMQ (AMQP)
- **Puerto 15672**: RabbitMQ Management UI

### Health Checks
Todos los servicios incluyen health checks automáticos:
- `/health` endpoint para servicios .NET
- Verificación de conectividad para infraestructura

## 🎯 Lo que Logramos

✅ **Un solo comando** para iniciar todo el sistema  
✅ **Dockerización completa** de todos los componentes  
✅ **Orquestación automática** con docker-compose  
✅ **Health checks** para monitoreo  
✅ **Scripts de prueba** automatizados  
✅ **Configuración optimizada** para desarrollo y producción  

## 💡 Notas Importantes

- Asegúrate de que Docker Desktop esté corriendo
- Los puertos 3000, 5001-5004, 1433, 5672, y 15672 deben estar libres
- La primera ejecución puede tomar más tiempo (construcción de imágenes)
- Las ejecuciones posteriores serán mucho más rápidas (imágenes en caché)

---

**¡Tu Order Management System está listo para producción con Docker! 🎉**
