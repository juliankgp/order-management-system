import { BaseService } from './baseService';
import type {
  OrderDto,
  CreateOrderDto,
  UpdateOrderDto,
  OrderFilters,
  PagedResponse
} from '../types/entities';
import { OrderStatus } from '../types/entities';

type GetOrdersParams = OrderFilters;

export class OrderService extends BaseService {
  private static readonly ENDPOINTS = {
    BASE: '/api/orders',
    GET_ALL: '/api/orders',
    GET_BY_ID: (id: string) => `/api/orders/${id}`,
    CREATE: '/api/orders',
    UPDATE: (id: string) => `/api/orders/${id}`,
    DELETE: (id: string) => `/api/orders/${id}`,
    TEST: '/api/orders/test',
    HEALTH: '/api/orders/health'
  };

  constructor() {
    super('https://localhost:5001'); // OrderService port
  }

  // Get all orders with optional filtering and pagination
  async getOrders(params: GetOrdersParams = {}): Promise<PagedResponse<OrderDto>> {
    try {
      const queryParams = new URLSearchParams();

      if (params.page) queryParams.append('page', params.page.toString());
      if (params.pageSize) queryParams.append('pageSize', params.pageSize.toString());
      if (params.customerId) queryParams.append('customerId', params.customerId);
      if (params.status) queryParams.append('status', this.getOrderStatusString(params.status));
      if (params.fromDate) queryParams.append('fromDate', params.fromDate);
      if (params.toDate) queryParams.append('toDate', params.toDate);
      if (params.orderNumber) queryParams.append('orderNumber', params.orderNumber);

      const url = `${OrderService.ENDPOINTS.GET_ALL}${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;
      const response = await this.get<PagedResponse<OrderDto>>(url);
      
      return response;
    } catch (error) {
      console.error('Error fetching orders:', error);
      throw error;
    }
  }

  // Get single order by ID
  async getOrder(id: string): Promise<OrderDto> {
    try {
      const response = await this.get<OrderDto>(OrderService.ENDPOINTS.GET_BY_ID(id));
      return response;
    } catch (error) {
      console.error(`Error fetching order ${id}:`, error);
      throw error;
    }
  }

  // Create new order
  async createOrder(orderData: CreateOrderDto): Promise<OrderDto> {
    try {
      const response = await this.post<OrderDto>(OrderService.ENDPOINTS.CREATE, orderData);
      return response;
    } catch (error) {
      console.error('Error creating order:', error);
      throw error;
    }
  }

  // Update existing order
  async updateOrder(id: string, orderData: UpdateOrderDto): Promise<OrderDto> {
    try {
      const response = await this.put<OrderDto>(OrderService.ENDPOINTS.UPDATE(id), orderData);
      return response;
    } catch (error) {
      console.error(`Error updating order ${id}:`, error);
      throw error;
    }
  }

  // Delete order (soft delete)
  async deleteOrder(id: string): Promise<void> {
    try {
      await this.delete(OrderService.ENDPOINTS.DELETE(id));
    } catch (error) {
      console.error(`Error deleting order ${id}:`, error);
      throw error;
    }
  }

  // Update order status
  async updateOrderStatus(id: string, status: OrderStatus, notes?: string): Promise<OrderDto> {
    try {
      const updateData: UpdateOrderDto = {
        status,
        notes
      };
      return await this.updateOrder(id, updateData);
    } catch (error) {
      console.error(`Error updating order status for ${id}:`, error);
      throw error;
    }
  }

  // Get orders for specific customer
  async getCustomerOrders(customerId: string, params: Omit<GetOrdersParams, 'customerId'> = {}): Promise<PagedResponse<OrderDto>> {
    try {
      return await this.getOrders({ ...params, customerId });
    } catch (error) {
      console.error(`Error fetching orders for customer ${customerId}:`, error);
      throw error;
    }
  }

  // Get orders by status
  async getOrdersByStatus(status: OrderStatus, params: Omit<GetOrdersParams, 'status'> = {}): Promise<PagedResponse<OrderDto>> {
    try {
      return await this.getOrders({ ...params, status });
    } catch (error) {
      console.error(`Error fetching orders by status ${status}:`, error);
      throw error;
    }
  }

  // Helper method to get human-readable order status
  getOrderStatusLabel(status: OrderStatus): string {
    const statusLabels: Record<OrderStatus, string> = {
      [1]: 'Pending',
      [2]: 'Confirmed', 
      [3]: 'Processing',
      [4]: 'Shipped',
      [5]: 'Delivered',
      [6]: 'Cancelled'
    };
    return statusLabels[status] || 'Unknown';
  }

  // Helper method to get order status color for UI
  getOrderStatusColor(status: OrderStatus): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' {
    const statusColors: Record<OrderStatus, 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning'> = {
      [1]: 'warning',    // Pending
      [2]: 'info',       // Confirmed
      [3]: 'primary',    // Processing
      [4]: 'secondary',  // Shipped
      [5]: 'success',    // Delivered
      [6]: 'error'       // Cancelled
    };
    return statusColors[status] || 'default';
  }

  // Helper method to convert OrderStatus enum to string for API
  private getOrderStatusString(status: OrderStatus): string {
    const statusStrings: Record<OrderStatus, string> = {
      [1]: 'Pending',
      [2]: 'Confirmed',
      [3]: 'Processing', 
      [4]: 'Shipped',
      [5]: 'Delivered',
      [6]: 'Cancelled'
    };
    return statusStrings[status] || 'Pending';
  }

  // Test connection to OrderService
  async testConnection(): Promise<string> {
    try {
      const response = await this.get<string>(OrderService.ENDPOINTS.TEST);
      return response;
    } catch (error) {
      console.error('Error testing OrderService connection:', error);
      throw error;
    }
  }

  // Health check
  async healthCheck(): Promise<{ status: string; timestamp: string }> {
    try {
      const response = await this.get<{ status: string; timestamp: string }>(OrderService.ENDPOINTS.HEALTH);
      return response;
    } catch (error) {
      console.error('Error checking OrderService health:', error);
      throw error;
    }
  }

  // Calculate totals for order items (helper method)
  static calculateOrderTotals(items: { unitPrice: number; quantity: number }[], taxRate: number = 0.1, shippingCost: number = 0) {
    const subtotal = items.reduce((sum, item) => sum + (item.unitPrice * item.quantity), 0);
    const taxAmount = subtotal * taxRate;
    const total = subtotal + taxAmount + shippingCost;

    return {
      subtotal: Number(subtotal.toFixed(2)),
      taxAmount: Number(taxAmount.toFixed(2)),
      shippingCost: Number(shippingCost.toFixed(2)),
      total: Number(total.toFixed(2))
    };
  }
}

export const orderService = new OrderService();