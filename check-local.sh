#!/bin/bash

# =============================================================================
# 🔍 Order Management System - Local Development Health Check
# =============================================================================
# Este script verifica el estado de todos los servicios locales
# =============================================================================

set -e

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
    echo -e "\n${CYAN}🔍 Order Management System - Verificación Local${NC}"
    echo -e "${CYAN}===============================================${NC}"
}

# Función para verificar un servicio
check_service() {
    local service_name=$1
    local port=$2
    local endpoint=$3
    local timeout=${4:-5}
    
    echo -e "\n${BLUE}🔍 Verificando $service_name en puerto $port...${NC}"
    
    # Verificar conectividad básica
    if curl -s --max-time $timeout "http://localhost:$port$endpoint" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ $service_name: DISPONIBLE${NC}"
        
        # Obtener información adicional si es posible
        if [[ "$endpoint" == "/health" ]]; then
            local health_status=$(curl -s --max-time $timeout "http://localhost:$port/health" 2>/dev/null || echo "No disponible")
            echo -e "${GREEN}   Estado: $health_status${NC}"
        fi
        
        return 0
    else
        echo -e "${RED}❌ $service_name: NO DISPONIBLE${NC}"
        return 1
    fi
}

# Función para verificar infraestructura
check_infrastructure() {
    echo -e "\n${PURPLE}🏗️  VERIFICANDO INFRAESTRUCTURA${NC}"
    echo -e "${PURPLE}================================${NC}"
    
    local infra_ok=true
    
    # Verificar SQL Server
    echo -e "\n${BLUE}🔍 Verificando SQL Server...${NC}"
    if command -v sqlcmd &> /dev/null; then
        if sqlcmd -S localhost -Q "SELECT GETDATE() as CurrentTime" -h -1 -t 5 &> /dev/null; then
            echo -e "${GREEN}✅ SQL Server: DISPONIBLE${NC}"
            local sql_version=$(sqlcmd -S localhost -Q "SELECT @@VERSION" -h -1 -t 5 2>/dev/null | head -1 || echo "Versión no disponible")
            echo -e "${GREEN}   Versión: ${sql_version:0:50}...${NC}"
        else
            echo -e "${RED}❌ SQL Server: NO DISPONIBLE${NC}"
            infra_ok=false
        fi
    else
        echo -e "${YELLOW}⚠️  sqlcmd no disponible - no se puede verificar SQL Server${NC}"
    fi
    
    # Verificar RabbitMQ
    echo -e "\n${BLUE}🔍 Verificando RabbitMQ...${NC}"
    if curl -s --max-time 5 http://localhost:15672 > /dev/null 2>&1; then
        echo -e "${GREEN}✅ RabbitMQ Management: DISPONIBLE${NC}"
        echo -e "${GREEN}   URL: http://localhost:15672 (guest/guest)${NC}"
    else
        echo -e "${RED}❌ RabbitMQ Management: NO DISPONIBLE${NC}"
        infra_ok=false
    fi
    
    # Verificar puerto RabbitMQ AMQP
    if nc -z localhost 5672 2>/dev/null || telnet localhost 5672 </dev/null 2>/dev/null | grep -q "Connected"; then
        echo -e "${GREEN}✅ RabbitMQ AMQP (5672): DISPONIBLE${NC}"
    else
        echo -e "${RED}❌ RabbitMQ AMQP (5672): NO DISPONIBLE${NC}"
        infra_ok=false
    fi
    
    if [ "$infra_ok" = true ]; then
        echo -e "\n${GREEN}✅ Infraestructura: LISTA${NC}"
    else
        echo -e "\n${RED}❌ Infraestructura: PROBLEMAS DETECTADOS${NC}"
    fi
    
    return $([ "$infra_ok" = true ] && echo 0 || echo 1)
}

# Función para verificar servicios backend
check_backend_services() {
    echo -e "\n${PURPLE}🔧 VERIFICANDO SERVICIOS BACKEND${NC}"
    echo -e "${PURPLE}==================================${NC}"
    
    local services_ok=0
    local total_services=4
    
    # Verificar cada servicio
    if check_service "CustomerService" "5003" "/health"; then
        ((services_ok++))
    fi
    
    if check_service "ProductService" "5002" "/health"; then
        ((services_ok++))
    fi
    
    if check_service "LoggingService" "5004" "/health"; then
        ((services_ok++))
    fi
    
    if check_service "OrderService" "5001" "/health"; then
        ((services_ok++))
    fi
    
    echo -e "\n${BLUE}📊 Servicios Backend: $services_ok/$total_services disponibles${NC}"
    
    if [ $services_ok -eq $total_services ]; then
        echo -e "${GREEN}✅ Todos los servicios backend están funcionando${NC}"
        return 0
    else
        echo -e "${RED}❌ Algunos servicios backend no están disponibles${NC}"
        return 1
    fi
}

# Función para verificar frontend
check_frontend() {
    echo -e "\n${PURPLE}🌐 VERIFICANDO FRONTEND${NC}"
    echo -e "${PURPLE}=======================${NC}"
    
    if check_service "Frontend" "3000" "/"; then
        echo -e "${GREEN}✅ Frontend disponible en http://localhost:3000${NC}"
        return 0
    else
        echo -e "${RED}❌ Frontend no está disponible${NC}"
        return 1
    fi
}

