# ===== SCRIPT POWERSHELL DE DIAGNÓSTICO =====
# Valida que todo esté funcionando correctamente

Write-Host "🔍 Diagnóstico del Order Management System" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$successCount = 0
$totalChecks = 0

function Test-Component {
    param(
        [string]$Description,
        [string]$Command,
        [string]$ExpectedOutput = ""
    )
    
    $script:totalChecks++
    Write-Host "Verificando $Description... " -NoNewline
    
    try {
        $result = Invoke-Expression $Command 2>$null
        if ($LASTEXITCODE -eq 0 -or $result) {
            Write-Host "✅ OK" -ForegroundColor Green
            $script:successCount++
        } else {
            Write-Host "❌ FAILED" -ForegroundColor Red
            if ($ExpectedOutput) {
                Write-Host "  Esperado: $ExpectedOutput" -ForegroundColor Yellow
            }
        }
    }
    catch {
        Write-Host "❌ FAILED" -ForegroundColor Red
        if ($ExpectedOutput) {
            Write-Host "  Esperado: $ExpectedOutput" -ForegroundColor Yellow
        }
    }
}

function Test-Url {
    param(
        [string]$Description,
        [string]$Url
    )
    
    $script:totalChecks++
    Write-Host "Verificando $Description... " -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ OK" -ForegroundColor Green
            $script:successCount++
        } else {
            Write-Host "❌ FAILED (Status: $($response.StatusCode))" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "❌ FAILED ($($_.Exception.Message))" -ForegroundColor Red
    }
}

Write-Host "=== VERIFICANDO DOCKER ===" -ForegroundColor Blue
Test-Component "Docker está corriendo" "docker info"
Test-Component "Docker Compose disponible" "docker-compose --version"

Write-Host "=== VERIFICANDO CONTAINERS ===" -ForegroundColor Blue
Test-Component "SQL Server container" "docker ps | Select-String 'oms-sqlserver'"
Test-Component "RabbitMQ container" "docker ps | Select-String 'oms-rabbitmq'"
Test-Component "Customer Service container" "docker ps | Select-String 'oms-customer-service'"
Test-Component "Product Service container" "docker ps | Select-String 'oms-product-service'"
Test-Component "Order Service container" "docker ps | Select-String 'oms-order-service'"
Test-Component "Logging Service container" "docker ps | Select-String 'oms-logging-service'"
Test-Component "Frontend container" "docker ps | Select-String 'oms-frontend'"

Write-Host "=== VERIFICANDO CONECTIVIDAD ===" -ForegroundColor Blue
Test-Component "SQL Server está respondiendo" "docker exec oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'OrderManagement2024!' -Q 'SELECT 1'"
Test-Component "RabbitMQ está respondiendo" "docker exec oms-rabbitmq rabbitmq-diagnostics ping"

Write-Host "=== VERIFICANDO SERVICIOS BACKEND ===" -ForegroundColor Blue
Test-Url "Customer Service Health" "http://localhost:5003/health"
Test-Url "Product Service Health" "http://localhost:5002/health"
Test-Url "Order Service Health" "http://localhost:5001/health"
Test-Url "Logging Service Health" "http://localhost:5004/health"

Write-Host "=== VERIFICANDO FRONTEND ===" -ForegroundColor Blue
Test-Url "Frontend está disponible" "http://localhost:3000"

Write-Host "=== VERIFICANDO APIS ===" -ForegroundColor Blue
Test-Url "Customer Service Swagger" "http://localhost:5003/swagger"
Test-Url "Product Service Swagger" "http://localhost:5002/swagger"
Test-Url "Order Service Swagger" "http://localhost:5001/swagger"
Test-Url "Logging Service Swagger" "http://localhost:5004/swagger"

Write-Host "=== VERIFICANDO MANAGEMENT UIs ===" -ForegroundColor Blue
Test-Url "RabbitMQ Management UI" "http://localhost:15672"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan

if ($successCount -eq $totalChecks) {
    Write-Host "🎉 TODOS LOS CHECKS PASARON ($successCount/$totalChecks)" -ForegroundColor Green
    Write-Host "El sistema está funcionando correctamente!" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "📍 URLs disponibles:" -ForegroundColor Yellow
    Write-Host "   Frontend:              http://localhost:3000"
    Write-Host "   Customer Service API:  http://localhost:5003/swagger"
    Write-Host "   Product Service API:   http://localhost:5002/swagger"
    Write-Host "   Order Service API:     http://localhost:5001/swagger"
    Write-Host "   Logging Service API:   http://localhost:5004/swagger"
    Write-Host "   RabbitMQ Management:   http://localhost:15672"
    
    exit 0
} else {
    Write-Host "❌ ALGUNOS CHECKS FALLARON ($successCount/$totalChecks)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🛠 Comandos de diagnóstico:" -ForegroundColor Yellow
    Write-Host "   Ver logs de todos los servicios: docker-compose logs -f"
    Write-Host "   Ver estado de containers:        docker-compose ps"
    Write-Host "   Reiniciar servicios:             docker-compose restart"
    Write-Host "   Limpiar y reiniciar:             docker-compose down; docker-compose up -d"
    
    exit 1
}
