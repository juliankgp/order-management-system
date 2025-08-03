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
        return 1
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
docker-compose build --parallel
build_result=$?
show_status $build_result "Construcción de imágenes completada"

if [ $build_result -ne 0 ]; then
    echo "❌ Error en la construcción. Abortando..."
    exit 1
fi

echo ""
echo "📋 ORDEN DE INICIO OPTIMIZADO:"
echo "   1️⃣ Infraestructura (SQL Server, RabbitMQ)"
echo "   2️⃣ Backend Services (Customer, Product, Logging)"
echo "   3️⃣ Order Service (requiere otros servicios)"  
echo "   4️⃣ Frontend (React App)"
echo ""

# FASE 1: Infraestructura
echo "🏗️ FASE 1: Iniciando servicios de infraestructura..."
docker-compose up -d sqlserver rabbitmq
infra_result=$?
show_status $infra_result "Servicios de infraestructura iniciados"

if [ $infra_result -ne 0 ]; then
    echo "❌ Error en infraestructura. Abortando..."
    exit 1
fi

# Esperar que la infraestructura esté lista
echo "⏳ Esperando que la infraestructura esté lista (15 segundos)..."
sleep 15

# FASE 2: Servicios base del backend  
echo ""
echo "🚀 FASE 2: Iniciando servicios base del backend..."
docker-compose up -d --no-deps customer-service product-service logging-service
backend_result=$?
show_status $backend_result "Servicios base del backend iniciados"

# Esperar que los servicios base estén listos
echo "⏳ Esperando que los servicios base estén listos (10 segundos)..."
sleep 10

# FASE 3: OrderService (requiere otros servicios)
echo ""
echo "🚀 FASE 3: Iniciando OrderService..."
docker-compose up -d --no-deps order-service
order_result=$?
show_status $order_result "OrderService iniciado"

# Esperar que OrderService esté listo
sleep 5

# FASE 4: Frontend
echo ""
echo "🌐 FASE 4: Iniciando frontend..."
docker-compose up -d --no-deps frontend
frontend_result=$?
show_status $frontend_result "Frontend iniciado"

# Esperar estabilización final
echo ""
echo "⏳ Esperando estabilización completa del sistema (15 segundos)..."
sleep 15

# VERIFICACIÓN COMPLETA
echo ""
echo "📊 VERIFICACIÓN COMPLETA DEL SISTEMA"
echo "====================================="

# Función para probar un servicio con reintentos
test_service_with_retry() {
    local name=$1
    local url=$2
    local max_attempts=3
    local attempt=1
    
    echo -n "  🔍 $name... "
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s -f "$url" > /dev/null 2>&1; then
            echo "✅ OK"
            return 0
        fi
        
        if [ $attempt -lt $max_attempts ]; then
            echo -n "⏳ "
            sleep 3
        fi
        
        attempt=$((attempt + 1))
    done
    
    echo "❌ FAIL (después de $max_attempts intentos)"
    return 1
}

# Probar todos los servicios
echo ""
echo "🔧 BACKEND SERVICES:"
test_service_with_retry "CustomerService" "http://localhost:5003/health"
test_service_with_retry "ProductService" "http://localhost:5002/health"
test_service_with_retry "OrderService" "http://localhost:5001/health"
test_service_with_retry "LoggingService" "http://localhost:5004/health"

echo ""
echo "🌐 FRONTEND:"
test_service_with_retry "React App" "http://localhost:3000"

echo ""
echo "🏗️ INFRASTRUCTURE:"
test_service_with_retry "RabbitMQ Management" "http://localhost:15672"

echo ""
echo "🎉 ¡Order Management System iniciado exitosamente!"
echo "=================================================="
echo ""
echo "🌟 SISTEMA DISPONIBLE EN:"
echo ""
echo "   📱 APLICACIÓN PRINCIPAL:"
echo "   🌐 Frontend: http://localhost:3000"
echo ""
echo "   🔧 MICROSERVICIOS API:"
echo "   🛒 Customer Service: http://localhost:5003"
echo "   📦 Product Service: http://localhost:5002"
echo "   📋 Order Service: http://localhost:5001"
echo "   📝 Logging Service: http://localhost:5004"
echo ""
echo "   🛠️ HERRAMIENTAS DE GESTIÓN:"
echo "   🐰 RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "   🗄️ SQL Server: localhost:1433 (sa/YourPassword123!)"
echo ""
echo "📚 COMANDOS ÚTILES:"
echo "   🛑 Detener sistema: docker-compose down"
echo "   📋 Ver logs: docker-compose logs -f [service-name]"
echo "   📊 Ver estado: docker ps"
echo "   🧪 Ejecutar pruebas: bash test-integration.sh"
echo ""
echo "🎯 ¡Tu aplicación está lista para usar! 🚀"
