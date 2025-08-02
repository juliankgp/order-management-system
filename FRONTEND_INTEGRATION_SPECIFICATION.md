# Especificación Técnica para Desarrollo Frontend - Order Management System

## Resumen Ejecutivo

Este documento proporciona una especificación técnica completa del backend del Order Management System para facilitar el desarrollo del frontend. El sistema está construido con una arquitectura de microservicios usando .NET 8, implementando Clean Architecture con patrón CQRS, bases de datos separadas por servicio y comunicación event-driven vía RabbitMQ.

### Arquitectura General

- **OrderService** (Puerto 5001): Gestión del ciclo de vida de órdenes, operaciones CRUD
- **ProductService** (Puerto 5002): Catálogo de productos e inventario
- **CustomerService** (Puerto 5003): Gestión de clientes y autenticación
- **LoggingService** (Puerto 5004): Logging centralizado y auditoría

### URLs de Desarrollo

- OrderService: https://localhost:5001/swagger
- ProductService: https://localhost:5002/swagger  
- CustomerService: https://localhost:5003/swagger
- LoggingService: https://localhost:5004/swagger

---

## Tabla de Endpoints

### CustomerService (Puerto 5003)

| Método | Ruta | Autenticación | Descripción |
|--------|------|---------------|-------------|
| POST | `/api/customers/register` | No | Registro de nuevo cliente |
| POST | `/api/customers/login` | No | Autenticación de cliente |
| GET | `/api/customers` | Sí | Lista de clientes con filtros |
| GET | `/api/customers/{id}` | Sí | Obtener cliente específico |
| GET | `/api/customers/profile` | Sí | Perfil del cliente autenticado |
| PUT | `/api/customers/profile` | Sí | Actualizar perfil del cliente |
| GET | `/api/customers/test` | No | Verificación de salud del servicio |
| GET | `/api/customers/jwt-debug` | No | Información de debug JWT |

### OrderService (Puerto 5001)

| Método | Ruta | Autenticación | Descripción |
|--------|------|---------------|-------------|
| POST | `/api/orders` | Sí | Crear nueva orden |
| GET | `/api/orders` | Sí | Lista de órdenes con filtros |
| GET | `/api/orders/{id}` | Sí | Obtener orden específica |
| PUT | `/api/orders/{id}` | Sí | Actualizar orden existente |
| DELETE | `/api/orders/{id}` | Sí | Eliminar/cancelar orden |
| GET | `/api/orders/test` | No | Verificación de salud |
| GET | `/api/orders/jwt-debug` | No | Debug JWT |
| GET | `/api/orders/health` | No | Estado de salud |

### ProductService (Puerto 5002)  

| Método | Ruta | Autenticación | Descripción |
|--------|------|---------------|-------------|
| GET | `/api/products` | Sí | Lista de productos con filtros |
| GET | `/api/products/{id}` | Sí | Obtener producto específico |
| GET | `/api/products/{id}/validate` | Sí | Validar existencia de producto |
| POST | `/api/products/validate-stock` | Sí | Validar disponibilidad de stock |
| POST | `/api/products/batch` | Sí | Obtener múltiples productos por IDs |
| GET | `/api/products/test` | No | Verificación de salud |
| GET | `/api/products/health` | No | Estado de salud |

### LoggingService (Puerto 5004)

| Método | Ruta | Autenticación | Descripción |
|--------|------|---------------|-------------|
| GET | `/api/logs` | Sí | Lista de logs con paginación |
| GET | `/api/logs/search` | Sí | Buscar logs con filtros |
| GET | `/api/logs/service/{serviceName}` | Sí | Logs por nombre de servicio |
| GET | `/api/logs/correlation/{correlationId}` | Sí | Logs por correlation ID |
| GET | `/api/logs/user/{userId}` | Sí | Logs por usuario |
| POST | `/api/logs` | Sí | Crear nueva entrada de log |
| GET | `/api/logs/health` | No | Verificación de salud |

---

