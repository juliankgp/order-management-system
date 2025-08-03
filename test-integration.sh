#!/bin/bash

echo "🧪 Iniciando pruebas de integración del Order Management System..."
echo ""

# Test Frontend
echo "🌐 Probando Frontend (http://localhost:3000)..."
if curl -s -f http://localhost:3000 > /dev/null; then
    echo "✅ Frontend: OK"
else
    echo "❌ Frontend: FAIL"
fi

# Test Backend Services
echo ""
echo "🚀 Probando servicios del Backend..."

services=("CustomerService:5003" "ProductService:5002" "OrderService:5001" "LoggingService:5004")

for service in "${services[@]}"; do
    IFS=':' read -r name port <<< "$service"
    echo "  🔍 Probando $name (http://localhost:$port/health)..."
    if curl -s -f "http://localhost:$port/health" | grep -q "Healthy"; then
        echo "    ✅ $name: Healthy"
    else
        echo "    ❌ $name: FAIL"
    fi
done

# Test Infrastructure
echo ""
echo "🏗️ Probando infraestructura..."

echo "  🐰 Probando RabbitMQ Management (http://localhost:15672)..."
if curl -s -f http://localhost:15672 > /dev/null; then
    echo "    ✅ RabbitMQ Management: OK"
else
    echo "    ❌ RabbitMQ Management: FAIL"
fi

echo "  🗄️ Probando SQL Server (localhost:1433)..."
if docker exec oms-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourPassword123!" -Q "SELECT 1" -C > /dev/null 2>&1; then
    echo "    ✅ SQL Server: OK"
else
    echo "    ❌ SQL Server: FAIL"
fi

echo ""
echo "🎉 Pruebas de integración completadas!"
echo "📊 Tu Order Management System está corriendo en Docker con los siguientes endpoints:"
echo "   🌐 Frontend: http://localhost:3000"
echo "   🛒 Customer Service: http://localhost:5003"
echo "   📦 Product Service: http://localhost:5002" 
echo "   📋 Order Service: http://localhost:5001"
echo "   📝 Logging Service: http://localhost:5004"
echo "   🐰 RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "   🗄️ SQL Server: localhost:1433 (sa/YourPassword123!)"
