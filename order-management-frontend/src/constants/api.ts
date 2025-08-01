// API Base URLs para cada microservicio
export const API_BASE_URLS = {
  CUSTOMER_SERVICE: 'https://localhost:5003',
  ORDER_SERVICE: 'https://localhost:5001', 
  PRODUCT_SERVICE: 'https://localhost:5002',
  LOGGING_SERVICE: 'https://localhost:5004'
} as const;

// Endpoints de API organizados por servicio
export const API_ENDPOINTS = {
  // Customer endpoints
  CUSTOMERS: {
    REGISTER: '/api/customers/register',
    LOGIN: '/api/customers/login',
    PROFILE: '/api/customers/profile',
    LIST: '/api/customers',
    DETAIL: '/api/customers/{id}',
    TEST: '/api/customers/test',
    JWT_DEBUG: '/api/customers/jwt-debug'
  },
  // Product endpoints  
  PRODUCTS: {
    LIST: '/api/products',
    DETAIL: '/api/products/{id}',
    VALIDATE: '/api/products/{id}/validate',
    VALIDATE_STOCK: '/api/products/validate-stock',
    BATCH: '/api/products/batch',
    TEST: '/api/products/test',
    HEALTH: '/api/products/health'
  },
  // Order endpoints
  ORDERS: {
    LIST: '/api/orders',
    CREATE: '/api/orders',
    DETAIL: '/api/orders/{id}',
    UPDATE: '/api/orders/{id}',
    DELETE: '/api/orders/{id}',
    TEST: '/api/orders/test',
    JWT_DEBUG: '/api/orders/jwt-debug',
    HEALTH: '/api/orders/health'
  },
  // Logging endpoints
  LOGS: {
    LIST: '/api/logs',
    SEARCH: '/api/logs/search',
    BY_SERVICE: '/api/logs/service/{serviceName}',
    BY_CORRELATION: '/api/logs/correlation/{correlationId}',
    BY_USER: '/api/logs/user/{userId}',
    CREATE: '/api/logs',
    HEALTH: '/api/logs/health'
  }
} as const;

// Configuraciones por defecto
export const API_CONFIG = {
  TIMEOUT: 10000,
  DEFAULT_PAGE_SIZE: 10,
  MAX_PAGE_SIZE: 100,
  JWT_HEADER_PREFIX: 'Bearer ',
  CONTENT_TYPE: 'application/json'
} as const;

// Códigos de estado HTTP
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  INTERNAL_SERVER_ERROR: 500
} as const;