## Ejemplos de Request y Response

### 1. Registro de Cliente

**Request:**
```http
POST /api/customers/register
Content-Type: application/json

{
  "email": "juan.perez@example.com",
  "password": "SecurePass123!",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phoneNumber": "+52-555-0123",
  "dateOfBirth": "1990-05-15",
  "gender": 1
}
```

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174001",
  "email": "juan.perez@example.com",
  "fullName": "Juan Pérez",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpires": "2025-08-02T10:30:00Z",
  "emailVerified": false
}
```

### 2. Login de Cliente

**Request:**
```http
POST /api/customers/login
Content-Type: application/json

{
  "email": "juan.perez@example.com",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174001",
  "email": "juan.perez@example.com",
  "fullName": "Juan Pérez",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenExpires": "2025-08-02T10:30:00Z",
  "emailVerified": false
}
```

### 3. Crear Orden

**Request:**
```http
POST /api/orders
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "customerId": "123e4567-e89b-12d3-a456-426614174001",
  "notes": "Entrega urgente",
  "items": [
    {
      "productId": "123e4567-e89b-12d3-a456-426614174003",
      "quantity": 2
    },
    {
      "productId": "123e4567-e89b-12d3-a456-426614174004",
      "quantity": 1
    }
  ]
}
```

**Response (201 Created):**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "customerId": "123e4567-e89b-12d3-a456-426614174001",
  "orderNumber": "ORD-20250801-A1B2C3D4",
  "status": "Pending",
  "orderDate": "2025-08-01T10:30:00Z",
  "totalAmount": 159.99,
  "subTotal": 149.99,
  "taxAmount": 15.00,
  "shippingCost": 0.00,
  "notes": "Entrega urgente",
  "createdAt": "2025-08-01T10:30:00Z",
  "updatedAt": "2025-08-01T10:30:00Z",
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174002",
      "productId": "123e4567-e89b-12d3-a456-426614174003",
      "productName": "Laptop Computer",
      "quantity": 2,
      "unitPrice": 999.99,
      "subtotal": 1999.98
    }
  ]
}
```

### 4. Lista de Productos

**Request:**
```http
GET /api/products?page=1&pageSize=10&category=Electronics&searchTerm=laptop&isActive=true
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174003",
      "name": "Laptop Computer",
      "description": "Laptop de alto rendimiento para profesionales",
      "sku": "LAPTOP-001",
      "price": 999.99,
      "stock": 25,
      "minimumStock": 5,
      "category": "Electronics",
      "brand": "TechBrand",
      "weight": 2500.0,
      "dimensions": "35x25x2",
      "imageUrl": "https://example.com/images/laptop-001.jpg",
      "isActive": true,
      "tags": "laptop,computer,electronics",
      "createdAt": "2025-01-10T08:00:00Z",
      "updatedAt": "2025-08-01T10:30:00Z"
    }
  ],
  "totalCount": 1,
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPrevious": false,
  "hasNext": false
}
```

---

## Esquemas de Modelos de Datos

### CustomerDto

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| id | Guid | Sí | Identificador único del cliente |
| email | string | Sí | Email único del cliente |
| firstName | string | Sí | Nombre del cliente |
| lastName | string | Sí | Apellido del cliente |
| fullName | string | Sí | Nombre completo (calculado) |
| phoneNumber | string | No | Número de teléfono |
| dateOfBirth | DateTime | No | Fecha de nacimiento |
| gender | Gender | No | Género (1=Male, 2=Female, 3=Other, 4=PreferNotToSay) |
| isActive | boolean | Sí | Estado del cliente |
| emailVerified | boolean | Sí | Estado de verificación del email |
| emailVerifiedAt | DateTime | No | Fecha de verificación del email |
| lastLoginAt | DateTime | No | Último login |
| preferences | string | No | Preferencias del cliente (JSON) |
| createdAt | DateTime | Sí | Fecha de creación |
| updatedAt | DateTime | Sí | Fecha de última actualización |
| addresses | CustomerAddressDto[] | Sí | Lista de direcciones |

