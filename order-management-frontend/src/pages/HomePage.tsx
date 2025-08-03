import React from 'react';
import { 
  Typography, 
  Box, 
  Container, 
  Grid, 
  Card, 
  CardContent, 
  CardActions,
  Button,
  Divider,
  Paper,
  List,
  ListItem,
  ListItemIcon,
  ListItemText
} from '@mui/material';
import { 
  ShoppingCart as ShoppingCartIcon,
  Assignment as OrderIcon,
  Inventory as ProductIcon,
  AccountCircle as AccountIcon,
  TrendingUp as TrendingIcon,
  Security as SecurityIcon,
  Speed as SpeedIcon
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const HomePage: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated, user } = useAuth();

  const features = [
    {
      title: 'Product Catalog',
      description: 'Browse and manage your product inventory with advanced filtering and search capabilities.',
      icon: <ProductIcon fontSize="large" color="primary" />,
      action: () => navigate(isAuthenticated ? '/products' : '/login')
    },
    {
      title: 'Order Management',
      description: 'Create, track, and manage orders with real-time status updates and detailed history.',
      icon: <OrderIcon fontSize="large" color="primary" />,
      action: () => navigate(isAuthenticated ? '/orders' : '/login')
    },
    {
      title: 'Shopping Cart',
      description: 'Add products to your cart and complete purchases with our streamlined checkout process.',
      icon: <ShoppingCartIcon fontSize="large" color="primary" />,
      action: () => navigate(isAuthenticated ? '/products' : '/login')
    }
  ];

  const benefits = [
    { text: 'Real-time inventory tracking', icon: <TrendingIcon /> },
    { text: 'Secure authentication system', icon: <SecurityIcon /> },
    { text: 'Fast and responsive interface', icon: <SpeedIcon /> },
    { text: 'Comprehensive order history', icon: <OrderIcon /> }
  ];

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Hero Section */}
      <Box textAlign="center" mb={6}>
        <Typography variant="h2" component="h1" gutterBottom fontWeight="bold">
          Order Management System
        </Typography>
        <Typography variant="h5" color="text.secondary" sx={{ maxWidth: 800, mx: 'auto', mb: 4 }}>
          {isAuthenticated 
            ? `Welcome back, ${user?.fullName}! Manage your orders and products efficiently.`
            : 'Streamline your business operations with our comprehensive order management solution.'
          }
        </Typography>
        
        {!isAuthenticated && (
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', flexWrap: 'wrap' }}>
            <Button 
              variant="contained" 
              size="large" 
              onClick={() => navigate('/login')}
              startIcon={<AccountIcon />}
            >
              Login
            </Button>
            <Button 
              variant="outlined" 
              size="large" 
              onClick={() => navigate('/register')}
            >
              Create Account
            </Button>
          </Box>
        )}
      </Box>

      {/* Features Section */}
      <Typography variant="h4" component="h2" textAlign="center" gutterBottom mb={4}>
        Key Features
      </Typography>
      
      <Grid container spacing={4} mb={6}>
        {features.map((feature, index) => (
          <Grid item xs={12} md={4} key={index}>
            <Card 
              sx={{ 
                height: '100%', 
                display: 'flex', 
                flexDirection: 'column',
                transition: 'transform 0.2s ease-in-out',
                '&:hover': {
                  transform: 'translateY(-4px)',
                  boxShadow: 4
                }
              }}
            >
              <CardContent sx={{ flexGrow: 1, textAlign: 'center', py: 3 }}>
                <Box mb={2}>
                  {feature.icon}
                </Box>
                <Typography variant="h6" component="h3" gutterBottom>
                  {feature.title}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  {feature.description}
                </Typography>
              </CardContent>
              <CardActions sx={{ justifyContent: 'center', pb: 2 }}>
                <Button 
                  variant="outlined" 
                  onClick={feature.action}
                  fullWidth
                  sx={{ mx: 2 }}
                >
                  {isAuthenticated ? 'Go to ' + feature.title : 'Get Started'}
                </Button>
              </CardActions>
            </Card>
          </Grid>
        ))}
      </Grid>

      <Divider sx={{ my: 4 }} />

      {/* Benefits Section */}
      <Grid container spacing={4} alignItems="center">
        <Grid item xs={12} md={6}>
          <Typography variant="h4" component="h2" gutterBottom>
            Why Choose Our System?
          </Typography>
          <Typography variant="body1" color="text.secondary" mb={3}>
            Built with modern technologies and best practices to provide a reliable, 
            scalable, and user-friendly solution for your business needs.
          </Typography>
          
          <Paper variant="outlined" sx={{ p: 0 }}>
            <List>
              {benefits.map((benefit, index) => (
                <ListItem key={index}>
                  <ListItemIcon>
                    {benefit.icon}
                  </ListItemIcon>
                  <ListItemText primary={benefit.text} />
                </ListItem>
              ))}
            </List>
          </Paper>
        </Grid>
        
        <Grid item xs={12} md={6}>
          <Box
            component="img"
            src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 400 300'%3E%3Crect width='400' height='300' fill='%23f5f5f5'/%3E%3Ctext x='200' y='150' text-anchor='middle' fill='%23666' font-family='Arial' font-size='16'%3EOrder Management%3C/text%3E%3C/svg%3E"
            alt="Order Management Illustration"
            sx={{
              width: '100%',
              height: 'auto',
              maxHeight: 300,
              borderRadius: 2,
              boxShadow: 2
            }}
          />
        </Grid>
      </Grid>

      {/* Call to Action */}
      {!isAuthenticated && (
        <Box textAlign="center" mt={6} py={4}>
          <Typography variant="h5" gutterBottom>
            Ready to get started?
          </Typography>
          <Typography variant="body1" color="text.secondary" mb={3}>
            Join thousands of businesses already using our order management system.
          </Typography>
          <Button 
            variant="contained" 
            size="large" 
            onClick={() => navigate('/register')}
            sx={{ minWidth: 200 }}
          >
            Start Free Trial
          </Button>
        </Box>
      )}
    </Container>
  );
};

export default HomePage;