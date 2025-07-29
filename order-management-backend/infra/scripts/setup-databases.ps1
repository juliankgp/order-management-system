# Script para configurar las bases de datos del sistema de gestión de órdenes
# Ejecutar como administrador

Write-Host "=== Order Management System - Database Setup ===" -ForegroundColor Green
Write-Host ""

# Verificar si SQL Server está disponible
Write-Host "Verificando conectividad con SQL Server..." -ForegroundColor Yellow

try {
    $connectionString = "Server=localhost;Integrated Security=true;TrustServerCertificate=true;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    Write-Host "✓ SQL Server está disponible" -ForegroundColor Green
}
catch {
    Write-Host "✗ Error conectando a SQL Server: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Asegúrate de que SQL Server esté instalado y ejecutándose" -ForegroundColor Yellow
    exit 1
}

# Lista de bases de datos a crear
$databases = @(
    "OrderManagement_Orders",
    "OrderManagement_Products", 
    "OrderManagement_Customers",
    "OrderManagement_Logs"
)

Write-Host ""
Write-Host "Creando bases de datos..." -ForegroundColor Yellow

foreach ($dbName in $databases) {
    try {
        Write-Host "Creando base de datos: $dbName" -ForegroundColor Cyan
        
        $createDbQuery = @"
        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '$dbName')
        BEGIN
            CREATE DATABASE [$dbName]
            PRINT 'Base de datos $dbName creada exitosamente'
        END
        ELSE
        BEGIN
            PRINT 'Base de datos $dbName ya existe'
        END
"@
        
        # Ejecutar la query
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $command = New-Object System.Data.SqlClient.SqlCommand($createDbQuery, $connection)
        $connection.Open()
        $result = $command.ExecuteNonQuery()
        $connection.Close()
        
        Write-Host "✓ $dbName configurada correctamente" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Error creando $dbName`: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Configuración de RabbitMQ ===" -ForegroundColor Green

# Verificar si RabbitMQ está instalado
$rabbitMqService = Get-Service -Name "RabbitMQ" -ErrorAction SilentlyContinue

if ($rabbitMqService) {
    if ($rabbitMqService.Status -eq "Running") {
        Write-Host "✓ RabbitMQ está ejecutándose" -ForegroundColor Green
    }
    else {
        Write-Host "Iniciando servicio RabbitMQ..." -ForegroundColor Yellow
        Start-Service -Name "RabbitMQ"
        Write-Host "✓ RabbitMQ iniciado" -ForegroundColor Green
    }
}
else {
    Write-Host "⚠ RabbitMQ no está instalado" -ForegroundColor Yellow
    Write-Host "Opciones para instalar RabbitMQ:" -ForegroundColor Cyan
    Write-Host "1. Docker: docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management" -ForegroundColor White
    Write-Host "2. Instalación local: https://www.rabbitmq.com/download.html" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Configuración Variables de Entorno ===" -ForegroundColor Green

# Configurar variables de entorno para desarrollo
$envVars = @{
    "OrderManagement_ConnectionString" = "Server=localhost;Integrated Security=true;TrustServerCertificate=true;"
    "OrderManagement_RabbitMQ_Host" = "localhost"
    "OrderManagement_RabbitMQ_User" = "guest"
    "OrderManagement_RabbitMQ_Password" = "guest"
}

foreach ($key in $envVars.Keys) {
    try {
        [Environment]::SetEnvironmentVariable($key, $envVars[$key], "User")
        Write-Host "✓ Variable de entorno configurada: $key" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Error configurando variable $key`: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Configuración completada ===" -ForegroundColor Green
Write-Host ""
Write-Host "Próximos pasos:" -ForegroundColor Cyan
Write-Host "1. Ejecutar migraciones de Entity Framework en cada servicio" -ForegroundColor White
Write-Host "2. Ejecutar los microservicios:" -ForegroundColor White
Write-Host "   - dotnet run --project services/OrderService/src/Web" -ForegroundColor Gray
Write-Host "   - dotnet run --project services/ProductService/src/Web" -ForegroundColor Gray
Write-Host "   - dotnet run --project services/CustomerService/src/Web" -ForegroundColor Gray
Write-Host "   - dotnet run --project services/LoggingService/src/Web" -ForegroundColor Gray
Write-Host ""
Write-Host "URLs de los servicios:" -ForegroundColor Cyan
Write-Host "- Order Service: https://localhost:5001" -ForegroundColor White
Write-Host "- Product Service: https://localhost:5002" -ForegroundColor White
Write-Host "- Customer Service: https://localhost:5003" -ForegroundColor White
Write-Host "- Logging Service: https://localhost:5004" -ForegroundColor White
Write-Host "- RabbitMQ Management: http://localhost:15672 (guest/guest)" -ForegroundColor White
