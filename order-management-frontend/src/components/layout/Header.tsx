import React from 'react';
import { 
  AppBar, 
  Toolbar, 
  Typography, 
  Button, 
  Box, 
  IconButton,
  Badge,
  Chip,
  CircularProgress
} from '@mui/material';
import { 
  ShoppingCart as ShoppingCartIcon,
  AccountCircle as AccountCircleIcon,
  ExitToApp as LogoutIcon
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';

const Header: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  
  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const handleLogout = () => {
    logout();
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
          {isLoading ? (
            <CircularProgress size={24} color="inherit" />
          ) : isAuthenticated ? (
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
              
              {/* Usuario info */}
              {user && (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <AccountCircleIcon />
                  <Chip
                    label={user.fullName}
                    variant="outlined"
                    size="small"
                    sx={{ 
                      color: 'white', 
                      borderColor: 'white',
                      '& .MuiChip-label': { color: 'white' }
                    }}
                  />
                </Box>
              )}
              
              <IconButton 
                color="inherit" 
                onClick={handleLogout}
                title="Cerrar sesión"
              >
                <LogoutIcon />
              </IconButton>
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