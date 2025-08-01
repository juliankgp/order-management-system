import React, { useState } from 'react';
import {
  Box,
  Typography,
  Button,
  Divider,
  Alert,
  Paper,
  Chip,
  IconButton,
  Drawer,
  AppBar,
  Toolbar,
} from '@mui/material';
import {
  ShoppingCart as CartIcon,
  Close as CloseIcon,
  LocalShipping as ShippingIcon,
  Receipt as ReceiptIcon,
} from '@mui/icons-material';
import { useCart } from '../../contexts/CartContext';
import CartItem from './CartItem';

export interface CartProps {
  open?: boolean;
  onClose?: () => void;
  onCheckout?: () => void;
  variant?: 'drawer' | 'inline';
}

const Cart: React.FC<CartProps> = ({
  open = false,
  onClose,
  onCheckout,
  variant = 'drawer'
}) => {
  const { cart, updateQuantity, removeItem, clearCart, hasItems } = useCart();
  const [loading, setLoading] = useState(false);

  const handleUpdateQuantity = async (productId: string, quantity: number) => {
    setLoading(true);
    try {
      updateQuantity(productId, quantity);
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveItem = async (productId: string) => {
    setLoading(true);
    try {
      removeItem(productId);
    } finally {
      setLoading(false);
    }
  };

  const handleClearCart = () => {
    if (window.confirm('Are you sure you want to clear your cart?')) {
      clearCart();
    }
  };

  const handleCheckout = () => {
    if (onCheckout) {
      onCheckout();
    }
    if (onClose) {
      onClose();
    }
  };

  const CartContent = () => (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      {/* Header */}
      <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <CartIcon />
            <Typography variant="h6">
              Shopping Cart
            </Typography>
            {hasItems && (
              <Chip 
                label={cart.itemCount} 
                size="small" 
                color="primary" 
              />
            )}
          </Box>
          {variant === 'drawer' && onClose && (
            <IconButton onClick={onClose}>
              <CloseIcon />
            </IconButton>
          )}
        </Box>
      </Box>

      {/* Cart Items */}
      <Box sx={{ flex: 1, overflow: 'auto', p: 2 }}>
        {!hasItems ? (
          <Box sx={{ textAlign: 'center', py: 4 }}>
            <CartIcon sx={{ fontSize: 64, color: 'grey.400', mb: 2 }} />
            <Typography variant="h6" color="text.secondary" gutterBottom>
              Your cart is empty
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Add some products to get started!
            </Typography>
          </Box>
        ) : (
          <>
            {cart.items.map((item) => (
              <CartItem
                key={item.product.id}
                item={item}
                onUpdateQuantity={handleUpdateQuantity}
                onRemove={handleRemoveItem}
                loading={loading}
              />
            ))}

            {/* Clear Cart Button */}
            <Box sx={{ textAlign: 'right', mt: 2 }}>
              <Button
                variant="outlined"
                color="secondary"
                onClick={handleClearCart}
                disabled={loading}
                size="small"
              >
                Clear Cart
              </Button>
            </Box>
          </>
        )}
      </Box>

      {/* Cart Summary */}
      {hasItems && (
        <Paper sx={{ p: 2, m: 2, mt: 0 }} elevation={1}>
          <Typography variant="h6" gutterBottom>
            Order Summary
          </Typography>
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2">
              Subtotal ({cart.itemCount} items)
            </Typography>
            <Typography variant="body2">
              ${cart.subtotal.toFixed(2)}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2">
              Tax (10%)
            </Typography>
            <Typography variant="body2">
              ${cart.taxAmount.toFixed(2)}
            </Typography>
          </Box>

          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
              <ShippingIcon sx={{ fontSize: 16 }} />
              <Typography variant="body2">
                Shipping
              </Typography>
            </Box>
            <Typography variant="body2">
              {cart.shippingCost === 0 ? 'FREE' : `$${cart.shippingCost.toFixed(2)}`}
            </Typography>
          </Box>

          {cart.subtotal > 100 && cart.shippingCost === 0 && (
            <Alert severity="success" sx={{ mb: 2, py: 0 }}>
              <Typography variant="caption">
                🎉 You qualify for free shipping!
              </Typography>
            </Alert>
          )}

          {cart.subtotal <= 100 && cart.shippingCost > 0 && (
            <Alert severity="info" sx={{ mb: 2, py: 0 }}>
              <Typography variant="caption">
                Add ${(100 - cart.subtotal).toFixed(2)} more for free shipping
              </Typography>
            </Alert>
          )}

          <Divider sx={{ my: 1 }} />

          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Total
            </Typography>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              ${cart.total.toFixed(2)}
            </Typography>
          </Box>

          <Button
            variant="contained"
            fullWidth
            size="large"
            onClick={handleCheckout}
            disabled={loading || !hasItems}
            startIcon={<ReceiptIcon />}
            sx={{ mb: 1 }}
          >
            Proceed to Checkout
          </Button>

          <Typography variant="caption" color="text.secondary" sx={{ display: 'block', textAlign: 'center' }}>
            Tax and shipping calculated at checkout
          </Typography>
        </Paper>
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
        <CartContent />
      </Drawer>
    );
  }

  return (
    <Box sx={{ width: '100%', maxWidth: 400 }}>
      <CartContent />
    </Box>
  );
};

export default Cart;