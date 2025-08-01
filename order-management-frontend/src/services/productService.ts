import { productApiClient } from './apiClient';
import { API_ENDPOINTS } from '../constants/api';
import type { 
  ProductDto, 
  ApiResponse, 
  PagedResponse
} from '../types';

export interface GetProductsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  category?: string;
  isActive?: boolean;
}

export interface ValidateStockRequest {
  items: Array<{
    productId: string;
    quantity: number;
  }>;
}

export interface ValidateStockResponse {
  isValid: boolean;
  items: Array<{
    productId: string;
    requestedQuantity: number;
    availableStock: number;
    isAvailable: boolean;
  }>;
}

class ProductService {
  async getProducts(params: GetProductsParams = {}): Promise<PagedResponse<ProductDto>> {
    const searchParams = new URLSearchParams();
    
    if (params.page !== undefined) searchParams.append('page', params.page.toString());
    if (params.pageSize !== undefined) searchParams.append('pageSize', params.pageSize.toString());
    if (params.search) searchParams.append('search', params.search);
    if (params.category) searchParams.append('category', params.category);
    if (params.isActive !== undefined) searchParams.append('isActive', params.isActive.toString());

    const url = `${API_ENDPOINTS.PRODUCTS.GET_ALL}?${searchParams.toString()}`;
    
    const response = await productApiClient.get<ApiResponse<PagedResponse<ProductDto>>>(url);
    return response.data.data;
  }

  async getProduct(id: string): Promise<ProductDto> {
    const response = await productApiClient.get<ApiResponse<ProductDto>>(
      API_ENDPOINTS.PRODUCTS.GET_BY_ID.replace(':id', id)
    );
    return response.data.data;
  }

  async getProductsBatch(ids: string[]): Promise<ProductDto[]> {
    const response = await productApiClient.post<ApiResponse<ProductDto[]>>(
      API_ENDPOINTS.PRODUCTS.GET_BATCH,
      { productIds: ids }
    );
    return response.data.data;
  }

  async validateStock(request: ValidateStockRequest): Promise<ValidateStockResponse> {
    const response = await productApiClient.post<ApiResponse<ValidateStockResponse>>(
      API_ENDPOINTS.PRODUCTS.VALIDATE_STOCK,
      request
    );
    return response.data.data;
  }

  async searchProducts(query: string, limit: number = 10): Promise<ProductDto[]> {
    const params: GetProductsParams = {
      search: query,
      pageSize: limit,
      page: 1,
      isActive: true
    };
    
    const result = await this.getProducts(params);
    return result.items;
  }

  async getProductsByCategory(category: string, page: number = 1, pageSize: number = 20): Promise<PagedResponse<ProductDto>> {
    return this.getProducts({
      category,
      page,
      pageSize,
      isActive: true
    });
  }

  async getActiveProducts(page: number = 1, pageSize: number = 20): Promise<PagedResponse<ProductDto>> {
    return this.getProducts({
      page,
      pageSize,
      isActive: true
    });
  }
}

export const productService = new ProductService();

export default productService;