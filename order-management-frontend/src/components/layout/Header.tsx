import React from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button, 
  Box, 
  IconButton,
  Badge
} from '@mui/material';
import { 
  ShoppingCart as ShoppingCartIcon,
  AccountCircle as AccountCircleIcon 
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';

const Header: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  
  // Simulamos estado de autenticación (se implementará en Phase 2)
  const isAuthenticated = !!localStorage.getItem('authToken');
  
  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const handleLogout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    navigate('/');
  };

  return (
    <AppBar position="sticky">
      <Toolbar>
        <Typography 
          variant="h6" 
          component="div" 
          sx={{ flexGrow: 1, cursor: 'pointer' }}
          onClick={() => handleNavigation('/')}
        >
          Order Management System
        </Typography>
        
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {isAuthenticated ? (
            <>
              <Button 
                color="inherit" 
                onClick={() => handleNavigation('/products')}
                variant={location.pathname === '/products' ? 'outlined' : 'text'}
              >
                Products
              </Button>
              <Button 
                color="inherit" 
                onClick={() => handleNavigation('/orders')}
                variant={location.pathname === '/orders' ? 'outlined' : 'text'}
              >
                My Orders
              </Button>
              <IconButton color="inherit" size="large">
                <Badge badgeContent={0} color="secondary">
                  <ShoppingCartIcon />
                </Badge>
              </IconButton>
              <IconButton color="inherit" size="large">
                <AccountCircleIcon />
              </IconButton>
              <Button color="inherit" onClick={handleLogout}>
                Logout
              </Button>
            </>
          ) : (
            <>
              <Button 
                color="inherit" 
                onClick={() => handleNavigation('/login')}
                variant={location.pathname === '/login' ? 'outlined' : 'text'}
              >
                Login
              </Button>
              <Button 
                color="inherit" 
                onClick={() => handleNavigation('/register')}
                variant={location.pathname === '/register' ? 'outlined' : 'text'}
              >
                Register
              </Button>
            </>
          )}
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Header;