### ProductDto

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| id | Guid | Sí | Identificador único del producto |
| name | string | Sí | Nombre del producto |
| description | string | No | Descripción del producto |
| sku | string | Sí | SKU único del producto |
| price | decimal | Sí | Precio unitario |
| stock | int | Sí | Cantidad en inventario |
| minimumStock | int | Sí | Stock mínimo |
| category | string | Sí | Categoría del producto |
| brand | string | No | Marca del producto |
| weight | decimal | No | Peso en gramos |
| dimensions | string | No | Dimensiones LxWxH en cm |
| imageUrl | string | No | URL de la imagen |
| isActive | boolean | Sí | Estado del producto |
| tags | string | No | Tags separados por comas |
| createdAt | DateTime | Sí | Fecha de creación |
| updatedAt | DateTime | Sí | Fecha de última actualización |

### OrderDto

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| id | Guid | Sí | Identificador único de la orden |
| customerId | Guid | Sí | ID del cliente |
| orderNumber | string | Sí | Número único de orden |
| status | string | Sí | Estado de la orden |
| orderDate | DateTime | Sí | Fecha de la orden |
| totalAmount | decimal | Sí | Monto total |
| subTotal | decimal | Sí | Subtotal sin impuestos |
| taxAmount | decimal | Sí | Monto de impuestos |
| shippingCost | decimal | Sí | Costo de envío |
| notes | string | No | Notas de la orden |
| createdAt | DateTime | Sí | Fecha de creación |
| updatedAt | DateTime | Sí | Fecha de última actualización |
| items | OrderItemDto[] | Sí | Items de la orden |

### OrderItemDto

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| id | Guid | Sí | Identificador único del item |
| productId | Guid | Sí | ID del producto |
| productName | string | Sí | Nombre del producto (snapshot) |
| quantity | int | Sí | Cantidad ordenada |
| unitPrice | decimal | Sí | Precio unitario (snapshot) |
| subtotal | decimal | Sí | Subtotal del item (calculado) |

### Enumeraciones

#### OrderStatus
- `Pending = 1` - Orden pendiente de procesamiento
- `Confirmed = 2` - Orden confirmada  
- `Processing = 3` - Orden en proceso
- `Shipped = 4` - Orden enviada
- `Delivered = 5` - Orden entregada
- `Cancelled = 6` - Orden cancelada

#### Gender
- `Male = 1` - Masculino
- `Female = 2` - Femenino
- `Other = 3` - Otro
- `PreferNotToSay = 4` - Prefiere no decir

#### LogLevel
- `Trace = 0` - Trazabilidad detallada
- `Debug = 1` - Información de debug
- `Information = 2` - Información general
- `Warning = 3` - Advertencia
- `Error = 4` - Error
- `Critical = 5` - Error crítico

---

## Flujos Principales de Uso

### 1. Flujo de Registro y Autenticación

```
1. Usuario completa formulario de registro
2. Frontend valida datos localmente
3. POST /api/customers/register
4. Backend valida y crea cliente
5. Backend genera JWT token
6. Frontend almacena token y datos del usuario
7. Redirección al dashboard/perfil
```

### 2. Flujo de Creación de Orden

```
1. Usuario selecciona productos y cantidades
2. Frontend valida stock disponible (opcional)
3. Usuario procede al checkout
4. POST /api/orders con productos seleccionados
5. Backend valida cliente, productos y stock
6. Backend calcula totales y crea orden
7. Backend publica evento OrderCreated
8. Frontend muestra confirmación de orden
9. ProductService actualiza stock automáticamente
```

### 3. Transiciones de Estado de Orden

```
PENDING → CONFIRMED → PROCESSING → SHIPPED → DELIVERED
    ↓         ↓           ↓
CANCELLED CANCELLED  CANCELLED
```

**Estados válidos por rol:**
- Cliente: Solo puede ver estados, no modificar
- Administrador: Puede cambiar entre estados válidos
- Sistema: Transiciones automáticas basadas en eventos

