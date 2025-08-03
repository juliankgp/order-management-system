import axios, { type AxiosResponse, type InternalAxiosRequestConfig } from 'axios';
import { API_CONFIG, HTTP_STATUS } from '../constants/api';
import { serviceConfig } from '../config/serviceConfig';

// Custom Axios instance type that returns data directly (due to response interceptor)
interface CustomAxiosInstance {
  get<T = unknown>(url: string, config?: object): Promise<T>;
  post<T = unknown>(url: string, data?: unknown, config?: object): Promise<T>;
  put<T = unknown>(url: string, data?: unknown, config?: object): Promise<T>;
  patch<T = unknown>(url: string, data?: unknown, config?: object): Promise<T>;
  delete<T = unknown>(url: string, config?: object): Promise<T>;
}

// Interfaz para respuesta estándar de API
export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  timestamp: string;
}

// Interfaz para respuesta paginada
export interface PagedResponse<T = unknown> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// Crear instancia de cliente API con configuración base
const createApiClient = (baseURL: string): CustomAxiosInstance => {
  const client = axios.create({
    baseURL,
    timeout: API_CONFIG.TIMEOUT,
    headers: {
      'Content-Type': API_CONFIG.CONTENT_TYPE
    }
  });

  // Request interceptor para añadir JWT token automáticamente
  client.interceptors.request.use(
    (config: InternalAxiosRequestConfig) => {
      const token = localStorage.getItem('authToken');
      if (token) {
        config.headers.Authorization = `${API_CONFIG.JWT_HEADER_PREFIX}${token}`;
      }

      // Log de requests en desarrollo
      if (import.meta.env.DEV) {
        console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`, {
          headers: config.headers,
          data: config.data,
          params: config.params
        });
      }

      return config;
    },
    (error) => {
      console.error('❌ Request interceptor error:', error);
      return Promise.reject(error);
    }
  );

  // Response interceptor para manejo global de respuestas y errores
  client.interceptors.response.use(
    (response: AxiosResponse) => {
      // Log de responses exitosas en desarrollo
      if (import.meta.env.DEV) {
        console.log(`✅ API Response: ${response.status} ${response.config.method?.toUpperCase()} ${response.config.url}`, response.data);
      }

      // Devolver directamente el data para respuestas exitosas
      return response.data;
    },
    (error) => {
      // Log de errores en desarrollo
      if (import.meta.env.DEV) {
        console.error(`❌ API Error: ${error.response?.status} ${error.config?.method?.toUpperCase()} ${error.config?.url}`, error.response?.data);
      }

      // Manejo específico de códigos de error
      if (error.response) {
        const { status } = error.response;

        switch (status) {
          case HTTP_STATUS.UNAUTHORIZED:
            // Token expirado o inválido - limpiar sesión y redirigir
            console.warn('🔒 Unauthorized access - clearing session');
            localStorage.removeItem('authToken');
            localStorage.removeItem('user');
            
            // Evitar redirección infinita si ya estamos en login
            if (!window.location.pathname.includes('/login')) {
              window.location.href = '/login';
            }
            break;

          case HTTP_STATUS.FORBIDDEN:
            console.warn('🚫 Forbidden access');
            break;

          case HTTP_STATUS.NOT_FOUND:
            console.warn('🔍 Resource not found');
            break;

          case HTTP_STATUS.CONFLICT:
            console.warn('⚠️ Conflict error (business rule violation)');
            break;

          case HTTP_STATUS.INTERNAL_SERVER_ERROR:
            console.error('💥 Internal server error');
            break;

          default:
            console.error(`🚨 Unexpected error: ${status}`);
        }
      } else if (error.request) {
        // Error de red
        console.error('🌐 Network error:', error.message);
      } else {
        // Error de configuración
        console.error('⚙️ Request configuration error:', error.message);
      }

      // Devolver el error normalizado
      return Promise.reject({
        success: false,
        message: error.response?.data?.message || error.message || 'An unexpected error occurred',
        data: null,
        errors: error.response?.data?.errors || [error.message],
        timestamp: new Date().toISOString(),
        status: error.response?.status
      });
    }
  );

  return client as CustomAxiosInstance;
};

// Instancias de clientes para cada microservicio
export const customerApiClient = createApiClient(serviceConfig.customerService);
export const orderApiClient = createApiClient(serviceConfig.orderService);
export const productApiClient = createApiClient(serviceConfig.productService);
export const loggingApiClient = createApiClient(serviceConfig.loggingService);

// Cliente genérico para casos especiales
export const genericApiClient = createApiClient('');

// Función helper para reemplazar parámetros en URLs
export const replaceUrlParams = (url: string, params: Record<string, string | number>): string => {
  let replacedUrl = url;
  Object.entries(params).forEach(([key, value]) => {
    replacedUrl = replacedUrl.replace(`{${key}}`, String(value));
  });
  return replacedUrl;
};

// Función helper para construir query parameters
export const buildQueryParams = (params: Record<string, unknown>): string => {
  const searchParams = new URLSearchParams();
  
  Object.entries(params).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      if (Array.isArray(value)) {
        value.forEach(item => searchParams.append(key, String(item)));
      } else {
        searchParams.append(key, String(value));
      }
    }
  });

  const queryString = searchParams.toString();
  return queryString ? `?${queryString}` : '';
};

// Tipo para parámetros de paginación
export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

// Función helper para manejar parámetros de paginación
export const buildPaginationParams = (params?: PaginationParams): Record<string, unknown> => {
  return {
    page: params?.page || 1,
    pageSize: params?.pageSize || API_CONFIG.DEFAULT_PAGE_SIZE,
    sortBy: params?.sortBy,
    sortDirection: params?.sortDirection || 'asc'
  };
};

export default {
  customerApiClient,
  orderApiClient,
  productApiClient,
  loggingApiClient,
  genericApiClient,
  replaceUrlParams,
  buildQueryParams,
  buildPaginationParams
};