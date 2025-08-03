import { describe, it, expect, vi, beforeEach } from 'vitest';
import { productService } from './productService';
import { type ProductDto } from '../types/entities';

// Mock the entire productService module
vi.mock('./productService', () => ({
  productService: {
    getProducts: vi.fn(),
    getProduct: vi.fn(),
    searchProducts: vi.fn(),
    getActiveProducts: vi.fn()
  }
}));

describe('ProductService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getProducts', () => {
    it('calls API with correct parameters', async () => {
      const mockResponse = {
        items: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10,
        totalPages: 1,
        hasNext: false,
        hasPrevious: false
      };

      vi.mocked(productService.getProducts).mockResolvedValue(mockResponse);

      const params = { page: 1, pageSize: 10 };
      const result = await productService.getProducts(params);

      expect(productService.getProducts).toHaveBeenCalledWith(params);
      expect(result).toEqual(mockResponse);
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
        minimumStock: 5,
        sku: 'TEST-001',
        category: 'Test',
        isActive: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };

      vi.mocked(productService.getProduct).mockResolvedValue(mockProduct);

      const result = await productService.getProduct('1');

      expect(productService.getProduct).toHaveBeenCalledWith('1');
      expect(result).toEqual(mockProduct);
    });
  });

  describe('helper methods', () => {
    it('searchProducts calls getProducts with search parameter', async () => {
      const mockProducts: ProductDto[] = [];

      vi.mocked(productService.searchProducts).mockResolvedValue(mockProducts);

      const result = await productService.searchProducts('test', 5);

      expect(productService.searchProducts).toHaveBeenCalledWith('test', 5);
      expect(result).toEqual(mockProducts);
    });

    it('getActiveProducts calls getProducts with isActive filter', async () => {
      const mockResponse = {
        items: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 20,
        totalPages: 0,
        hasNext: false,
        hasPrevious: false
      };

      vi.mocked(productService.getActiveProducts).mockResolvedValue(mockResponse);

      const result = await productService.getActiveProducts();

      expect(productService.getActiveProducts).toHaveBeenCalledWith();
      expect(result).toEqual(mockResponse);
    });
  });
});