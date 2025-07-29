# Script para ejecutar migraciones de Entity Framework
# Ejecutar desde la raíz del proyecto

Write-Host "=== Order Management System - Database Migrations ===" -ForegroundColor Green
Write-Host ""

$services = @(
    @{
        Name = "OrderService"
        Path = "services/OrderService/src/Web"
        Context = "OrderDbContext"
    },
    @{
        Name = "ProductService"
        Path = "services/ProductService/src/Web"
        Context = "ProductDbContext"
    },
    @{
        Name = "CustomerService"
        Path = "services/CustomerService/src/Web"
        Context = "CustomerDbContext"
    },
    @{
        Name = "LoggingService"
        Path = "services/LoggingService/src/Web"
        Context = "LoggingDbContext"
    }
)

foreach ($service in $services) {
    Write-Host "Ejecutando migraciones para $($service.Name)..." -ForegroundColor Yellow
    
    try {
        # Navegar al directorio del servicio
        $originalPath = Get-Location
        Set-Location $service.Path
        
        # Verificar si existe el proyecto
        if (-not (Test-Path "*.csproj")) {
            Write-Host "✗ No se encontró el archivo de proyecto en $($service.Path)" -ForegroundColor Red
            Set-Location $originalPath
            continue
        }
        
        # Crear migración inicial si no existe
        Write-Host "  Creando migración inicial..." -ForegroundColor Cyan
        $addMigrationOutput = dotnet ef migrations add InitialCreate --context $service.Context 2>&1
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠ La migración ya existe o hubo un error: $addMigrationOutput" -ForegroundColor Yellow
        } else {
            Write-Host "  ✓ Migración creada exitosamente" -ForegroundColor Green
        }
        
        # Actualizar base de datos
        Write-Host "  Aplicando migraciones a la base de datos..." -ForegroundColor Cyan
        $updateDbOutput = dotnet ef database update --context $service.Context 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ Base de datos actualizada exitosamente" -ForegroundColor Green
        } else {
            Write-Host "  ✗ Error actualizando base de datos: $updateDbOutput" -ForegroundColor Red
        }
        
        # Volver al directorio original
        Set-Location $originalPath
        
        Write-Host ""
    }
    catch {
        Write-Host "✗ Error procesando $($service.Name): $($_.Exception.Message)" -ForegroundColor Red
        Set-Location $originalPath
    }
}

Write-Host "=== Migraciones completadas ===" -ForegroundColor Green
Write-Host ""
Write-Host "Para verificar las migraciones ejecutadas, puedes usar:" -ForegroundColor Cyan
Write-Host "dotnet ef migrations list --context [ContextName]" -ForegroundColor White
Write-Host ""
Write-Host "Para crear una nueva migración:" -ForegroundColor Cyan
Write-Host "dotnet ef migrations add [MigrationName] --context [ContextName]" -ForegroundColor White