### 4. Flujo de Gestión de Stock

```
1. Orden creada → Stock reservado inmediatamente
2. Orden cancelada → Stock liberado
3. Producto actualizado → Evento de stock publicado
4. LoggingService registra todos los movimientos
```

---

## Autenticación y Autorización

### Estructura del JWT Token

**Claims incluidos:**
```json
{
  "nameid": "customer-guid",
  "email": "user@example.com", 
  "name": "Nombre Completo",
  "role": ["Customer"],
  "jti": "unique-token-id",
  "iat": "issued-at-timestamp",
  "exp": "expiration-timestamp"
}
```

### Configuración de Autenticación

**Headers requeridos:**
```http
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

**Duración del token:** 60 minutos (configurable)

**Algoritmo:** HMAC-SHA256

### Endpoints que NO requieren autenticación:
- `POST /api/customers/register`
- `POST /api/customers/login`
- `GET /api/*/test`
- `GET /api/*/jwt-debug`
- `GET /api/*/health`

---

## Validaciones y Reglas de Negocio

### Validaciones de Cliente

#### Registro
- **Email:** Formato válido, único, máximo 255 caracteres
- **Password:** 8-100 caracteres, debe incluir:
  - Al menos una minúscula
  - Al menos una mayúscula
  - Al menos un dígito
  - Al menos un carácter especial
- **Nombres:** Solo letras y espacios, máximo 100 caracteres
- **Teléfono:** Formato válido, máximo 20 caracteres (opcional)
- **Fecha de nacimiento:** Debe ser fecha pasada, realista

#### Actualización de Perfil
- No se puede cambiar email
- Password requiere validación del password actual
- Otros campos siguen las mismas reglas de registro

### Validaciones de Orden

#### Creación
- **Cliente:** Debe existir y estar activo
- **Items:** Mínimo 1, máximo 50 items por orden
- **Cantidad:** Entre 1 y 1000 por item
- **Productos:** Deben existir, estar activos y tener stock suficiente
- **Notas:** Máximo 500 caracteres (opcional)

#### Actualización
- Solo órdenes en estado `Pending` pueden editarse completamente
- Cambios de estado deben seguir transiciones válidas
- Items: Mismas validaciones que creación

#### Eliminación
- Solo órdenes en estado `Pending` pueden eliminarse
- La eliminación es soft delete (IsDeleted = true)

### Cálculos Financieros

- **Subtotal:** Suma de (cantidad × precio unitario)
- **Impuestos:** 10% del subtotal
- **Envío:** $10.00 (gratis si subtotal > $100.00)
- **Total:** Subtotal + Impuestos + Envío

---

## Códigos de Error y Manejo

### Estructura de Respuesta de Error

```json
{
  "success": false,
  "message": "Mensaje descriptivo del error",
  "data": null,
  "errors": ["Lista de errores específicos"],
  "timestamp": "2025-08-01T10:30:00Z"
}
```

### Códigos de Estado HTTP

#### 400 Bad Request - Errores de Validación
```json
{
  "success": false,
  "message": "Uno o más errores de validación ocurrieron.",
  "errors": [
    "El email es requerido",
    "La contraseña debe tener al menos 8 caracteres",
    "El nombre solo puede contener letras y espacios"
  ]
}
```

#### 401 Unauthorized - Autenticación Requerida
```json
{
  "success": false,
  "message": "Autenticación requerida",
  "errors": ["Token inválido o expirado"]
}
```

#### 404 Not Found - Entidad No Encontrada
```json
{
  "success": false,
  "message": "La entidad 'Customer' con clave '{guid}' no fue encontrada.",
  "errors": []
}
```

#### 409 Conflict - Violación de Reglas de Negocio
```json
{
  "success": false,
  "message": "Stock insuficiente para el producto {productId}. Disponible: 5, Solicitado: 10",
  "errors": []
}
```

#### 500 Internal Server Error - Error del Servidor
```json
{
  "success": false,
  "message": "Error interno del servidor al crear la orden",
  "errors": []
}
```

---

## Paginación y Filtros

### Estructura de Paginación

**Parámetros de Query:**
- `page` - Número de página (default: 1, mínimo: 1)
- `pageSize` - Tamaño de página (default: 10, máximo: 100)
- `sortBy` - Campo para ordenar (opcional)
- `sortDirection` - Dirección del ordenamiento: "asc" o "desc" (default: "asc")

**Respuesta paginada:**
```json
{
  "items": [/* array de items */],
  "totalCount": 100,
  "currentPage": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasPrevious": false,
  "hasNext": true
}
```

### Filtros por Endpoint

#### GET /api/customers
- `searchTerm` - Busca en nombre, email
- `isActive` - Filtro por estado

#### GET /api/orders  
- `customerId` - Órdenes de un cliente específico
- `status` - Filtro por estado de orden
- `fromDate` - Fecha de inicio
- `toDate` - Fecha de fin
- `orderNumber` - Búsqueda exacta por número

#### GET /api/products
- `category` - Filtro por categoría
- `searchTerm` - Busca en nombre, descripción, tags
- `isActive` - Filtro por estado

#### GET /api/logs/search
- `serviceName` - Nombre del servicio
- `level` - Nivel de log
- `fromDate` - Fecha de inicio
- `toDate` - Fecha de fin
- `category` - Categoría de log
- `correlationId` - ID de correlación
- `userId` - ID de usuario

---

## Notas para el Frontend

### Recomendaciones de Implementación

#### 1. Manejo de Autenticación
```javascript
// Ejemplo de interceptor para agregar JWT automáticamente
axios.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Manejo de respuestas 401
axios.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Limpiar sesión y redirigir al login
      localStorage.removeItem('authToken');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

