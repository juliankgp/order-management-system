import { orderApiClient } from './apiClient';
import { API_ENDPOINTS } from '../constants/api';
import type { 
  OrderDto, 
  CreateOrderDto,
  UpdateOrderDto,
  ApiResponse, 
  PagedResponse,
  OrderFilters,
  OrderStatus
} from '../types';

export interface GetOrdersParams {
  page?: number;
  pageSize?: number;
  status?: OrderStatus;
  customerId?: string;
  orderNumber?: string;
  fromDate?: string;
  toDate?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface OrderCalculation {
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  total: number;
}

class OrderService {
  // Get orders with filtering and pagination
  async getOrders(params: GetOrdersParams = {}): Promise<PagedResponse<OrderDto>> {
    const searchParams = new URLSearchParams();
    
    if (params.page !== undefined) searchParams.append('page', params.page.toString());
    if (params.pageSize !== undefined) searchParams.append('pageSize', params.pageSize.toString());
    if (params.status !== undefined) searchParams.append('status', params.status.toString());
    if (params.customerId) searchParams.append('customerId', params.customerId);
    if (params.orderNumber) searchParams.append('orderNumber', params.orderNumber);
    if (params.fromDate) searchParams.append('fromDate', params.fromDate);
    if (params.toDate) searchParams.append('toDate', params.toDate);
    if (params.sortBy) searchParams.append('sortBy', params.sortBy);
    if (params.sortDirection) searchParams.append('sortDirection', params.sortDirection);

    const url = `${API_ENDPOINTS.ORDERS.LIST}?${searchParams.toString()}`;
    
    const response = await orderApiClient.get<ApiResponse<PagedResponse<OrderDto>>>(url);
    return response.data.data;
  }

  // Get orders for specific customer
  async getCustomerOrders(customerId: string, params: GetOrdersParams = {}): Promise<PagedResponse<OrderDto>> {
    return this.getOrders({
      ...params,
      customerId
    });
  }

  // Get single order by ID
  async getOrder(id: string): Promise<OrderDto> {
    const response = await orderApiClient.get<ApiResponse<OrderDto>>(
      API_ENDPOINTS.ORDERS.DETAIL.replace('{id}', id)
    );
    return response.data.data;
  }

  // Create new order
  async createOrder(orderData: CreateOrderDto): Promise<OrderDto> {
    const response = await orderApiClient.post<ApiResponse<OrderDto>>(
      API_ENDPOINTS.ORDERS.CREATE,
      orderData
    );
    return response.data.data;
  }

  // Update existing order
  async updateOrder(id: string, updateData: UpdateOrderDto): Promise<OrderDto> {
    const response = await orderApiClient.put<ApiResponse<OrderDto>>(
      API_ENDPOINTS.ORDERS.UPDATE.replace('{id}', id),
      updateData
    );
    return response.data.data;
  }

  // Delete order (cancel)
  async deleteOrder(id: string): Promise<void> {
    await orderApiClient.delete(
      API_ENDPOINTS.ORDERS.DELETE.replace('{id}', id)
    );
  }

  // Cancel order (soft delete by updating status)
  async cancelOrder(id: string): Promise<OrderDto> {
    return this.updateOrder(id, { status: 6 }); // OrderStatus.Cancelled = 6
  }

  // Calculate order totals locally (for client-side validation)
  calculateOrderTotals(items: Array<{ quantity: number; unitPrice: number }>): OrderCalculation {
    const subtotal = items.reduce((sum, item) => sum + (item.quantity * item.unitPrice), 0);
    const taxAmount = subtotal * 0.10; // 10% tax as per backend specification
    const shippingCost = subtotal > 100 ? 0 : 10.00; // Free shipping over $100
    const total = subtotal + taxAmount + shippingCost;

    return {
      subtotal: Math.round(subtotal * 100) / 100,
      taxAmount: Math.round(taxAmount * 100) / 100,
      shippingCost: Math.round(shippingCost * 100) / 100,
      total: Math.round(total * 100) / 100
    };
  }

  // Helper methods for filtering
  async getOrdersByStatus(status: OrderStatus, page: number = 1, pageSize: number = 20): Promise<PagedResponse<OrderDto>> {
    return this.getOrders({
      status,
      page,
      pageSize,
      sortBy: 'orderDate',
      sortDirection: 'desc'
    });
  }

  async getRecentOrders(customerId: string, limit: number = 5): Promise<OrderDto[]> {
    const result = await this.getCustomerOrders(customerId, {
      page: 1,
      pageSize: limit,
      sortBy: 'orderDate',
      sortDirection: 'desc'
    });
    return result.items;
  }

  async searchOrders(query: string, customerId?: string, limit: number = 10): Promise<OrderDto[]> {
    const params: GetOrdersParams = {
      orderNumber: query,
      page: 1,
      pageSize: limit,
      sortBy: 'orderDate',
      sortDirection: 'desc'
    };

    if (customerId) {
      params.customerId = customerId;
    }

    const result = await this.getOrders(params);
    return result.items;
  }

  // Get order status label for display
  getOrderStatusLabel(status: OrderStatus): string {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Confirmed';
      case 3: return 'Processing';
      case 4: return 'Shipped';
      case 5: return 'Delivered';
      case 6: return 'Cancelled';
      default: return 'Unknown';
    }
  }

  // Get order status color for UI
  getOrderStatusColor(status: OrderStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' {
    switch (status) {
      case 1: return 'warning';    // Pending
      case 2: return 'info';       // Confirmed
      case 3: return 'primary';    // Processing
      case 4: return 'secondary';  // Shipped
      case 5: return 'success';    // Delivered
      case 6: return 'error';      // Cancelled
      default: return 'default';
    }
  }

  // Check if order can be cancelled
  canCancelOrder(status: OrderStatus): boolean {
    return status === 1; // Only Pending orders can be cancelled
  }

  // Check if order is active (not cancelled)
  isOrderActive(status: OrderStatus): boolean {
    return status !== 6; // Not cancelled
  }
}

export const orderService = new OrderService();

export default orderService;