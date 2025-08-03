#!/bin/bash

# Script para verificar y reiniciar RabbitMQ si es necesario
# Colores para mejor visualización
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

check_rabbitmq() {
    echo -e "${BLUE}🔍 Verificando estado de RabbitMQ...${NC}"
    
    # Verificar si el contenedor existe
    if docker ps -a | grep -q "rabbitmq-local"; then
        # Verificar si está corriendo
        if docker ps | grep -q "rabbitmq-local"; then
            echo -e "${GREEN}✅ Contenedor RabbitMQ está corriendo${NC}"
            
            # Verificar si responde en el puerto de management
            if curl -s http://localhost:15672 >/dev/null 2>&1; then
                echo -e "${GREEN}✅ RabbitMQ Management UI responde${NC}"
                
                # Verificar conexión AMQP básica
                if timeout 5 bash -c '</dev/tcp/localhost/5672' >/dev/null 2>&1; then
                    echo -e "${GREEN}✅ Puerto AMQP (5672) accesible${NC}"
                    echo -e "${GREEN}🎉 RabbitMQ está completamente funcional${NC}"
                    return 0
                else
                    echo -e "${YELLOW}⚠️  Puerto AMQP no responde${NC}"
                    return 1
                fi
            else
                echo -e "${YELLOW}⚠️  Management UI no responde${NC}"
                return 1
            fi
        else
            echo -e "${YELLOW}⚠️  Contenedor existe pero no está corriendo${NC}"
            return 1
        fi
    else
        echo -e "${YELLOW}⚠️  Contenedor RabbitMQ no existe${NC}"
        return 1
    fi
}

restart_rabbitmq() {
    echo -e "${BLUE}🔄 Reiniciando RabbitMQ...${NC}"
    
    # Detener y eliminar contenedor existente
    docker stop rabbitmq-local 2>/dev/null || true
    docker rm rabbitmq-local 2>/dev/null || true
    
    # Crear nuevo contenedor con configuración correcta
    echo -e "${BLUE}🚀 Iniciando nuevo contenedor RabbitMQ...${NC}"
    docker run -d --name rabbitmq-local \
        -p 5672:5672 \
        -p 15672:15672 \
        -e RABBITMQ_DEFAULT_USER=guest \
        -e RABBITMQ_DEFAULT_PASS=guest \
        rabbitmq:3-management
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ Contenedor iniciado exitosamente${NC}"
        
        echo -e "${BLUE}⏳ Esperando que RabbitMQ se inicialice completamente...${NC}"
        sleep 25
        
        # Verificar que esté funcionando
        if check_rabbitmq; then
            echo -e "${GREEN}🎉 RabbitMQ reiniciado exitosamente${NC}"
            return 0
        else
            echo -e "${RED}❌ RabbitMQ no respondió después del reinicio${NC}"
            return 1
        fi
    else
        echo -e "${RED}❌ Error al iniciar contenedor RabbitMQ${NC}"
        return 1
    fi
}

# Función principal
main() {
    echo -e "${BLUE}🐰 Verificador de RabbitMQ para Order Management System${NC}"
    echo -e "${BLUE}====================================================${NC}\n"
    
    if check_rabbitmq; then
        echo -e "\n${GREEN}✅ RabbitMQ está funcionando correctamente. No se requiere acción.${NC}"
        exit 0
    else
        echo -e "\n${YELLOW}⚠️  Problemas detectados con RabbitMQ. Intentando reiniciar...${NC}"
        
        if restart_rabbitmq; then
            echo -e "\n${GREEN}✅ RabbitMQ ha sido reparado exitosamente.${NC}"
            echo -e "${GREEN}📊 URLs disponibles:${NC}"
            echo -e "${GREEN}   • Management UI: http://localhost:15672${NC}"
            echo -e "${GREEN}   • AMQP Port: localhost:5672${NC}"
            echo -e "${GREEN}   • Usuario: guest / Contraseña: guest${NC}"
            exit 0
        else
            echo -e "\n${RED}❌ No se pudo reparar RabbitMQ automáticamente.${NC}"
            echo -e "${RED}💡 Acciones manuales recomendadas:${NC}"
            echo -e "${RED}   1. Verificar que Docker esté funcionando${NC}"
            echo -e "${RED}   2. Verificar que los puertos 5672 y 15672 estén libres${NC}"
            echo -e "${RED}   3. Revisar logs: docker logs rabbitmq-local${NC}"
            exit 1
        fi
    fi
}

# Ejecutar función principal
main "$@"
