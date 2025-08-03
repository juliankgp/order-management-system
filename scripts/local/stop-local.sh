#!/bin/bash

# =============================================================================
# 🛑 Order Management System - Local Development Stop Script
# =============================================================================
# Este script detiene todos los servicios que fueron iniciados localmente
# =============================================================================

set -e

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
WHITE='\033[1;37m'
NC='\033[0m' # No Color

# Función para mostrar encabezado
show_header() {
    echo -e "\n${CYAN}🛑 Deteniendo Order Management System Local...${NC}"
    echo -e "${CYAN}===============================================${NC}"
}

# Función para detener servicio por PID
stop_service_by_pid() {
    local service_name=$1
    local pid_file="logs/${service_name,,}.pid"
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        
        if ps -p $pid > /dev/null 2>&1; then
            echo -e "${BLUE}🛑 Deteniendo $service_name (PID: $pid)...${NC}"
            kill $pid 2>/dev/null || true
            
            # Esperar que el proceso termine
            sleep 2
            
            # Si aún está corriendo, forzar terminación
            if ps -p $pid > /dev/null 2>&1; then
                echo -e "${YELLOW}⚠️  Forzando terminación de $service_name...${NC}"
                kill -9 $pid 2>/dev/null || true
            fi
            
            echo -e "${GREEN}✅ $service_name detenido${NC}"
        else
            echo -e "${YELLOW}⚠️  $service_name ya no está corriendo${NC}"
        fi
        
        # Limpiar archivo PID
        rm -f "$pid_file"
    else
        echo -e "${YELLOW}⚠️  No se encontró archivo PID para $service_name${NC}"
    fi
}

# Función para detener servicios por puerto
stop_services_by_port() {
    local ports=(5001 5002 5003 5004 3000)
    local service_names=("OrderService" "ProductService" "CustomerService" "LoggingService" "Frontend")
    
    echo -e "\n${BLUE}🔍 Buscando procesos por puerto...${NC}"
    
    for i in "${!ports[@]}"; do
        local port=${ports[$i]}
        local service_name=${service_names[$i]}
        
        # Buscar proceso usando el puerto (funciona en Windows con bash)
        local pid=$(netstat -ano | grep ":$port " | grep "LISTENING" | awk '{print $5}' | head -1)
        
        if [ ! -z "$pid" ] && [ "$pid" != "0" ]; then
            echo -e "${BLUE}🛑 Deteniendo $service_name en puerto $port (PID: $pid)...${NC}"
            taskkill //PID $pid //F 2>/dev/null || kill -9 $pid 2>/dev/null || true
            echo -e "${GREEN}✅ Proceso en puerto $port detenido${NC}"
        else
            echo -e "${YELLOW}⚠️  No se encontró proceso en puerto $port${NC}"
        fi
    done
}

# Función para limpiar archivos temporales
cleanup_temp_files() {
    echo -e "\n${BLUE}🧹 Limpiando archivos temporales...${NC}"
    
    # Limpiar PIDs
    rm -f logs/*.pid 2>/dev/null || true
    
    # Rotar logs (mover a archivo con timestamp)
    if [ -d "logs" ]; then
        local timestamp=$(date +"%Y%m%d_%H%M%S")
        
        for log_file in logs/*.log; do
            if [ -f "$log_file" ]; then
                local base_name=$(basename "$log_file" .log)
                mv "$log_file" "logs/${base_name}_${timestamp}.log" 2>/dev/null || true
            fi
        done
        
        echo -e "${GREEN}✅ Logs archivados con timestamp${NC}"
    fi
}

# Función para mostrar servicios aún corriendo
check_remaining_services() {
    echo -e "\n${BLUE}🔍 Verificando servicios restantes...${NC}"
    
    local ports=(5001 5002 5003 5004 3000)
    local service_names=("OrderService" "ProductService" "CustomerService" "LoggingService" "Frontend")
    local running_services=0
    
    for i in "${!ports[@]}"; do
        local port=${ports[$i]}
        local service_name=${service_names[$i]}
        
        if curl -s "http://localhost:$port" > /dev/null 2>&1; then
            echo -e "${RED}❌ $service_name aún responde en puerto $port${NC}"
            ((running_services++))
        fi
    done
    
    if [ $running_services -eq 0 ]; then
        echo -e "${GREEN}✅ Todos los servicios han sido detenidos${NC}"
    else
        echo -e "${YELLOW}⚠️  $running_services servicio(s) aún están corriendo${NC}"
    fi
}

# Función para mostrar información final
show_final_info() {
    echo -e "\n${WHITE}✅ SERVICIOS LOCALES DETENIDOS${NC}"
    echo -e "${WHITE}===============================${NC}"
    
    echo -e "\n${YELLOW}📋 Para ver logs archivados:${NC}"
    echo -e "${YELLOW}   ls -la logs/*.log${NC}"
    
    echo -e "\n${YELLOW}📋 Para iniciar nuevamente:${NC}"
    echo -e "${YELLOW}   bash start-local.sh${NC}"
    
    echo -e "\n${YELLOW}📋 Para usar Docker en su lugar:${NC}"
    echo -e "${YELLOW}   bash start-oms.sh${NC}"
    
    echo -e "\n${GREEN}✨ ¡Hasta la próxima! ✨${NC}"
}

# Función para cleanup forzado (opción --force)
force_cleanup() {
    echo -e "\n${RED}⚠️  LIMPIEZA FORZADA ACTIVADA${NC}"
    echo -e "${RED}=============================${NC}"
    
    # Matar todos los procesos dotnet y node relacionados
    echo -e "${BLUE}🛑 Deteniendo todos los procesos dotnet...${NC}"
    pkill -f "dotnet.*Api" 2>/dev/null || true
    
    echo -e "${BLUE}🛑 Deteniendo todos los procesos npm/node...${NC}"
    pkill -f "node.*vite" 2>/dev/null || true
    pkill -f "npm.*dev" 2>/dev/null || true
    
    # En Windows con bash
    taskkill //IM "dotnet.exe" //F 2>/dev/null || true
    taskkill //IM "node.exe" //F 2>/dev/null || true
    
    echo -e "${GREEN}✅ Limpieza forzada completada${NC}"
}

# =============================================================================
# FUNCIÓN PRINCIPAL
# =============================================================================
main() {
    local force_mode=false
    
    # Verificar argumentos
    if [[ "$1" == "--force" || "$1" == "-f" ]]; then
        force_mode=true
    fi
    
    show_header
    
    if [ "$force_mode" = true ]; then
        force_cleanup
    else
        # Detener servicios de manera ordenada
        echo -e "\n${BLUE}🛑 Deteniendo servicios por PID...${NC}"
        stop_service_by_pid "CustomerService"
        stop_service_by_pid "ProductService"
        stop_service_by_pid "LoggingService"
        stop_service_by_pid "OrderService"
        stop_service_by_pid "Frontend"
        
        # Verificar por puerto como backup
        stop_services_by_port
    fi
    
    cleanup_temp_files
    check_remaining_services
    show_final_info
}

# Verificar si se está ejecutando el script directamente
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi
