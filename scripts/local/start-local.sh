#!/bin/bash

# =============================================================================
# 🏠 Order Management System - Local Development Startup Script
# =============================================================================
# Este script inicia todos los servicios localmente sin Docker
# Útil para desarrollo rápido, debugging, y pruebas locales
# =============================================================================

set -e  # Salir en caso de error

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Función para mostrar encabezado
show_header() {
    echo -e "\n${CYAN}🏠 Iniciando Order Management System Localmente...${NC}"
    echo -e "${CYAN}========================================================${NC}"
}

# Función para verificar dependencias
check_dependencies() {
    echo -e "\n${BLUE}🔍 Verificando dependencias del sistema...${NC}"
    
    # Verificar .NET 8
    if ! command -v dotnet &> /dev/null; then
        echo -e "${RED}❌ .NET no está instalado. Instala .NET 8 SDK${NC}"
        exit 1
    fi
    
    local dotnet_version=$(dotnet --version)
    if [[ ! $dotnet_version == 8.* ]]; then
        echo -e "${YELLOW}⚠️  Se recomienda .NET 8. Versión actual: $dotnet_version${NC}"
    else
        echo -e "${GREEN}✅ .NET 8 SDK detectado: $dotnet_version${NC}"
    fi
    
    # Verificar Node.js
    if ! command -v npm &> /dev/null; then
        echo -e "${RED}❌ Node.js/npm no está instalado${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Node.js/npm detectado: $(node --version)${NC}"
    
    # Verificar SQL Server (conexión)
    echo -e "${BLUE}🔍 Verificando SQL Server...${NC}"
    if command -v sqlcmd &> /dev/null; then
        if sqlcmd -S localhost -Q "SELECT 1" -t 5 &> /dev/null; then
            echo -e "${GREEN}✅ SQL Server está disponible${NC}"
        else
            echo -e "${YELLOW}⚠️  SQL Server no responde. Asegúrate de que esté corriendo${NC}"
            echo -e "${YELLOW}   Puedes usar: 'docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong@Passw0rd -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest'${NC}"
        fi
    else
        echo -e "${YELLOW}⚠️  sqlcmd no está disponible. Instala SQL Server command line tools${NC}"
    fi
    
    # Verificar RabbitMQ
    echo -e "${BLUE}🔍 Verificando RabbitMQ...${NC}"
    if curl -s http://localhost:15672 &> /dev/null; then
        echo -e "${GREEN}✅ RabbitMQ está disponible${NC}"
    else
        echo -e "${YELLOW}⚠️  RabbitMQ no responde. Iniciando RabbitMQ automáticamente...${NC}"
        
        # Detener contenedor existente si existe
        docker stop rabbitmq-local 2>/dev/null || true
        docker rm rabbitmq-local 2>/dev/null || true
        
        # Iniciar RabbitMQ con configuración correcta
        docker run -d --name rabbitmq-local \
            -p 5672:5672 \
            -p 15672:15672 \
            -e RABBITMQ_DEFAULT_USER=guest \
            -e RABBITMQ_DEFAULT_PASS=guest \
            rabbitmq:3-management
        
        echo -e "${BLUE}⏳ Esperando que RabbitMQ se inicie completamente...${NC}"
        sleep 20
        
        # Verificar que esté funcionando
        if curl -s http://localhost:15672 &> /dev/null; then
            echo -e "${GREEN}✅ RabbitMQ iniciado exitosamente${NC}"
        else
            echo -e "${YELLOW}⚠️  RabbitMQ tardó más de lo esperado. Continúa de todas formas...${NC}"
        fi
    fi
}

# Función para crear directorios de logs
setup_logging_directories() {
    echo -e "\n${BLUE}📁 Configurando directorios de logs...${NC}"
    
    local backend_root="./order-management-backend"
    local services=("CustomerService" "OrderService" "ProductService" "LoggingService")
    
    for service in "${services[@]}"; do
        local log_dir="$backend_root/services/$service/src/Api/logs"
        mkdir -p "$log_dir"
        echo -e "${GREEN}✅ Directorio creado: $log_dir${NC}"
    done
}

