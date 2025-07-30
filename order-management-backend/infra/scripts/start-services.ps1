# Script para ejecutar todos los microservicios en paralelo
# Ejecutar desde la raíz del proyecto backend

Write-Host "=== Order Management System - Startup Script ===" -ForegroundColor Green
Write-Host ""

# Verificar que dotnet esté disponible
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK detectado: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "✗ .NET SDK no está instalado o no está en el PATH" -ForegroundColor Red
    exit 1
}

# Lista de servicios a ejecutar
$services = @(
    @{
        Name = "OrderService"
        Path = "services/OrderService/src/Api"
        Port = "5001"
        Url = "https://localhost:5001"
    },
    @{
        Name = "ProductService"
        Path = "services/ProductService/src/Api"
        Port = "5002"
        Url = "https://localhost:5002"
    },
    @{
        Name = "CustomerService"
        Path = "services/CustomerService/src/Api"
        Port = "5003"
        Url = "https://localhost:5003"
    },
    @{
        Name = "LoggingService"
        Path = "services/LoggingService/src/Api"
        Port = "5004"
        Url = "https://localhost:5004"
    }
)

# Array para almacenar los jobs
$jobs = @()

Write-Host "Iniciando microservicios..." -ForegroundColor Yellow
Write-Host ""

foreach ($service in $services) {
    if (Test-Path $service.Path) {
        Write-Host "Iniciando $($service.Name) en puerto $($service.Port)..." -ForegroundColor Cyan
        
        # Crear un script block para ejecutar el servicio
        $scriptBlock = {
            param($servicePath, $serviceName)
            Set-Location $servicePath
            dotnet run
        }
        
        # Iniciar el servicio como job en background
        $job = Start-Job -ScriptBlock $scriptBlock -ArgumentList $service.Path, $service.Name -Name $service.Name
        $jobs += $job
        
        Write-Host "✓ $($service.Name) iniciado (Job ID: $($job.Id))" -ForegroundColor Green
    }
    else {
        Write-Host "✗ No se encontró el path para $($service.Name): $($service.Path)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Servicios en ejecución ===" -ForegroundColor Green

# Mostrar URLs de los servicios
foreach ($service in $services) {
    Write-Host "$($service.Name): $($service.Url)" -ForegroundColor White
}

Write-Host ""
Write-Host "URLs adicionales:" -ForegroundColor Cyan
Write-Host "- RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
Write-Host "- Swagger UI disponible en /swagger en cada servicio" -ForegroundColor White

Write-Host ""
Write-Host "=== Comandos útiles ===" -ForegroundColor Green
Write-Host "Para ver el estado de los servicios:" -ForegroundColor Cyan
Write-Host "Get-Job" -ForegroundColor White
Write-Host ""
Write-Host "Para ver los logs de un servicio:" -ForegroundColor Cyan
Write-Host "Receive-Job -Name [ServiceName] -Keep" -ForegroundColor White
Write-Host ""
Write-Host "Para detener un servicio:" -ForegroundColor Cyan
Write-Host "Stop-Job -Name [ServiceName]" -ForegroundColor White
Write-Host ""
Write-Host "Para detener todos los servicios:" -ForegroundColor Cyan
Write-Host "Get-Job | Stop-Job" -ForegroundColor White

Write-Host ""
Write-Host "Presiona Ctrl+C para detener todos los servicios y salir" -ForegroundColor Yellow

# Mantener el script corriendo y mostrar estado cada 30 segundos
try {
    while ($true) {
        Start-Sleep -Seconds 30
        
        Write-Host ""
        Write-Host "Estado de servicios a las $(Get-Date -Format 'HH:mm:ss'):" -ForegroundColor Gray
        
        foreach ($job in $jobs) {
            $status = Get-Job -Id $job.Id
            $statusColor = switch ($status.State) {
                "Running" { "Green" }
                "Completed" { "Yellow" }
                "Failed" { "Red" }
                "Stopped" { "Gray" }
                default { "White" }
            }
            Write-Host "  $($job.Name): $($status.State)" -ForegroundColor $statusColor
        }
    }
}
catch [System.Management.Automation.PipelineStoppedException] {
    Write-Host ""
    Write-Host "Deteniendo todos los servicios..." -ForegroundColor Yellow
    
    # Detener todos los jobs
    $jobs | Stop-Job
    $jobs | Remove-Job -Force
    
    Write-Host "✓ Todos los servicios han sido detenidos" -ForegroundColor Green
}
