import React from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
  Drawer,
  IconButton,
  Badge
} from '@mui/material';
import {
  Close as CloseIcon,
  ShoppingCart as CartIcon,
  ArrowBack as BackIcon
} from '@mui/icons-material';
import { useCart } from '../../contexts/CartContext';
import { CartItem } from './CartItem';
import { CartSummary } from './CartSummary';

interface CartProps {
  open?: boolean;
  onClose?: () => void;
  onCheckout?: () => void;
  variant?: 'drawer' | 'page';
}

export const Cart: React.FC<CartProps> = ({ 
  open = false, 
  onClose, 
  onCheckout,
  variant = 'drawer'
}) => {
  const { state, clearCart } = useCart();
  const { cart, error } = state;

  const handleCheckout = () => {
    if (onCheckout) {
      onCheckout();
    }
    if (onClose) {
      onClose();
    }
  };

  const handleClearCart = () => {
    clearCart();
  };

  const cartContent = (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box sx={{ p: 2, borderBottom: '1px solid #eee' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center' }}>
            {variant === 'page' ? (
              <IconButton onClick={onClose} sx={{ mr: 1 }}>
                <BackIcon />
              </IconButton>
            ) : null}
            <Typography variant="h6" component="h1">
              Shopping Cart
            </Typography>
            <Badge badgeContent={cart.itemCount} color="primary" sx={{ ml: 2 }}>
              <CartIcon />
            </Badge>
          </Box>
          
          {variant === 'drawer' && onClose && (
            <IconButton onClick={onClose} aria-label="Close cart">
              <CloseIcon />
            </IconButton>
          )}
        </Box>
        
        {cart.itemCount > 0 && (
          <Button
            variant="text"
            color="error"
            size="small"
            onClick={handleClearCart}
            sx={{ mt: 1 }}
          >
            Clear Cart
          </Button>
        )}
      </Box>

      {/* Error Display */}
      {error && (
        <Box sx={{ p: 2 }}>
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        </Box>
      )}

      {/* Cart Items */}
      <Box sx={{ flex: 1, overflow: 'auto', p: 2 }}>
        {cart.itemCount === 0 ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <CartIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              Your cart is empty
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
              Start shopping to add items to your cart
            </Typography>
            {onClose && (
              <Button
                variant="contained"
                onClick={onClose}
                startIcon={<BackIcon />}
              >
                Continue Shopping
              </Button>
            )}
          </Box>
        ) : (
          <Box>
            {cart.items.map((item) => (
              <CartItem key={item.product.id} item={item} />
            ))}
          </Box>
        )}
      </Box>

      {/* Cart Summary */}
      {cart.itemCount > 0 && (
        <Box sx={{ p: 2, borderTop: '1px solid #eee' }}>
          <CartSummary
            compact={variant === 'drawer'}
            showCheckoutButton={true}
            onCheckout={handleCheckout}
          />
          
          {variant === 'drawer' && onClose && (
            <Button
              variant="outlined"
              fullWidth
              onClick={onClose}
              sx={{ mt: 2 }}
              startIcon={<BackIcon />}
            >
              Continue Shopping
            </Button>
          )}
        </Box>
      )}
    </Box>
  );

  if (variant === 'drawer') {
    return (
      <Drawer
        anchor="right"
        open={open}
        onClose={onClose}
        PaperProps={{
          sx: { width: { xs: '100%', sm: 400 } }
        }}
      >
        {cartContent}
      </Drawer>
    );
  }

  return (
    <Box sx={{ width: '100%', minHeight: '80vh' }}>
      {cartContent}
    </Box>
  );
};

// Cart Icon component for header/navigation
interface CartIconButtonProps {
  onClick: () => void;
}

export const CartIconButton: React.FC<CartIconButtonProps> = ({ onClick }) => {
  const { state } = useCart();
  const { cart } = state;

  return (
    <IconButton
      onClick={onClick}
      color="inherit"
      aria-label="Open shopping cart"
    >
      <Badge badgeContent={cart.itemCount} color="error">
        <CartIcon />
      </Badge>
    </IconButton>
  );
};