# Función para instalar dependencias del frontend
install_frontend_dependencies() {
    echo -e "\n${BLUE}📦 Instalando dependencias del frontend...${NC}"
    cd ./order-management-frontend
    
    if [ ! -d "node_modules" ]; then
        echo -e "${BLUE}Instalando paquetes npm...${NC}"
        npm install
    else
        echo -e "${GREEN}✅ node_modules ya existe${NC}"
    fi
    
    cd ..
}

# Función para compilar servicios backend
build_backend_services() {
    echo -e "\n${BLUE}🔨 Compilando servicios backend...${NC}"
    
    local backend_root="./order-management-backend"
    local services=("CustomerService" "ProductService" "LoggingService" "OrderService")
    
    for service in "${services[@]}"; do
        echo -e "${BLUE}🔨 Compilando $service...${NC}"
        cd "$backend_root/services/$service/src/Api"
        
        if dotnet build --configuration Release; then
            echo -e "${GREEN}✅ $service compilado exitosamente${NC}"
        else
            echo -e "${RED}❌ Error compilando $service${NC}"
            exit 1
        fi
        
        cd - > /dev/null
    done
}

# Función para iniciar un servicio backend
start_backend_service() {
    local service_name=$1
    local service_port=$2
    local project_path=$3
    
    echo -e "${BLUE}🚀 Iniciando $service_name en puerto $service_port...${NC}"
    
    cd "$project_path"
    
    # Iniciar el servicio en background
    nohup dotnet run --urls="http://localhost:$service_port" --environment=Development > "../../../../../logs/${service_name,,}.log" 2>&1 &
    local pid=$!
    
    # Guardar PID para poder hacer cleanup después
    echo $pid > "../../../../../logs/${service_name,,}.pid"
    
    cd - > /dev/null
    
    # Esperar un momento para que el servicio inicie
    sleep 3
    
    # Verificar que el servicio esté corriendo
    if curl -s "http://localhost:$service_port/health" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ $service_name está corriendo en http://localhost:$service_port${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠️  $service_name iniciado, verificando disponibilidad...${NC}"
        
        # Reintentar verificación por 30 segundos
        for i in {1..10}; do
            sleep 3
            if curl -s "http://localhost:$service_port/health" > /dev/null 2>&1; then
                echo -e "${GREEN}✅ $service_name está corriendo en http://localhost:$service_port${NC}"
                return 0
            fi
            echo -e "${YELLOW}   Intento $i/10...${NC}"
        done
        
        echo -e "${YELLOW}⚠️  $service_name está iniciando, pero no responde aún en health check${NC}"
        return 1
    fi
}

# Función para iniciar todos los servicios backend
start_backend_services() {
    echo -e "\n${PURPLE}🚀 INICIANDO SERVICIOS BACKEND${NC}"
    echo -e "${PURPLE}================================${NC}"
    
    # Crear directorio para logs y PIDs
    mkdir -p logs
    
    local backend_root="./order-management-backend/services"
    
    # Iniciar servicios en orden de dependencia
    start_backend_service "CustomerService" "5003" "$backend_root/CustomerService/src/Api"
    start_backend_service "ProductService" "5002" "$backend_root/ProductService/src/Api"
    start_backend_service "LoggingService" "5004" "$backend_root/LoggingService/src/Api"
    
    # Esperar que los servicios base estén estables
    echo -e "\n${BLUE}⏳ Esperando estabilización de servicios base (10 segundos)...${NC}"
    sleep 10
    
    # Iniciar OrderService (depende de otros servicios)
    start_backend_service "OrderService" "5001" "$backend_root/OrderService/src/Api"
}

