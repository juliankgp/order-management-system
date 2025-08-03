import React, { useState } from 'react';
import { 
  Toolbar, 
  Typography, 
  Box, 
  CircularProgress,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Divider,
  useTheme
} from '@mui/material';
import { 
  ExitToApp as LogoutIcon,
  Person as PersonIcon,
  Dashboard as DashboardIcon,
  AutoAwesome as AutoAwesomeIcon
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import { CartIconButton, Cart } from '../cart/Cart';
import {
  StyledAppBar,
  LogoContainer,
  LogoIcon,
  LogoText,
  NavButton,
  ProfileButton,
  ProfileAvatar,
  GetStartedButton
} from './Header.styles';

const Header: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const { user, isAuthenticated, isLoading, logout } = useAuth();
  const [cartOpen, setCartOpen] = useState(false);
  const [profileMenuAnchor, setProfileMenuAnchor] = useState<null | HTMLElement>(null);
  
  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const handleLogout = () => {
    logout();
    navigate('/');
    setProfileMenuAnchor(null);
  };

  const handleCartOpen = () => {
    setCartOpen(true);
  };

  const handleCartClose = () => {
    setCartOpen(false);
  };

  const handleCheckout = () => {
    setCartOpen(false);
    navigate('/checkout');
  };

  const handleProfileMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setProfileMenuAnchor(event.currentTarget);
  };

  const handleProfileMenuClose = () => {
    setProfileMenuAnchor(null);
  };

  const handleProfileNavigation = (path: string) => {
    navigate(path);
    setProfileMenuAnchor(null);
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <>
      <StyledAppBar position="sticky">
        <Toolbar sx={{ minHeight: 70 }}>
          <LogoContainer onClick={() => handleNavigation('/')}>
            <LogoIcon>
              <AutoAwesomeIcon sx={{ color: 'white', fontSize: 24 }} />
            </LogoIcon>
            <LogoText variant="h6">
              OrderFlow Pro
            </LogoText>
          </LogoContainer>
          
          <Box sx={{ flexGrow: 1 }} />
          
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {isLoading ? (
              <CircularProgress size={24} />
            ) : isAuthenticated ? (
              <>
                <NavButton 
                  color="primary"
                  onClick={() => handleNavigation('/products')}
                  variant={location.pathname === '/products' ? 'contained' : 'text'}
                >
                  Products
                </NavButton>
                <NavButton 
                  color="primary"
                  onClick={() => handleNavigation('/orders')}
                  variant={location.pathname === '/orders' ? 'contained' : 'text'}
                >
                  My Orders
                </NavButton>

                <CartIconButton onClick={handleCartOpen} />
                
                {user && (
                  <>
                    <ProfileButton onClick={handleProfileMenuOpen}>
                      <ProfileAvatar>
                        {getInitials(user.fullName)}
                      </ProfileAvatar>
                      <Box sx={{ textAlign: 'left', color: theme.palette.text.primary }}>
                        <Typography variant="body2" fontWeight="600">
                          {user.fullName.split(' ')[0]}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {user.email}
                        </Typography>
                      </Box>
                    </ProfileButton>

                    <Menu
                      anchorEl={profileMenuAnchor}
                      open={Boolean(profileMenuAnchor)}
                      onClose={handleProfileMenuClose}
                      transformOrigin={{ horizontal: 'right', vertical: 'top' }}
                      anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
                      PaperProps={{
                        sx: {
                          mt: 1,
                          borderRadius: 2,
                          minWidth: 200
                        }
                      }}
                    >
                      <MenuItem onClick={() => handleProfileNavigation('/')}>
                        <ListItemIcon>
                          <DashboardIcon fontSize="small" />
                        </ListItemIcon>
                        <ListItemText>Dashboard</ListItemText>
                      </MenuItem>
                      <MenuItem onClick={() => handleProfileNavigation('/profile')}>
                        <ListItemIcon>
                          <PersonIcon fontSize="small" />
                        </ListItemIcon>
                        <ListItemText>Profile Settings</ListItemText>
                      </MenuItem>
                      <Divider />
                      <MenuItem onClick={handleLogout} sx={{ color: 'error.main' }}>
                        <ListItemIcon>
                          <LogoutIcon fontSize="small" color="error" />
                        </ListItemIcon>
                        <ListItemText>Sign Out</ListItemText>
                      </MenuItem>
                    </Menu>
                  </>
                )}
              </>
            ) : (
              <>
                <NavButton 
                  color="primary"
                  onClick={() => handleNavigation('/login')}
                  variant={location.pathname === '/login' ? 'contained' : 'text'}
                >
                  Sign In
                </NavButton>
                <GetStartedButton 
                  variant="contained"
                  onClick={() => handleNavigation('/register')}
                >
                  Get Started
                </GetStartedButton>
              </>
            )}
          </Box>
        </Toolbar>
      </StyledAppBar>
      
      <Cart
        open={cartOpen}
        onClose={handleCartClose}
        onCheckout={handleCheckout}
        variant="drawer"
      />
    </>
  );
};

export default Header;