# Función para probar endpoints de API
test_api_endpoints() {
    echo -e "\n${PURPLE}🧪 PROBANDO ENDPOINTS DE API${NC}"
    echo -e "${PURPLE}=============================${NC}"
    
    local endpoints_ok=0
    local total_endpoints=0
    
    # Probar endpoints básicos
    local endpoints=(
        "CustomerService:5003:/api/customers:GET"
        "ProductService:5002:/api/products:GET"
        "OrderService:5001:/api/orders:GET"
        "LoggingService:5004:/api/logs:GET"
    )
    
    for endpoint_info in "${endpoints[@]}"; do
        IFS=':' read -r service_name port path method <<< "$endpoint_info"
        ((total_endpoints++))
        
        echo -e "\n${BLUE}🧪 Probando $service_name $method $path...${NC}"
        
        local response_code=$(curl -s -o /dev/null -w "%{http_code}" --max-time 10 "http://localhost:$port$path" 2>/dev/null || echo "000")
        
        if [[ "$response_code" =~ ^[2-3][0-9][0-9]$ ]]; then
            echo -e "${GREEN}✅ $service_name $path: HTTP $response_code${NC}"
            ((endpoints_ok++))
        else
            echo -e "${RED}❌ $service_name $path: HTTP $response_code (Error)${NC}"
        fi
    done
    
    echo -e "\n${BLUE}📊 Endpoints: $endpoints_ok/$total_endpoints funcionando${NC}"
    
    if [ $endpoints_ok -eq $total_endpoints ]; then
        echo -e "${GREEN}✅ Todos los endpoints están funcionando${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠️  Algunos endpoints no están funcionando correctamente${NC}"
        return 1
    fi
}

# Función para verificar logs
check_logs() {
    echo -e "\n${PURPLE}📋 VERIFICANDO LOGS${NC}"
    echo -e "${PURPLE}===================${NC}"
    
    if [ ! -d "logs" ]; then
        echo -e "${YELLOW}⚠️  Directorio de logs no existe${NC}"
        return 1
    fi
    
    local log_files=(
        "customerservice.log"
        "productservice.log"
        "orderservice.log"
        "loggingservice.log"
        "frontend.log"
    )
    
    local logs_ok=0
    
    for log_file in "${log_files[@]}"; do
        local log_path="logs/$log_file"
        
        if [ -f "$log_path" ]; then
            local log_size=$(stat -f%z "$log_path" 2>/dev/null || stat -c%s "$log_path" 2>/dev/null || echo "0")
            local last_update=$(stat -f%m "$log_path" 2>/dev/null || stat -c%Y "$log_path" 2>/dev/null || echo "0")
            local now=$(date +%s)
            local time_diff=$((now - last_update))
            
            if [ $time_diff -lt 300 ]; then  # Actualizado en los últimos 5 minutos
                echo -e "${GREEN}✅ $log_file: Activo (${log_size} bytes, actualizado hace ${time_diff}s)${NC}"
                ((logs_ok++))
            else
                echo -e "${YELLOW}⚠️  $log_file: Inactivo (última actualización hace ${time_diff}s)${NC}"
            fi
        else
            echo -e "${RED}❌ $log_file: No existe${NC}"
        fi
    done
    
    echo -e "\n${BLUE}📊 Logs activos: $logs_ok/${#log_files[@]}${NC}"
}

# Función para mostrar resumen del sistema
show_system_summary() {
    echo -e "\n${WHITE}📊 RESUMEN DEL SISTEMA${NC}"
    echo -e "${WHITE}======================${NC}"
    
    echo -e "\n${CYAN}🔗 URLs del Sistema:${NC}"
    echo -e "${CYAN}   🌐 Frontend:            http://localhost:3000${NC}"
    echo -e "${CYAN}   📦 Customer Service:    http://localhost:5003/swagger${NC}"
    echo -e "${CYAN}   📦 Product Service:     http://localhost:5002/swagger${NC}"
    echo -e "${CYAN}   📦 Order Service:       http://localhost:5001/swagger${NC}"
    echo -e "${CYAN}   📦 Logging Service:     http://localhost:5004/swagger${NC}"
    echo -e "${CYAN}   🐰 RabbitMQ Management: http://localhost:15672${NC}"
    
    echo -e "\n${YELLOW}📋 Comandos Útiles:${NC}"
    echo -e "${YELLOW}   Ver logs:       tail -f logs/[service].log${NC}"
    echo -e "${YELLOW}   Detener todo:   bash stop-local.sh${NC}"
    echo -e "${YELLOW}   Reiniciar:      bash stop-local.sh && bash start-local.sh${NC}"
}

# Función para verificación rápida
quick_check() {
    echo -e "\n${BLUE}⚡ VERIFICACIÓN RÁPIDA${NC}"
    echo -e "${BLUE}=====================${NC}"
    
    local services=(
        "Frontend:3000"
        "OrderService:5001"
        "ProductService:5002"
        "CustomerService:5003"
        "LoggingService:5004"
    )
    
    for service_info in "${services[@]}"; do
        IFS=':' read -r service_name port <<< "$service_info"
        
        if curl -s --max-time 2 "http://localhost:$port" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ $service_name${NC}"
        else
            echo -e "${RED}❌ $service_name${NC}"
        fi
    done
}

# =============================================================================
# FUNCIÓN PRINCIPAL
# =============================================================================
main() {
    local quick_mode=false
    
    # Verificar argumentos
    if [[ "$1" == "--quick" || "$1" == "-q" ]]; then
        quick_mode=true
    fi
    
    show_header
    
    if [ "$quick_mode" = true ]; then
        quick_check
    else
        check_infrastructure
        echo ""
        check_backend_services
        echo ""
        check_frontend
        echo ""
        test_api_endpoints
        echo ""
        check_logs
        echo ""
        show_system_summary
    fi
    
    echo -e "\n${GREEN}✅ Verificación completada${NC}"
}

# Verificar si se está ejecutando el script directamente
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