# Función para iniciar el frontend
start_frontend() {
    echo -e "\n${PURPLE}🌐 INICIANDO FRONTEND${NC}"
    echo -e "${PURPLE}=====================${NC}"
    
    cd ./order-management-frontend
    
    # Configurar variables de entorno para desarrollo local
    export VITE_DOCKER_MODE=false
    export VITE_ORDER_SERVICE_URL=http://localhost:5001
    export VITE_PRODUCT_SERVICE_URL=http://localhost:5002
    export VITE_CUSTOMER_SERVICE_URL=http://localhost:5003
    export VITE_LOGGING_SERVICE_URL=http://localhost:5004
    
    echo -e "${BLUE}🚀 Iniciando servidor de desarrollo Vite...${NC}"
    
    # Iniciar frontend en background
    nohup npm run dev > ../logs/frontend.log 2>&1 &
    local pid=$!
    echo $pid > ../logs/frontend.pid
    
    cd ..
    
    # Esperar que el frontend inicie
    echo -e "${BLUE}⏳ Esperando que el frontend esté listo...${NC}"
    sleep 5
    
    # Verificar frontend
    for i in {1..10}; do
        if curl -s http://localhost:3000 > /dev/null 2>&1; then
            echo -e "${GREEN}✅ Frontend está corriendo en http://localhost:3000${NC}"
            return 0
        fi
        sleep 2
        echo -e "${YELLOW}   Verificando frontend... $i/10${NC}"
    done
    
    echo -e "${YELLOW}⚠️  Frontend iniciado, pero puede estar aún cargando${NC}"
}

# Función para mostrar estado del sistema
show_system_status() {
    echo -e "\n${WHITE}📊 ESTADO DEL SISTEMA${NC}"
    echo -e "${WHITE}=====================${NC}"
    
    local services=(
        "CustomerService:5003:/health"
        "ProductService:5002:/health"
        "OrderService:5001:/health"
        "LoggingService:5004:/health"
        "Frontend:3000:/"
    )
    
    for service_info in "${services[@]}"; do
        IFS=':' read -r service_name port endpoint <<< "$service_info"
        
        if curl -s "http://localhost:$port$endpoint" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ $service_name: http://localhost:$port${NC}"
        else
            echo -e "${RED}❌ $service_name: No responde en puerto $port${NC}"
        fi
    done
    
    echo -e "\n${CYAN}🔗 URLs útiles para desarrollo:${NC}"
    echo -e "${CYAN}   Frontend:           http://localhost:3000${NC}"
    echo -e "${CYAN}   Order Service:      http://localhost:5001/swagger${NC}"
    echo -e "${CYAN}   Product Service:    http://localhost:5002/swagger${NC}"
    echo -e "${CYAN}   Customer Service:   http://localhost:5003/swagger${NC}"
    echo -e "${CYAN}   Logging Service:    http://localhost:5004/swagger${NC}"
    echo -e "${CYAN}   RabbitMQ Management: http://localhost:15672 (guest/guest)${NC}"
}

# Función para mostrar información final
show_final_info() {
    echo -e "\n${WHITE}🎉 ¡SISTEMA INICIADO LOCALMENTE!${NC}"
    echo -e "${WHITE}=================================${NC}"
    
    echo -e "\n${YELLOW}📋 Para detener todos los servicios:${NC}"
    echo -e "${YELLOW}   bash stop-local.sh${NC}"
    
    echo -e "\n${YELLOW}📋 Para ver logs en tiempo real:${NC}"
    echo -e "${YELLOW}   tail -f logs/customerservice.log${NC}"
    echo -e "${YELLOW}   tail -f logs/productservice.log${NC}"
    echo -e "${YELLOW}   tail -f logs/orderservice.log${NC}"
    echo -e "${YELLOW}   tail -f logs/loggingservice.log${NC}"
    echo -e "${YELLOW}   tail -f logs/frontend.log${NC}"
    
    echo -e "\n${YELLOW}📋 Para ejecutar tests:${NC}"
    echo -e "${YELLOW}   cd order-management-frontend && npm test${NC}"
    
    echo -e "\n${GREEN}✨ ¡Happy coding! ✨${NC}"
}

# =============================================================================
# FUNCIÓN PRINCIPAL
# =============================================================================
main() {
    show_header
    check_dependencies
    setup_logging_directories
    install_frontend_dependencies
    build_backend_services
    start_backend_services
    start_frontend
    show_system_status
    show_final_info
}

# Verificar si se está ejecutando el script directamente
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
