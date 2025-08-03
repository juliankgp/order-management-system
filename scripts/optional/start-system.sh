#!/bin/bash
# ===== SCRIPT DE INICIO COMPLETO - ORDER MANAGEMENT SYSTEM =====
# Un solo comando para levantar todo el sistema

set -e  # Salir si hay errores

echo "🚀 Iniciando Order Management System con Docker..."
echo "=================================================="

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Función para imprimir mensajes coloreados
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Verificar que Docker esté corriendo
if ! docker info > /dev/null 2>&1; then
    print_error "Docker no está corriendo. Por favor inicia Docker Desktop."
    exit 1
fi

print_success "Docker está corriendo ✅"

# Crear directorios de logs si no existen
print_status "Creando directorios de logs..."
mkdir -p logs/customer-service
mkdir -p logs/product-service
mkdir -p logs/order-service
mkdir -p logs/logging-service

# Limpiar containers y volúmenes previos (opcional)
if [ "$1" = "--clean" ]; then
    print_warning "Limpiando containers y volúmenes previos..."
    docker-compose down -v --remove-orphans 2>/dev/null || true
    docker system prune -f 2>/dev/null || true
fi

# Construir y levantar servicios
print_status "Construyendo imágenes y levantando servicios..."
print_status "Esto puede tomar varios minutos la primera vez..."

# Levantar infraestructura primero
print_status "1/4 - Levantando infraestructura (SQL Server y RabbitMQ)..."
docker-compose up -d sqlserver rabbitmq

# Esperar a que la infraestructura esté lista
print_status "Esperando a que la infraestructura esté lista..."
sleep 30

# Verificar que SQL Server esté listo
print_status "Verificando SQL Server..."
for i in {1..30}; do
    if docker exec oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "OrderManagement2024!" -Q "SELECT 1" > /dev/null 2>&1; then
        print_success "SQL Server está listo ✅"
        break
    fi
    if [ $i -eq 30 ]; then
        print_error "SQL Server no está respondiendo después de 5 minutos"
        exit 1
    fi
    echo -n "."
    sleep 10
done

# Verificar que RabbitMQ esté listo
print_status "Verificando RabbitMQ..."
for i in {1..20}; do
    if docker exec oms-rabbitmq rabbitmq-diagnostics ping > /dev/null 2>&1; then
        print_success "RabbitMQ está listo ✅"
        break
    fi
    if [ $i -eq 20 ]; then
        print_error "RabbitMQ no está respondiendo después de 3 minutos"
        exit 1
    fi
    echo -n "."
    sleep 10
done

# Levantar servicios backend
print_status "2/4 - Levantando servicios backend..."
docker-compose up -d customer-service product-service logging-service
sleep 20

# Levantar order service (depende de los otros servicios)
print_status "3/4 - Levantando Order Service..."
docker-compose up -d order-service
sleep 15

# Levantar frontend
print_status "4/4 - Levantando Frontend..."
docker-compose up -d frontend

# Opcionalmente levantar nginx
if [ "$2" = "--with-nginx" ]; then
    print_status "Levantando Nginx reverse proxy..."
    docker-compose up -d nginx
fi

# Verificar estado de todos los servicios
print_status "Verificando estado de los servicios..."
sleep 10

# Función para verificar health de un servicio
check_service_health() {
    local service_name=$1
    local port=$2
    
    for i in {1..10}; do
        if curl -f http://localhost:$port/health > /dev/null 2>&1; then
            print_success "$service_name está saludable ✅"
            return 0
        fi
        sleep 5
    done
    print_warning "$service_name no responde en el puerto $port ⚠️"
    return 1
}

# Verificar servicios backend
check_service_health "Customer Service" 5003
check_service_health "Product Service" 5002
check_service_health "Order Service" 5001
check_service_health "Logging Service" 5004

# Verificar frontend
if curl -f http://localhost:3000 > /dev/null 2>&1; then
    print_success "Frontend está disponible ✅"
else
    print_warning "Frontend no responde en el puerto 3000 ⚠️"
fi

echo ""
echo "=================================================="
print_success "🎉 Order Management System iniciado exitosamente!"
echo "=================================================="
echo ""
echo "📍 URLs disponibles:"
echo "   Frontend:              http://localhost:3000"
echo "   Customer Service API:  http://localhost:5003/swagger"
echo "   Product Service API:   http://localhost:5002/swagger"
echo "   Order Service API:     http://localhost:5001/swagger"
echo "   Logging Service API:   http://localhost:5004/swagger"
echo "   RabbitMQ Management:   http://localhost:15672 (admin/OrderManagement2024!)"
echo ""
if [ "$2" = "--with-nginx" ]; then
    echo "   Nginx Proxy:           http://localhost"
    echo ""
fi
echo "📋 Comandos útiles:"
echo "   Ver logs en tiempo real:    docker-compose logs -f"
echo "   Ver estado de servicios:    docker-compose ps"
echo "   Parar todos los servicios:  docker-compose down"
echo "   Limpiar todo:               docker-compose down -v --remove-orphans"
echo ""
print_status "Para ver logs en tiempo real ejecuta: docker-compose logs -f"
