# Guía de Solución de Problemas - RabbitMQ

Esta guía describe los problemas comunes con RabbitMQ y sus soluciones implementadas en el sistema.

## ❌ Problema Identificado

**Error común**: `ACCESS_REFUSED - Login was refused using authentication mechanism PLAIN`

**Síntomas**:
- LoggingService falla al iniciar
- Errores de conexión en los logs
- Servicios que publican eventos no pueden conectarse

**Causa raíz**: 
RabbitMQ en Docker no tenía las credenciales por defecto configuradas correctamente.

## ✅ Soluciones Implementadas

### 1. **Script de Inicio Mejorado** (`start-local.sh`)
- ✅ **Auto-detección** de problemas con RabbitMQ
- ✅ **Inicio automático** de RabbitMQ si no está disponible
- ✅ **Configuración correcta** con variables de entorno explícitas
- ✅ **Validación** de que el servicio esté funcionando antes de continuar

### 2. **LoggingService Robusto**
- ✅ **Reintentos automáticos** (5 intentos con backoff exponencial)
- ✅ **Inicio sin RabbitMQ** si no está disponible
- ✅ **Logging detallado** para diagnóstico
- ✅ **Reconexión automática** con configuración de recovery

### 3. **Validación en Servicios**
- ✅ **CustomerService**: Ya tenía validación de conexión
- ✅ **OrderService**: Agregada validación antes de publicar
- ✅ **ProductService**: Agregada validación antes de publicar
- ✅ **LoggingService**: Mejorado manejo de errores

### 4. **Script de Verificación** (`check-rabbitmq.sh`)
- ✅ **Diagnóstico automático** del estado de RabbitMQ
- ✅ **Reparación automática** si hay problemas
- ✅ **Validación completa** (contenedor, management UI, puerto AMQP)

## 🔧 Comando de RabbitMQ Correcto

```bash
# Comando correcto para iniciar RabbitMQ localmente
docker run -d --name rabbitmq-local \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3-management
```

**Variables de entorno importantes**:
- `RABBITMQ_DEFAULT_USER=guest`: Usuario por defecto
- `RABBITMQ_DEFAULT_PASS=guest`: Contraseña por defecto

## 🚀 Uso de Scripts

### Inicio Normal
```bash
# El script ahora maneja RabbitMQ automáticamente
bash start-local.sh
```

### Verificación Manual de RabbitMQ
```bash
# Para diagnosticar problemas específicos con RabbitMQ
bash check-rabbitmq.sh
```

### Parar Todo
```bash
# Detiene todos los servicios incluyendo RabbitMQ
bash stop-local.sh
```

## 🔍 Diagnóstico de Problemas

### Verificar Estado
```bash
# Verificar contenedor
docker ps | grep rabbitmq

# Verificar logs
docker logs rabbitmq-local

# Verificar conectividad
curl http://localhost:15672
```

### URLs de Verificación
- **Management UI**: http://localhost:15672
- **Health Check LoggingService**: http://localhost:5004/health
- **Credenciales**: guest / guest

## 📊 Beneficios de las Mejoras

### ✅ **Prevención**
- Script detecta y corrige problemas automáticamente
- Configuración explícita evita problemas de autenticación
- Validaciones previenen fallos en runtime

### ✅ **Robustez**
- Servicios continúan funcionando sin RabbitMQ
- Reconexión automática cuando RabbitMQ vuelve
- Logs detallados para diagnóstico

### ✅ **Experiencia de Desarrollo**
- Inicio más confiable del entorno local
- Menos interrupciones por problemas de infraestructura
- Diagnóstico automatizado

## ⚠️ Puntos Importantes

1. **Orden de Inicio**: RabbitMQ debe iniciarse antes que LoggingService
2. **Tiempo de Espera**: RabbitMQ tarda ~20-25 segundos en estar completamente listo
3. **Puertos**: Asegúrate de que 5672 y 15672 estén libres
4. **Docker**: Requiere Docker funcionando para RabbitMQ

## 🔄 Próximos Pasos

Para hacer el sistema aún más robusto, considera:

1. **Configuración de Producción**: Variables de entorno específicas para cada ambiente
2. **Monitoreo**: Health checks más sofisticados
3. **Persistencia**: Configurar volúmenes para datos de RabbitMQ
4. **Clustering**: Para alta disponibilidad en producción
