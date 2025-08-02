import React, { useEffect, useState } from 'react';
import { 
  Typography, 
  Box, 
  Card, 
  CardContent, 
  Button, 
  Chip,
  Paper,
  Skeleton,
  List,
  ListItem,
  ListItemText,
  Divider
} from '@mui/material';
import { 
  ShoppingCart as CartIcon,
  Inventory as ProductsIcon,
  Receipt as OrdersIcon,
  TrendingUp as TrendingIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../contexts/CartContext';
import { orderService } from '../services/orderService';
import { productService } from '../services/productService';
import type { OrderDto } from '../types/entities';

const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const { user, isAuthenticated } = useAuth();
  const { state: cartState } = useCart();
  const cart = cartState.cart;
  const [recentOrders, setRecentOrders] = useState<OrderDto[]>([]);
  const [stats, setStats] = useState({
    totalOrders: 0,
    totalProducts: 0,
    loading: true
  });

  useEffect(() => {
    const loadDashboardData = async () => {
      if (!isAuthenticated) {
        setStats(prev => ({ ...prev, loading: false }));
        return;
      }

      try {
        // Load recent orders (last 5)
        const ordersResponse = await orderService.getOrders({ pageSize: 5 });
        setRecentOrders(ordersResponse.items);

        // Load basic stats
        const productsResponse = await productService.getProducts({ pageSize: 1 });
        
        setStats({
          totalOrders: ordersResponse.totalCount,
          totalProducts: productsResponse.totalCount,
          loading: false
        });
      } catch (error) {
        setStats(prev => ({ ...prev, loading: false }));
      }
    };

    loadDashboardData();
  }, [isAuthenticated]);

  const getOrderStatusColor = (status: number) => {
    switch (status) {
      case 1: return 'warning';
      case 2: return 'success';
      case 3: return 'error';
      default: return 'default';
    }
  };

  const getOrderStatusLabel = (status: number) => {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Completed';
      case 3: return 'Cancelled';
      default: return 'Unknown';
    }
  };

  if (!isAuthenticated) {
    return (
      <Box sx={{ 
        width: '100%', 
        py: 8, 
        px: { xs: 2, sm: 4, md: 6, lg: 8 },
        textAlign: 'center' 
      }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Order Management System
        </Typography>
        <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 600, mx: 'auto', mb: 4 }}>
          Manage your orders, browse products, and track your purchase history with our comprehensive order management platform.
        </Typography>
        <Box sx={{ mt: 4, display: 'flex', gap: 2, justifyContent: 'center' }}>
          <Button 
            variant="contained" 
            size="large"
            onClick={() => navigate('/login')}
          >
            Login
          </Button>
          <Button 
            variant="outlined" 
            size="large"
            onClick={() => navigate('/register')}
          >
            Register
          </Button>
        </Box>
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%', py: 3 }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Welcome back, {user?.fullName}!
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Here's an overview of your account activity
      </Typography>

      {/* Quick Stats Cards */}
      <Box sx={{ 
        display: 'grid', 
        gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: 'repeat(4, 1fr)' },
        gap: 3,
        mb: 4 
      }}>
        <Card>
          <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <CartIcon color="primary" sx={{ fontSize: 40 }} />
            <Box>
              <Typography variant="h6">{cart.itemCount}</Typography>
              <Typography variant="body2" color="text.secondary">
                Items in Cart
              </Typography>
            </Box>
          </CardContent>
        </Card>
        
        <Card>
          <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <OrdersIcon color="secondary" sx={{ fontSize: 40 }} />
            <Box>
              {stats.loading ? (
                <Skeleton width={40} height={32} />
              ) : (
                <Typography variant="h6">{stats.totalOrders}</Typography>
              )}
              <Typography variant="body2" color="text.secondary">
                Total Orders
              </Typography>
            </Box>
          </CardContent>
        </Card>

        <Card>
          <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <ProductsIcon color="success" sx={{ fontSize: 40 }} />
            <Box>
              {stats.loading ? (
                <Skeleton width={40} height={32} />
              ) : (
                <Typography variant="h6">{stats.totalProducts}</Typography>
              )}
              <Typography variant="body2" color="text.secondary">
                Available Products
              </Typography>
            </Box>
          </CardContent>
        </Card>

        <Card>
          <CardContent sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            <TrendingIcon color="info" sx={{ fontSize: 40 }} />
            <Box>
              <Typography variant="h6">${cart.total.toFixed(2)}</Typography>
              <Typography variant="body2" color="text.secondary">
                Cart Total
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Box>

      {/* Quick Actions and Recent Orders */}
      <Box sx={{ 
        display: 'grid', 
        gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' },
        gap: 3 
      }}>
        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Quick Actions
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            <Button 
              variant="contained" 
              startIcon={<ProductsIcon />}
              onClick={() => navigate('/products')}
              fullWidth
            >
              Browse Products
            </Button>
            <Button 
              variant="outlined" 
              startIcon={<OrdersIcon />}
              onClick={() => navigate('/orders')}
              fullWidth
            >
              View My Orders
            </Button>
            {cart.itemCount > 0 && (
              <Button 
                variant="contained" 
                color="secondary"
                startIcon={<CartIcon />}
                onClick={() => navigate('/checkout')}
                fullWidth
              >
                Checkout ({cart.itemCount} items)
              </Button>
            )}
          </Box>
        </Paper>

        <Paper sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Recent Orders
          </Typography>
          {stats.loading ? (
            <Box>
              {[1, 2, 3].map((i) => (
                <Box key={i} sx={{ mb: 2 }}>
                  <Skeleton variant="text" width="100%" height={40} />
                </Box>
              ))}
            </Box>
          ) : recentOrders.length > 0 ? (
            <List dense>
              {recentOrders.map((order, index) => (
                <Box key={order.id}>
                  <ListItem sx={{ px: 0 }}>
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                          <Typography variant="body2" fontWeight="medium">
                            Order #{order.orderNumber}
                          </Typography>
                          <Chip 
                            label={getOrderStatusLabel(order.status)} 
                            size="small"
                            color={getOrderStatusColor(order.status) as any}
                          />
                        </Box>
                      }
                      secondary={
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mt: 0.5 }}>
                          <Typography variant="caption" color="text.secondary">
                            {new Date(order.orderDate).toLocaleDateString()}
                          </Typography>
                          <Typography variant="caption" fontWeight="medium">
                            ${order.totalAmount.toFixed(2)}
                          </Typography>
                        </Box>
                      }
                    />
                  </ListItem>
                  {index < recentOrders.length - 1 && <Divider />}
                </Box>
              ))}
            </List>
          ) : (
            <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 2 }}>
              No orders yet. Start shopping to see your order history here!
            </Typography>
          )}
          
          {recentOrders.length > 0 && (
            <Button 
              variant="text" 
              size="small" 
              onClick={() => navigate('/orders')}
              sx={{ mt: 1 }}
            >
              View All Orders
            </Button>
          )}
        </Paper>
      </Box>
    </Box>
  );
};

export default HomePage;