#!/bin/bash
# ===== SCRIPT DE DIAGNÓSTICO DEL SISTEMA =====
# Valida que todo esté funcionando correctamente

echo "🔍 Diagnóstico del Order Management System"
echo "=========================================="

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

success_count=0
total_checks=0

check() {
    local description="$1"
    local command="$2"
    local expected_output="$3"
    
    ((total_checks++))
    echo -n "Verificando $description... "
    
    if eval "$command" >/dev/null 2>&1; then
        echo -e "${GREEN}✅ OK${NC}"
        ((success_count++))
    else
        echo -e "${RED}❌ FAILED${NC}"
        if [ -n "$expected_output" ]; then
            echo "  Esperado: $expected_output"
        fi
    fi
}

echo -e "${BLUE}=== VERIFICANDO DOCKER ===${NC}"
check "Docker está corriendo" "docker info"
check "Docker Compose disponible" "docker-compose --version"

echo -e "${BLUE}=== VERIFICANDO CONTAINERS ===${NC}"
check "SQL Server container" "docker ps | grep oms-sqlserver"
check "RabbitMQ container" "docker ps | grep oms-rabbitmq"
check "Customer Service container" "docker ps | grep oms-customer-service"
check "Product Service container" "docker ps | grep oms-product-service"
check "Order Service container" "docker ps | grep oms-order-service"
check "Logging Service container" "docker ps | grep oms-logging-service"
check "Frontend container" "docker ps | grep oms-frontend"

echo -e "${BLUE}=== VERIFICANDO CONECTIVIDAD ===${NC}"
check "SQL Server está respondiendo" "docker exec oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'OrderManagement2024!' -Q 'SELECT 1'"
check "RabbitMQ está respondiendo" "docker exec oms-rabbitmq rabbitmq-diagnostics ping"

echo -e "${BLUE}=== VERIFICANDO SERVICIOS BACKEND ===${NC}"
check "Customer Service Health" "curl -f http://localhost:5003/health"
check "Product Service Health" "curl -f http://localhost:5002/health"
check "Order Service Health" "curl -f http://localhost:5001/health"
check "Logging Service Health" "curl -f http://localhost:5004/health"

echo -e "${BLUE}=== VERIFICANDO FRONTEND ===${NC}"
check "Frontend está disponible" "curl -f http://localhost:3000"

echo -e "${BLUE}=== VERIFICANDO APIS ===${NC}"
check "Customer Service Swagger" "curl -f http://localhost:5003/swagger"
check "Product Service Swagger" "curl -f http://localhost:5002/swagger"
check "Order Service Swagger" "curl -f http://localhost:5001/swagger"
check "Logging Service Swagger" "curl -f http://localhost:5004/swagger"

echo -e "${BLUE}=== VERIFICANDO MANAGEMENT UIs ===${NC}"
check "RabbitMQ Management UI" "curl -f http://localhost:15672"

echo ""
echo "=========================================="
if [ $success_count -eq $total_checks ]; then
    echo -e "${GREEN}🎉 TODOS LOS CHECKS PASARON ($success_count/$total_checks)${NC}"
    echo -e "${GREEN}El sistema está funcionando correctamente!${NC}"
    
    echo ""
    echo "📍 URLs disponibles:"
    echo "   Frontend:              http://localhost:3000"
    echo "   Customer Service API:  http://localhost:5003/swagger"
    echo "   Product Service API:   http://localhost:5002/swagger"
    echo "   Order Service API:     http://localhost:5001/swagger"
    echo "   Logging Service API:   http://localhost:5004/swagger"
    echo "   RabbitMQ Management:   http://localhost:15672"
    
    exit 0
else
    echo -e "${RED}❌ ALGUNOS CHECKS FALLARON ($success_count/$total_checks)${NC}"
    echo ""
    echo "🛠 Comandos de diagnóstico:"
    echo "   Ver logs de todos los servicios: docker-compose logs -f"
    echo "   Ver estado de containers:        docker-compose ps"
    echo "   Reiniciar servicios:             docker-compose restart"
    echo "   Limpiar y reiniciar:             docker-compose down && docker-compose up -d"
    
    exit 1
fi
