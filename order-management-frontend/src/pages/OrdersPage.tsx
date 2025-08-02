import React, { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
  Pagination,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Paper,
  Skeleton
} from '@mui/material';
import {
  Refresh as RefreshIcon,
  Search as SearchIcon,
  FilterList as FilterIcon
} from '@mui/icons-material';
import { useSearchParams } from 'react-router-dom';
import type { OrderDto, OrderFilters, PagedResponse } from '../types/entities';
import { OrderStatus } from '../types/entities';
import { orderService } from '../services/orderService';
import { useAuth } from '../context/AuthContext';
import { OrderCard } from '../components/order/OrderCard';

const OrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<OrderDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [totalPages, setTotalPages] = useState(0);
  const [totalItems, setTotalItems] = useState(0);
  const [showFilters, setShowFilters] = useState(false);

  const [searchParams, setSearchParams] = useSearchParams();
  const {} = useAuth(); // Keep for future use

  // Get filters from URL
  const getFiltersFromUrl = (): OrderFilters => {
    return {
      page: parseInt(searchParams.get('page') || '1'),
      pageSize: parseInt(searchParams.get('pageSize') || '10'),
      customerId: searchParams.get('customerId') || undefined,
      status: searchParams.get('status') ? parseInt(searchParams.get('status')!) as OrderStatus : undefined,
      fromDate: searchParams.get('fromDate') || undefined,
      toDate: searchParams.get('toDate') || undefined,
      orderNumber: searchParams.get('orderNumber') || undefined
    };
  };

  // Update URL with filters
  const updateUrlWithFilters = (newFilters: Partial<OrderFilters>) => {
    const currentFilters = getFiltersFromUrl();
    const updatedFilters = { ...currentFilters, ...newFilters };
    
    const params = new URLSearchParams();
    
    if (updatedFilters.page && updatedFilters.page > 1) {
      params.set('page', updatedFilters.page.toString());
    }
    if (updatedFilters.pageSize && updatedFilters.pageSize !== 10) {
      params.set('pageSize', updatedFilters.pageSize.toString());
    }
    if (updatedFilters.customerId) {
      params.set('customerId', updatedFilters.customerId);
    }
    if (updatedFilters.status) {
      params.set('status', updatedFilters.status.toString());
    }
    if (updatedFilters.fromDate) {
      params.set('fromDate', updatedFilters.fromDate);
    }
    if (updatedFilters.toDate) {
      params.set('toDate', updatedFilters.toDate);
    }
    if (updatedFilters.orderNumber) {
      params.set('orderNumber', updatedFilters.orderNumber);
    }

    setSearchParams(params);
  };

  // Load orders from API
  const loadOrders = useCallback(async (filters: OrderFilters) => {
    setLoading(true);
    setError(null);

    try {
      const response: PagedResponse<OrderDto> = await orderService.getOrders(filters);
      
      setOrders(response.items);
      setTotalPages(response.totalPages);
      setTotalItems(response.totalCount);
    } catch (error) {
      console.error('Error loading orders:', error);
      setError('Failed to load orders. Please try again.');
    } finally {
      setLoading(false);
    }
  }, []);

  // Load orders when filters change
  useEffect(() => {
    const filters = getFiltersFromUrl();
    loadOrders(filters);
  }, [searchParams, loadOrders]);

  // Handle filter changes
  const handleFilterChange = (filterName: keyof OrderFilters, value: any) => {
    updateUrlWithFilters({ [filterName]: value, page: 1 }); // Reset to page 1 when filtering
  };

  // Handle page change
  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    updateUrlWithFilters({ page });
  };

  // Handle refresh
  const handleRefresh = () => {
    const filters = getFiltersFromUrl();
    loadOrders(filters);
  };

  // Handle order status update
  const handleStatusUpdate = async (orderId: string, newStatus: OrderStatus) => {
    try {
      await orderService.updateOrderStatus(orderId, newStatus);
      
      // Refresh orders to show updated status
      const filters = getFiltersFromUrl();
      loadOrders(filters);
      
      // Show success message (you might want to add a snackbar here)
    } catch (error) {
      console.error('Error updating order status:', error);
      setError('Failed to update order status. Please try again.');
    }
  };

  // Get current filters
  const currentFilters = getFiltersFromUrl();

  return (
    <Box sx={{ width: '100%', p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
        <Typography variant="h4" component="h1">
          Orders
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={<FilterIcon />}
            onClick={() => setShowFilters(!showFilters)}
          >
            Filters
          </Button>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={handleRefresh}
            disabled={loading}
          >
            Refresh
          </Button>
        </Box>
      </Box>

      {/* Filters */}
      {showFilters && (
        <Paper elevation={1} sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Filters
          </Typography>
          <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 2 }}>
            <TextField
              label="Order Number"
              value={currentFilters.orderNumber || ''}
              onChange={(e) => handleFilterChange('orderNumber', e.target.value || undefined)}
              size="small"
              InputProps={{
                startAdornment: <SearchIcon sx={{ color: 'text.secondary', mr: 1 }} />
              }}
            />
            
            <FormControl size="small">
              <InputLabel>Status</InputLabel>
              <Select
                value={currentFilters.status || ''}
                label="Status"
                onChange={(e) => handleFilterChange('status', e.target.value || undefined)}
              >
                <MenuItem value="">All Statuses</MenuItem>
                <MenuItem value={1}>Pending</MenuItem>
                <MenuItem value={2}>Confirmed</MenuItem>
                <MenuItem value={3}>Processing</MenuItem>
                <MenuItem value={4}>Shipped</MenuItem>
                <MenuItem value={5}>Delivered</MenuItem>
                <MenuItem value={6}>Cancelled</MenuItem>
              </Select>
            </FormControl>
            
            <TextField
              label="From Date"
              type="date"
              value={currentFilters.fromDate || ''}
              onChange={(e) => handleFilterChange('fromDate', e.target.value || undefined)}
              size="small"
              InputLabelProps={{ shrink: true }}
            />
            
            <TextField
              label="To Date"
              type="date"
              value={currentFilters.toDate || ''}
              onChange={(e) => handleFilterChange('toDate', e.target.value || undefined)}
              size="small"
              InputLabelProps={{ shrink: true }}
            />
            
            <FormControl size="small">
              <InputLabel>Page Size</InputLabel>
              <Select
                value={currentFilters.pageSize || 10}
                label="Page Size"
                onChange={(e) => handleFilterChange('pageSize', e.target.value)}
              >
                <MenuItem value={5}>5 per page</MenuItem>
                <MenuItem value={10}>10 per page</MenuItem>
                <MenuItem value={25}>25 per page</MenuItem>
                <MenuItem value={50}>50 per page</MenuItem>
              </Select>
            </FormControl>
          </Box>
        </Paper>
      )}

      {/* Error Display */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Orders Count */}
      {!loading && orders.length > 0 && (
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Showing {orders.length} of {totalItems} orders
        </Typography>
      )}

      {/* Loading */}
      {loading && (
        <Box>
          {[...Array(3)].map((_, index) => (
            <Skeleton key={index} variant="rectangular" height={200} sx={{ mb: 2, borderRadius: 1 }} />
          ))}
        </Box>
      )}

      {/* Orders List */}
      {!loading && orders.length === 0 && !error && (
        <Paper elevation={1} sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="h6" color="text.secondary" gutterBottom>
            No orders found
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {Object.keys(currentFilters).some(key => currentFilters[key as keyof OrderFilters])
              ? 'Try adjusting your filters to see more orders.'
              : 'You haven\'t placed any orders yet.'}
          </Typography>
        </Paper>
      )}

      {!loading && orders.length > 0 && (
        <Box>
          {orders.map((order) => (
            <OrderCard
              key={order.id}
              order={order}
              onStatusUpdate={handleStatusUpdate}
              onViewDetails={() => {
                // You might want to navigate to a detailed order view
              }}
            />
          ))}
        </Box>
      )}

      {/* Pagination */}
      {!loading && totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
          <Pagination
            count={totalPages}
            page={currentFilters.page || 1}
            onChange={handlePageChange}
            color="primary"
            size="large"
          />
        </Box>
      )}
    </Box>
  );
};

export default OrdersPage;