#### 2. Gestión de Estado de Orden
```javascript
// Estados de orden para la UI
const orderStates = {
  PENDING: { canEdit: true, canCancel: true, showTracking: false },
  CONFIRMED: { canEdit: false, canCancel: true, showTracking: false },
  PROCESSING: { canEdit: false, canCancel: false, showTracking: true },
  SHIPPED: { canEdit: false, canCancel: false, showTracking: true },
  DELIVERED: { canEdit: false, canCancel: false, showTracking: true },
  CANCELLED: { canEdit: false, canCancel: false, showTracking: false }
};
```

#### 3. Validación de Formularios
```javascript
// Validaciones del lado cliente que coinciden con el backend
const validatePassword = (password) => {
  const regex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/;
  return regex.test(password);
};

const validateEmail = (email) => {
  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return regex.test(email) && email.length <= 255;
};
```

### Puntos de Integración Críticos

#### 1. Creación de Orden
- Validar stock antes de enviar la orden
- Manejar respuestas de productos no encontrados
- Mostrar cálculos de precio en tiempo real

#### 2. Gestión de Sesión
- Monitorear expiración del token
- Implementar auto-logout por inactividad
- Manejar refrescos de página con token existente

#### 3. Manejo de Errores
- Mostrar errores de validación por campo
- Diferenciar entre errores de negocio y técnicos
- Implementar reintentos para errores temporales

### Consideraciones de Performance

#### 1. Caching
- Cachear lista de productos (se actualiza raramente)
- Cachear información del perfil del usuario
- Invalidar cache al detectar cambios

#### 2. Paginación
- Usar paginación lazy loading para listas grandes
- Implementar búsqueda con debounce
- Mostrar indicadores de carga durante requests

#### 3. Optimización de Requests
- Batching de requests cuando sea posible
- Usar el endpoint `/api/products/batch` para múltiples productos
- Implementar loading states apropiados

---

## Glosario de Términos

**API Response**: Estructura estándar de respuesta que incluye `success`, `message`, `data`, `errors`, y `timestamp`.

**Bearer Token**: Token JWT incluido en el header `Authorization` con el formato `Bearer <token>`.

