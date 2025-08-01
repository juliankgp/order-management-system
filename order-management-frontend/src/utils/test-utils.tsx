import { type ReactElement } from 'react';
import { render, type RenderOptions } from '@testing-library/react';
import { vi } from 'vitest';
import TestWrapper from './TestWrapper';

// Custom render function
const customRender = (
  ui: ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>,
) => render(ui, { wrapper: TestWrapper, ...options });

// Mock data para testing
export const mockCustomer = {
  id: '123e4567-e89b-12d3-a456-426614174001',
  email: 'test@example.com',
  firstName: 'John',
  lastName: 'Doe',
  fullName: 'John Doe',
  phoneNumber: '+1-555-0123',
  dateOfBirth: '1990-01-01',
  gender: 1,
  isActive: true,
  emailVerified: true,
  emailVerifiedAt: '2025-01-01T00:00:00Z',
  lastLoginAt: '2025-01-01T00:00:00Z',
  preferences: null,
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-01T00:00:00Z',
  addresses: []
};

export const mockProduct = {
  id: '123e4567-e89b-12d3-a456-426614174003',
  name: 'Test Product',
  description: 'A test product description',
  sku: 'TEST-001',
  price: 99.99,
  stock: 50,
  minimumStock: 10,
  category: 'Electronics',
  brand: 'TestBrand',
  weight: 1000,
  dimensions: '10x10x10',
  imageUrl: 'https://example.com/test-product.jpg',
  isActive: true,
  tags: 'test,product,electronics',
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-01T00:00:00Z'
};

export const mockOrder = {
  id: '123e4567-e89b-12d3-a456-426614174000',
  customerId: '123e4567-e89b-12d3-a456-426614174001',
  customerName: 'John Doe',
  orderNumber: 'ORD-20250801-A1B2C3D4',
  status: 1, // Pending
  orderDate: '2025-01-01T00:00:00Z',
  totalAmount: 109.99,
  subTotal: 99.99,
  taxAmount: 10.00,
  shippingCost: 0.00,
  notes: 'Test order notes',
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-01T00:00:00Z',
  items: [
    {
      id: '123e4567-e89b-12d3-a456-426614174002',
      productId: '123e4567-e89b-12d3-a456-426614174003',
      productName: 'Test Product',
      quantity: 1,
      unitPrice: 99.99,
      subtotal: 99.99
    }
  ]
};

export const mockAuthResponse = {
  id: '123e4567-e89b-12d3-a456-426614174001',
  email: 'test@example.com',
  fullName: 'John Doe',
  token: 'mock-jwt-token',
  tokenExpires: '2025-01-02T00:00:00Z',
  emailVerified: true
};

// Función helper para simular localStorage
export const mockLocalStorage = () => {
  const mockStorage: { [key: string]: string } = {};
  
  return {
    getItem: vi.fn((key: string) => mockStorage[key] || null),
    setItem: vi.fn((key: string, value: string) => {
      mockStorage[key] = value;
    }),
    removeItem: vi.fn((key: string) => {
      delete mockStorage[key];
    }),
    clear: vi.fn(() => {
      Object.keys(mockStorage).forEach(key => delete mockStorage[key]);
    }),
    key: vi.fn(),
    length: 0
  };
};

// Función helper para setup de usuario autenticado
export const setupAuthenticatedUser = () => {
  localStorage.setItem('authToken', mockAuthResponse.token);
  localStorage.setItem('user', JSON.stringify({
    id: mockAuthResponse.id,
    email: mockAuthResponse.email,
    fullName: mockAuthResponse.fullName,
    emailVerified: mockAuthResponse.emailVerified
  }));
};

// Función helper para limpiar autenticación
export const clearAuthentication = () => {
  localStorage.removeItem('authToken');
  localStorage.removeItem('user');
};

// re-export specific testing utilities
export { 
  screen,
  fireEvent,
  waitFor,
  act,
  cleanup,
  within,
  prettyDOM 
} from '@testing-library/react';

// override render method
export { customRender as render };