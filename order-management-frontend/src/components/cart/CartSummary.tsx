import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Divider,
  Button,
  Chip
} from '@mui/material';
import {
  ShoppingCart as CartIcon,
  LocalShipping as ShippingIcon
} from '@mui/icons-material';
import { useCart } from '../../contexts/CartContext';

interface CartSummaryProps {
  compact?: boolean;
  showCheckoutButton?: boolean;
  onCheckout?: () => void;
}

export const CartSummary: React.FC<CartSummaryProps> = ({ 
  compact = false, 
  showCheckoutButton = true,
  onCheckout 
}) => {
  const { state } = useCart();
  const { cart } = state;

  const handleCheckout = () => {
    if (onCheckout) {
      onCheckout();
    }
  };

  if (cart.itemCount === 0) {
    return (
      <Card>
        <CardContent sx={{ textAlign: 'center', py: 4 }}>
          <CartIcon sx={{ fontSize: 48, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Your cart is empty
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Add some products to get started
          </Typography>
        </CardContent>
      </Card>
    );
  }

  if (compact) {
    return (
      <Box sx={{ p: 2, bgcolor: 'background.paper', borderRadius: 1, border: '1px solid #eee' }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
          <Typography variant="body2" color="text.secondary">
            {cart.itemCount} {cart.itemCount === 1 ? 'item' : 'items'}
          </Typography>
          <Typography variant="h6" fontWeight={600}>
            ${cart.total.toFixed(2)}
          </Typography>
        </Box>
        
        {showCheckoutButton && (
          <Button
            variant="contained"
            fullWidth
            onClick={handleCheckout}
            disabled={cart.itemCount === 0}
            size="small"
          >
            Checkout
          </Button>
        )}
      </Box>
    );
  }

  return (
    <Card>
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
          <CartIcon sx={{ mr: 1 }} />
          <Typography variant="h6" component="h2">
            Order Summary
          </Typography>
          <Chip
            label={`${cart.itemCount} ${cart.itemCount === 1 ? 'item' : 'items'}`}
            size="small"
            sx={{ ml: 'auto' }}
          />
        </Box>
        
        <Box sx={{ mb: 3 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Subtotal
            </Typography>
            <Typography variant="body2">
              ${cart.subtotal.toFixed(2)}
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
            <Typography variant="body2" color="text.secondary">
              Tax
            </Typography>
            <Typography variant="body2">
              ${cart.taxAmount.toFixed(2)}
            </Typography>
          </Box>
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Box sx={{ display: 'flex', alignItems: 'center' }}>
              <ShippingIcon sx={{ fontSize: 16, mr: 0.5, color: 'text.secondary' }} />
              <Typography variant="body2" color="text.secondary">
                Shipping
              </Typography>
            </Box>
            <Typography variant="body2">
              {cart.shippingCost === 0 ? 'Free' : `$${cart.shippingCost.toFixed(2)}`}
            </Typography>
          </Box>
          
          <Divider sx={{ my: 2 }} />
          
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Typography variant="h6" fontWeight={600}>
              Total
            </Typography>
            <Typography variant="h6" fontWeight={600} color="primary">
              ${cart.total.toFixed(2)}
            </Typography>
          </Box>
        </Box>
        
        {cart.shippingCost === 0 && cart.subtotal > 0 && (
          <Box sx={{ mb: 3, p: 2, bgcolor: 'success.light', borderRadius: 1 }}>
            <Typography variant="caption" color="success.dark">
              🎉 You qualify for free shipping!
            </Typography>
          </Box>
        )}
        
        {showCheckoutButton && (
          <Button
            variant="contained"
            fullWidth
            size="large"
            onClick={handleCheckout}
            disabled={cart.itemCount === 0}
            sx={{ py: 1.5 }}
          >
            Proceed to Checkout
          </Button>
        )}
        
        <Typography variant="caption" color="text.secondary" sx={{ display: 'block', textAlign: 'center', mt: 2 }}>
          Secure checkout powered by SSL encryption
        </Typography>
      </CardContent>
    </Card>
  );
};