**CQRS**: Command Query Responsibility Segregation - patrón que separa operaciones de lectura y escritura.

**DTO**: Data Transfer Object - objeto usado para transferir datos entre capas o servicios.

**Event-Driven**: Arquitectura basada en eventos donde los servicios se comunican mediante eventos asíncronos.

**JWT**: JSON Web Token - estándar para representar claims de manera segura entre partes.

**Microservices**: Arquitectura donde la aplicación se divide en servicios pequeños e independientes.

**PagedResult**: Estructura que encapsula resultados paginados con metadatos de paginación.

**Soft Delete**: Eliminación lógica donde se marca un registro como eliminado sin borrarlo físicamente.

**UnitOfWork**: Patrón que mantiene una lista de objetos afectados por una transacción de negocio.

---

## Anexos

### A. Configuración de Desarrollo

#### Variables de Entorno
```bash
# URLs de servicios
CUSTOMER_SERVICE_URL=https://localhost:5003
ORDER_SERVICE_URL=https://localhost:5001
PRODUCT_SERVICE_URL=https://localhost:5002
LOGGING_SERVICE_URL=https://localhost:5004

# JWT Configuration
JWT_SECRET_KEY=OrderManagement-JWT-Secret-Key-2025-Super-Secure-At-Least-256-Bits-Long
JWT_ISSUER=OrderManagementSystem
JWT_AUDIENCE=OrderManagementSystem
JWT_EXPIRE_MINUTES=60
```

#### CORS Configuration
El backend está configurado con política CORS permisiva para desarrollo:
```javascript
// Permitidas todas las origins, métodos y headers en desarrollo
// Para producción, configurar origins específicas
```

### B. Ejemplos de Integración

#### Login Flow Completo
```javascript
// 1. Login request
const loginResponse = await fetch('/api/customers/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

const authData = await loginResponse.json();

// 2. Store auth data
localStorage.setItem('authToken', authData.token);
localStorage.setItem('user', JSON.stringify({
  id: authData.id,
  email: authData.email,
  fullName: authData.fullName
}));

// 3. Set token expiration timer
const expiresIn = new Date(authData.tokenExpires) - new Date();
setTimeout(() => {
  // Handle token expiration
  localStorage.clear();
  window.location.href = '/login';
}, expiresIn);
```

#### Order Creation Flow
```javascript
// 1. Validate products and stock
const validateProducts = async (items) => {
  const productIds = items.map(item => item.productId);
  const products = await fetch('/api/products/batch', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ productIds })
  });
  
  // Validate stock for each item
  for (const item of items) {
    const product = products.find(p => p.id === item.productId);
    if (product.stock < item.quantity) {
      throw new Error(`Insufficient stock for ${product.name}`);
    }
  }
};

// 2. Create order
const createOrder = async (orderData) => {
  await validateProducts(orderData.items);
  
  const response = await fetch('/api/orders', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(orderData)
  });
  
  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.message);
  }
  
  return await response.json();
};
```

### C. Testing Endpoints

#### Postman Collection Structure
```json
{
  "info": { "name": "Order Management API" },
  "variable": [
    { "key": "baseUrl", "value": "https://localhost" },
    { "key": "token", "value": "{{authToken}}" }
  ],
  "item": [
    {
      "name": "Authentication",
      "item": [
        { "name": "Register", "request": { /* POST /api/customers/register */ } },
        { "name": "Login", "request": { /* POST /api/customers/login */ } }
      ]
    },
    {
      "name": "Orders",
      "item": [
        { "name": "Create Order", "request": { /* POST /api/orders */ } },
        { "name": "Get Orders", "request": { /* GET /api/orders */ } }
      ]
    }
  ]
}
```

---

Este documento proporciona toda la información necesaria para desarrollar un frontend que se integre correctamente con el Order Management System backend. Para cualquier duda o aclaración adicional, consultar la documentación de Swagger en cada servicio o revisar el código fuente del backend.