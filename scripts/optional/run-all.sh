#!/bin/bash

echo "🚀 Iniciando Order Management System con Docker..."
echo "=================================================="
echo ""

# Función para mostrar el status con colores
show_status() {
    if [ $1 -eq 0 ]; then
        echo "✅ $2"
    else
        echo "❌ $2"
    fi
}

# Verificar que Docker esté corriendo
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker no está corriendo. Por favor inicia Docker Desktop."
    exit 1
fi
echo "✅ Docker está corriendo"

# Limpiar contenedores previos
echo ""
echo "🧹 Limpiando contenedores previos..."
docker-compose down > /dev/null 2>&1

# Construir todas las imágenes
echo ""
echo "🔨 Construyendo imágenes Docker..."
docker-compose build
build_result=$?
show_status $build_result "Construcción de imágenes completada"

if [ $build_result -ne 0 ]; then
    echo "❌ Error en la construcción. Abortando..."
    exit 1
fi

# Iniciando servicios de infraestructura primero
echo ""
echo "🏗️ Iniciando servicios de infraestructura..."
docker-compose up -d sqlserver rabbitmq
infra_result=$?
show_status $infra_result "Servicios de infraestructura iniciados"

# Esperar un momento para que la infraestructura esté lista
echo ""
echo "⏳ Esperando que la infraestructura esté lista..."
sleep 10

# Iniciar servicios del backend (sin dependencias de health checks)
echo ""
echo "🚀 Iniciando servicios del backend..."
docker-compose up -d --no-deps customer-service product-service logging-service
backend_result=$?
show_status $backend_result "Servicios del backend iniciados (parte 1)"

# Esperar un momento antes de iniciar OrderService
sleep 5

# Iniciar OrderService por separado
echo "🚀 Iniciando OrderService..."
docker-compose up -d --no-deps order-service
order_result=$?
show_status $order_result "OrderService iniciado"

# Iniciar frontend
echo ""
echo "🌐 Iniciando frontend..."
docker-compose up -d --no-deps frontend
frontend_result=$?
show_status $frontend_result "Frontend iniciado"

# Esperar un momento para que todo se estabilice
echo ""
echo "⏳ Esperando que todos los servicios se estabilicen..."
sleep 15

# Verificar el estado final
echo ""
echo "📊 Verificando estado de los servicios..."
echo ""

# Función para probar un servicio
test_service() {
    local name=$1
    local url=$2
    echo -n "  🔍 $name... "
    if curl -s -f "$url" > /dev/null 2>&1; then
        echo "✅ OK"
        return 0
    else
        echo "❌ FAIL"
        return 1
    fi
}

# Probar servicios del backend
test_service "CustomerService" "http://localhost:5003/health"
test_service "ProductService" "http://localhost:5002/health"
test_service "OrderService" "http://localhost:5001/health"
test_service "LoggingService" "http://localhost:5004/health"

# Probar frontend
test_service "Frontend" "http://localhost:3000"

# Probar infraestructura
test_service "RabbitMQ Management" "http://localhost:15672"

echo ""
echo "🎉 ¡Order Management System iniciado!"
echo "=================================================="
echo ""
echo "🌟 Tu sistema está corriendo en los siguientes endpoints:"
echo ""
echo "   📱 FRONTEND:"
echo "   🌐 Aplicación Web: http://localhost:3000"
echo ""
echo "   🔧 BACKEND SERVICES:"
echo "   🛒 Customer Service: http://localhost:5003"
echo "   📦 Product Service: http://localhost:5002"
echo "   📋 Order Service: http://localhost:5001"
echo "   📝 Logging Service: http://localhost:5004"
echo ""
echo "   🏗️ INFRASTRUCTURE:"
echo "   🐰 RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "   🗄️ SQL Server: localhost:1433 (sa/YourPassword123!)"
echo ""
echo "💡 Para detener todo el sistema ejecuta: docker-compose down"
echo "🔍 Para ver logs: docker-compose logs -f [service-name]"
echo "📊 Para ver el estado: docker ps"
echo ""
