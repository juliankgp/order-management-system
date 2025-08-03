# 📋 Scripts de Desarrollo - Resumen

## 🎯 Scripts Disponibles

### 🐳 Deployment con Docker
- **`start-oms.sh`** - Inicia todo el sistema con Docker (modo producción)
- **`test-integration.sh`** - Prueba la integración completa del sistema Docker

### 🏠 Desarrollo Local  
- **`start-local.sh`** - Inicia todos los servicios localmente (modo desarrollo)
- **`stop-local.sh`** - Detiene todos los servicios locales ordenadamente
- **`check-local.sh`** - Verifica el estado de los servicios locales

## 🔄 Flujos de Trabajo Recomendados

### Para Desarrollo Activo (Recomendado)
```bash
# 1. Iniciar infraestructura externa (solo una vez)
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d --name sql-server -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

# 2. Desarrollo diario
bash start-local.sh           # Iniciar servicios
# [desarrollar código...]
bash check-local.sh           # Verificar estado
bash stop-local.sh            # Detener al final del día
```

### Para Demos y Testing (Más Fácil)
```bash
bash start-oms.sh             # Inicia todo automáticamente
bash test-integration.sh      # Prueba que todo funciona
```

## 🌟 Ventajas de Cada Modo

### 🏠 Modo Local (`start-local.sh`)
✅ **Desarrollo rápido** - Hot reload y debugging completo  
✅ **Menor recursos** - No overhead de Docker  
✅ **Debugging nativo** - Attach debugger directamente  
✅ **Logs en tiempo real** - tail -f logs/*.log  
✅ **Flexibilidad** - Ejecutar servicios individualmente  

❌ **Requiere dependencias** - .NET, Node.js, SQL Server  
❌ **Configuración manual** - Variables de entorno  

### 🐳 Modo Docker (`start-oms.sh`)
✅ **Un solo comando** - Todo incluido  
✅ **Aislamiento completo** - Sin conflictos de puertos  
✅ **Simula producción** - Comportamiento real  
✅ **Cero configuración** - Funciona out-of-the-box  

❌ **Recursos intensivos** - Más CPU/RAM  
❌ **Debugging limitado** - Más complejo depurar  
❌ **Builds más lentos** - Reconstruir imágenes  

## 📁 Archivos de Configuración Importantes

### Para Desarrollo Local
- `order-management-frontend/.env.example` - Configuración del frontend
- `LOCAL_DEVELOPMENT.md` - Guía completa de desarrollo local
- `logs/` - Directorio de logs de servicios locales

### Para Docker
- `docker-compose.yml` - Orquestación completa
- `DOCKER_DEPLOYMENT.md` - Guía de deployment Docker
- `config/` - Configuraciones específicas de Docker

## 🎮 Comandos de Uso Diario

```bash
# Ver estado rápido
bash check-local.sh --quick

# Ver logs en tiempo real
tail -f logs/customerservice.log
tail -f logs/orderservice.log

# Reiniciar un servicio específico
bash stop-local.sh && bash start-local.sh

# Limpiar todo y empezar de nuevo
bash stop-local.sh --force
bash start-local.sh

# Alternar entre modos
bash stop-local.sh              # Detener local
bash start-oms.sh               # Cambiar a Docker
# o viceversa
docker-compose down             # Detener Docker  
bash start-local.sh             # Cambiar a local
```

## 🔧 Troubleshooting Quick Fix

```bash
# Problema: Puerto ocupado
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Problema: Servicio no responde
bash check-local.sh             # Diagnóstico completo
bash stop-local.sh --force      # Limpieza forzada
bash start-local.sh             # Reinicio limpio

# Problema: Dependencias
dotnet --version                # Verificar .NET
npm --version                   # Verificar Node
sqlcmd -S localhost -Q "SELECT 1"  # Verificar SQL Server
```

---

**¡Ya tienes herramientas completas para desarrollo eficiente en ambos modos!** 🚀
