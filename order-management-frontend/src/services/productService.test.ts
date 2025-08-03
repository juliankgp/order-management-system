import { describe, it, expect, vi, beforeEach } from 'vitest';
import { productService } from './productService';
import { type ProductDto } from '../types/entities';

// Mock de axios
vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: vi.fn(),
      post: vi.fn(),
      put: vi.fn(),
      delete: vi.fn(),
      interceptors: {
        request: { use: vi.fn() },
        response: { use: vi.fn() }
      }
    }))
  }
}));

describe('ProductService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getProducts', () => {
    it('calls API with correct parameters', async () => {
      const mockResponse = {
        data: {
          items: [],
          totalCount: 0,
          currentPage: 1,
          pageSize: 10
        }
      };

      const mockGet = vi.fn().mockResolvedValue(mockResponse);
      (productService as any).client.get = mockGet;

      const params = { page: 1, pageSize: 10 };
      await productService.getProducts(params);

      expect(mockGet).toHaveBeenCalledWith('/products', { params });
    });
  });

  describe('getProduct', () => {
    it('calls API with product ID', async () => {
      const mockProduct: ProductDto = {
        id: '1',
        name: 'Test Product',
        description: 'Test Description',
        price: 100,
        stock: 10,
        sku: 'TEST-001',
        category: 'Test',
        isActive: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };

      const mockGet = vi.fn().mockResolvedValue({ data: mockProduct });
      (productService as any).client.get = mockGet;

      await productService.getProduct('1');

      expect(mockGet).toHaveBeenCalledWith('/products/1');
    });
  });

  describe('helper methods', () => {
    it('searchProducts calls getProducts with search parameter', async () => {
      const spy = vi.spyOn(productService, 'getProducts').mockResolvedValue({
        items: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10
      });

      await productService.searchProducts('test', 5);

      expect(spy).toHaveBeenCalledWith({
        search: 'test',
        pageSize: 5
      });
    });

    it('getActiveProducts calls getProducts with isActive filter', async () => {
      const spy = vi.spyOn(productService, 'getProducts').mockResolvedValue({
        items: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10
      });

      await productService.getActiveProducts();

      expect(spy).toHaveBeenCalledWith({ isActive: true });
    });
  });
});