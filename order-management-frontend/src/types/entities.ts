// Enumeraciones basadas en la especificación del backend

export const Gender = {
  Male: 1,
  Female: 2,
  Other: 3,
  PreferNotToSay: 4
} as const;
export type Gender = typeof Gender[keyof typeof Gender];

export const OrderStatus = {
  Pending: 1,
  Confirmed: 2,
  Processing: 3,
  Shipped: 4,
  Delivered: 5,
  Cancelled: 6
} as const;
export type OrderStatus = typeof OrderStatus[keyof typeof OrderStatus];

export const LogLevel = {
  Trace: 0,
  Debug: 1,
  Information: 2,
  Warning: 3,
  Error: 4,
  Critical: 5
} as const;
export type LogLevel = typeof LogLevel[keyof typeof LogLevel];

// Interfaces para entidades principales

export interface CustomerDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  gender?: Gender;
  isActive: boolean;
  emailVerified: boolean;
  emailVerifiedAt?: string;
  lastLoginAt?: string;
  preferences?: string;
  createdAt: string;
  updatedAt: string;
  addresses: CustomerAddressDto[];
}

export interface CustomerAddressDto {
  id: string;
  customerId: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  zipCode: string;
  country: string;
  isDefault: boolean;
  addressType: string;
  createdAt: string;
  updatedAt: string;
}

export interface ProductDto {
  id: string;
  name: string;
  description?: string;
  sku: string;
  price: number;
  stockQuantity: number;
  minimumStock: number;
  category?: string;
  brand?: string;
  weight?: number;
  length?: number;
  width?: number;
  height?: number;
  imageUrl?: string;
  isActive: boolean;
  tags?: string;
  createdAt: string;
  updatedAt: string;
}

export interface OrderDto {
  id: string;
  customerId: string;
  customerName?: string; // Para display
  orderNumber: string;
  status: OrderStatus;
  orderDate: string;
  totalAmount: number;
  subTotal: number;
  taxAmount: number;
  shippingCost: number;
  notes?: string;
  createdAt: string;
  updatedAt: string;
  items: OrderItemDto[];
}

export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  subtotal: number;
}

export interface LogEntryDto {
  id: string;
  timestamp: string;
  level: LogLevel;
  message: string;
  serviceName: string;
  category?: string;
  correlationId?: string;
  userId?: string;
  properties?: string;
  exception?: string;
  createdAt: string;
}

// DTOs para requests

export interface RegisterCustomerDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  dateOfBirth?: Date | null;
  gender?: Gender | null;
}

export interface LoginCustomerDto {
  email: string;
  password: string;
}

export interface CreateOrderDto {
  customerId: string;
  notes?: string;
  items: CreateOrderItemDto[];
}

export interface CreateOrderItemDto {
  productId: string;
  quantity: number;
}

export interface UpdateOrderDto {
  status?: OrderStatus;
  notes?: string;
  items?: CreateOrderItemDto[];
}

// DTOs para responses de autenticación

export interface AuthResponse {
  id: string;
  email: string;
  fullName: string;
  token: string;
  tokenExpires: string;
  emailVerified: boolean;
}

// DTOs para validaciones

export interface ValidateStockRequest {
  items: StockValidationItem[];
}

export interface StockValidationItem {
  productId: string;
  quantity: number;
}

export interface ValidateStockResponse {
  isValid: boolean;
  errors: StockValidationError[];
}

export interface StockValidationError {
  productId: string;
  productName: string;
  requestedQuantity: number;
  availableStock: number;
  message: string;
}

export interface BatchProductRequest {
  productIds: string[];
}

// Tipos para UI state

export interface User {
  id: string;
  email: string;
  fullName: string;
  emailVerified: boolean;
}

export interface CartItem {
  product: ProductDto;
  quantity: number;
  subtotal: number;
}

export interface Cart {
  items: CartItem[];
  itemCount: number;
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  total: number;
}

// Tipos para filtros y búsquedas

export interface CustomerFilters {
  searchTerm?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface ProductFilters {
  category?: string;
  searchTerm?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface ProductFilter {
  search?: string;
  category?: string;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
  inStock?: boolean;
}

export interface OrderFilters {
  customerId?: string;
  status?: OrderStatus;
  fromDate?: string;
  toDate?: string;
  orderNumber?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface LogFilters {
  serviceName?: string;
  level?: LogLevel;
  fromDate?: string;
  toDate?: string;
  category?: string;
  correlationId?: string;
  userId?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

// Tipos para formularios

export interface LoginFormData {
  email: string;
  password: string;
}

export interface RegisterFormData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  dateOfBirth?: Date | null;
  gender?: number | null;
}

export interface OrderFormData {
  notes?: string;
  items: {
    productId: string;
    quantity: number;
  }[];
}