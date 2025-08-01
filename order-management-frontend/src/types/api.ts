// Tipos para respuestas de API estándar

export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
  timestamp: string;
}

export interface PagedResult<T = unknown> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// Alias for consistency
export type PagedResponse<T = unknown> = PagedResult<T>;

// Tipos para parámetros de query
export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface QueryParams extends PaginationParams {
  [key: string]: unknown;
}

// Tipos para errores de API
export interface ApiError {
  success: false;
  message: string;
  data: null;
  errors: string[];
  timestamp: string;
  status?: number;
}

// Tipos para configuración de endpoints
export interface EndpointConfig {
  url: string;
  method: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH';
  requiresAuth: boolean;
  timeout?: number;
}

// Tipos para interceptores
export interface RequestInterceptorConfig {
  onRequest?: (config: unknown) => unknown;
  onRequestError?: (error: unknown) => unknown;
}

export interface ResponseInterceptorConfig {
  onResponse?: (response: unknown) => unknown;
  onResponseError?: (error: unknown) => unknown;
}

// Tipos para validación de formularios
export interface ValidationRule {
  required?: boolean;
  minLength?: number;
  maxLength?: number;
  pattern?: RegExp;
  customValidator?: (value: unknown) => string | null;
}

export interface FormValidationSchema {
  [fieldName: string]: ValidationRule;
}

export interface ValidationError {
  field: string;
  message: string;
}

// Tipos para loading states
export interface LoadingState {
  isLoading: boolean;
  error: string | null;
  lastUpdated?: string;
}

export interface AsyncState<T> extends LoadingState {
  data: T | null;
}

// Tipos para cache
export interface CacheEntry<T> {
  data: T;
  timestamp: number;
  expiresIn: number;
}

export interface CacheConfig {
  ttl: number; // Time to live in milliseconds
  maxSize: number;
  enabled: boolean;
}

// Tipos para notificaciones/alertas
export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  title: string;
  message: string;
  duration?: number;
  autoClose?: boolean;
  actions?: NotificationAction[];
}

export interface NotificationAction {
  label: string;
  action: () => void;
  style?: 'primary' | 'secondary';
}

// Tipos para configuración de la aplicación
export interface AppConfig {
  apiBaseUrls: {
    customerService: string;
    orderService: string;
    productService: string;
    loggingService: string;
  };
  auth: {
    tokenKey: string;
    userKey: string;
    tokenExpirationBuffer: number; // minutes before actual expiration
  };
  ui: {
    defaultPageSize: number;
    maxPageSize: number;
    debounceDelay: number;
    animationDuration: number;
  };
  features: {
    enableLogging: boolean;
    enableDevTools: boolean;
    enableOfflineMode: boolean;
  };
}

// Tipos para hooks personalizados
export interface UseApiOptions<T> {
  enabled?: boolean;
  refetchOnWindowFocus?: boolean;
  refetchOnReconnect?: boolean;
  retry?: number | boolean;
  cacheTime?: number;
  staleTime?: number;
  onSuccess?: (data: T) => void;
  onError?: (error: ApiError) => void;
}

export interface UseInfiniteQueryOptions<T> extends UseApiOptions<T> {
  getNextPageParam?: (lastPage: PagedResult<T>) => number | undefined;
  getPreviousPageParam?: (firstPage: PagedResult<T>) => number | undefined;
}

// Tipos para contextos de React
export interface AuthContextType {
  user: Record<string, unknown> | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: Record<string, unknown>) => Promise<void>;
  register: (userData: Record<string, unknown>) => Promise<void>;
  logout: () => void;
  updateProfile: (userData: Record<string, unknown>) => Promise<void>;
}

export interface CartContextType {
  cart: Record<string, unknown>;
  addItem: (product: Record<string, unknown>, quantity: number) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  getTotalItems: () => number;
  getTotalPrice: () => number;
}

export interface NotificationContextType {
  notifications: Notification[];
  addNotification: (notification: Omit<Notification, 'id'>) => string;
  removeNotification: (id: string) => void;
  clearNotifications: () => void;
}

// Tipos para rutas
export interface RouteConfig {
  path: string;
  component: React.ComponentType<Record<string, unknown>>;
  exact?: boolean;
  requiresAuth?: boolean;
  roles?: string[];
  title?: string;
  description?: string;
}

export interface BreadcrumbItem {
  label: string;
  path?: string;
  isActive?: boolean;
}

// Tipos para testing
export interface MockApiResponse<T> {
  success: boolean;
  data: T;
  delay?: number;
  shouldFail?: boolean;
  errorMessage?: string;
}

export interface TestWrapperProps {
  initialState?: Record<string, unknown>;
  customRoutes?: RouteConfig[];
  mockApiResponses?: Record<string, MockApiResponse<unknown>>;
}