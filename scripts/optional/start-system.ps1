# ===== SCRIPT POWERSHELL PARA WINDOWS - ORDER MANAGEMENT SYSTEM =====
# Un solo comando para levantar todo el sistema en Windows

param(
    [switch]$Clean,
    [switch]$WithNginx,
    [switch]$Help
)

if ($Help) {
    Write-Host @"
🚀 Order Management System - Docker Startup Script

USAGE:
    .\start-system.ps1 [OPTIONS]

OPTIONS:
    -Clean          Limpia containers y volúmenes previos
    -WithNginx      Incluye Nginx reverse proxy
    -Help           Muestra esta ayuda

EXAMPLES:
    .\start-system.ps1                    # Inicio normal
    .\start-system.ps1 -Clean             # Limpia todo antes de iniciar
    .\start-system.ps1 -WithNginx         # Incluye nginx proxy
    .\start-system.ps1 -Clean -WithNginx  # Limpia e incluye nginx

"@
    exit 0
}

# Configuración de colores
$Host.UI.RawUI.ForegroundColor = "White"

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

try {
    Write-Host "🚀 Iniciando Order Management System con Docker..." -ForegroundColor Cyan
    Write-Host "==================================================" -ForegroundColor Cyan

    # Verificar que Docker esté corriendo
    Write-Status "Verificando Docker..."
    try {
        docker info | Out-Null
        Write-Success "Docker está corriendo ✅"
    }
    catch {
        Write-Error "Docker no está corriendo. Por favor inicia Docker Desktop."
        exit 1
    }

    # Crear directorios de logs si no existen
    Write-Status "Creando directorios de logs..."
    @("logs\customer-service", "logs\product-service", "logs\order-service", "logs\logging-service") | ForEach-Object {
        if (!(Test-Path $_)) {
            New-Item -ItemType Directory -Path $_ -Force | Out-Null
        }
    }

    # Limpiar containers y volúmenes previos si se solicita
    if ($Clean) {
        Write-Warning "Limpiando containers y volúmenes previos..."
        try {
            docker-compose down -v --remove-orphans 2>$null
            docker system prune -f 2>$null
        }
        catch {
            # Ignorar errores de limpieza
        }
    }

    # Construir y levantar servicios
    Write-Status "Construyendo imágenes y levantando servicios..."
    Write-Status "Esto puede tomar varios minutos la primera vez..."

    # Levantar infraestructura primero
    Write-Status "1/4 - Levantando infraestructura (SQL Server y RabbitMQ)..."
    docker-compose up -d sqlserver rabbitmq

    # Esperar a que la infraestructura esté lista
    Write-Status "Esperando a que la infraestructura esté lista..."
    Start-Sleep -Seconds 30

    # Verificar que SQL Server esté listo
    Write-Status "Verificando SQL Server..."
    $sqlReady = $false
    for ($i = 1; $i -le 30; $i++) {
        try {
            docker exec oms-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "OrderManagement2024!" -Q "SELECT 1" 2>$null | Out-Null
            Write-Success "SQL Server está listo ✅"
            $sqlReady = $true
            break
        }
        catch {
            if ($i -eq 30) {
                Write-Error "SQL Server no está respondiendo después de 5 minutos"
                exit 1
            }
            Write-Host "." -NoNewline
            Start-Sleep -Seconds 10
        }
    }

    # Verificar que RabbitMQ esté listo
    Write-Status "Verificando RabbitMQ..."
    $rabbitReady = $false
    for ($i = 1; $i -le 20; $i++) {
        try {
            docker exec oms-rabbitmq rabbitmq-diagnostics ping 2>$null | Out-Null
            Write-Success "RabbitMQ está listo ✅"
            $rabbitReady = $true
            break
        }
        catch {
            if ($i -eq 20) {
                Write-Error "RabbitMQ no está respondiendo después de 3 minutos"
                exit 1
            }
            Write-Host "." -NoNewline
            Start-Sleep -Seconds 10
        }
    }

    # Levantar servicios backend
    Write-Status "2/4 - Levantando servicios backend..."
    docker-compose up -d customer-service product-service logging-service
    Start-Sleep -Seconds 20

    # Levantar order service (depende de los otros servicios)
    Write-Status "3/4 - Levantando Order Service..."
    docker-compose up -d order-service
    Start-Sleep -Seconds 15

    # Levantar frontend
    Write-Status "4/4 - Levantando Frontend..."
    docker-compose up -d frontend

    # Opcionalmente levantar nginx
    if ($WithNginx) {
        Write-Status "Levantando Nginx reverse proxy..."
        docker-compose up -d nginx
    }

    # Verificar estado de todos los servicios
    Write-Status "Verificando estado de los servicios..."
    Start-Sleep -Seconds 10

    # Función para verificar health de un servicio
    function Test-ServiceHealth {
        param(
            [string]$ServiceName,
            [int]$Port
        )
        
        for ($i = 1; $i -le 10; $i++) {
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:$Port/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
                if ($response.StatusCode -eq 200) {
                    Write-Success "$ServiceName está saludable ✅"
                    return $true
                }
            }
            catch {
                Start-Sleep -Seconds 5
            }
        }
        Write-Warning "$ServiceName no responde en el puerto $Port ⚠️"
        return $false
    }

    # Verificar servicios backend
    Test-ServiceHealth "Customer Service" 5003
    Test-ServiceHealth "Product Service" 5002
    Test-ServiceHealth "Order Service" 5001
    Test-ServiceHealth "Logging Service" 5004

    # Verificar frontend
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:3000" -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-Success "Frontend está disponible ✅"
    }
    catch {
        Write-Warning "Frontend no responde en el puerto 3000 ⚠️"
    }

    Write-Host ""
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Success "🎉 Order Management System iniciado exitosamente!"
    Write-Host "==================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📍 URLs disponibles:" -ForegroundColor Yellow
    Write-Host "   Frontend:              http://localhost:3000"
    Write-Host "   Customer Service API:  http://localhost:5003/swagger"
    Write-Host "   Product Service API:   http://localhost:5002/swagger"
    Write-Host "   Order Service API:     http://localhost:5001/swagger"
    Write-Host "   Logging Service API:   http://localhost:5004/swagger"
    Write-Host "   RabbitMQ Management:   http://localhost:15672 (admin/OrderManagement2024!)"
    Write-Host ""
    
    if ($WithNginx) {
        Write-Host "   Nginx Proxy:           http://localhost"
        Write-Host ""
    }
    
    Write-Host "📋 Comandos útiles:" -ForegroundColor Yellow
    Write-Host "   Ver logs en tiempo real:    docker-compose logs -f"
    Write-Host "   Ver estado de servicios:    docker-compose ps"
    Write-Host "   Parar todos los servicios:  docker-compose down"
    Write-Host "   Limpiar todo:               docker-compose down -v --remove-orphans"
    Write-Host ""
    Write-Status "Para ver logs en tiempo real ejecuta: docker-compose logs -f"

}
catch {
    Write-Error "Error durante el inicio del sistema: $($_.Exception.Message)"
    Write-Host "Para más detalles ejecuta: docker-compose logs" -ForegroundColor Yellow
    exit 1
}
