import React, { useState } from 'react';
import {
  Box,
  Stepper,
  Step,
  StepLabel,
  Button,
  Typography,
  Paper,
  Alert,
  CircularProgress,
  Divider
} from '@mui/material';
import {
  ArrowBack as BackIcon,
  Payment as PaymentIcon,
  CheckCircle as CheckCircleIcon
} from '@mui/icons-material';
import { useCart } from '../../contexts/CartContext';
import { useAuth } from '../../context/AuthContext';
import { orderService } from '../../services/orderService';
import type { CreateOrderDto, Cart, CartItem as CartItemType } from '../../types/entities';
import { CartSummary } from '../cart/CartSummary';
import { CartItem } from '../cart/CartItem';

interface CheckoutProps {
  onBack?: () => void;
  onComplete?: (orderId: string) => void;
}

const steps = ['Review Order', 'Payment', 'Confirmation'];

export const Checkout: React.FC<CheckoutProps> = ({ onBack, onComplete }) => {
  const [activeStep, setActiveStep] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [orderNotes, setOrderNotes] = useState('');
  const [createdOrderId, setCreatedOrderId] = useState<string | null>(null);

  const { state: cartState, clearCart } = useCart();
  const { user } = useAuth();
  const { cart } = cartState;

  const handleNext = async () => {
    if (activeStep === steps.length - 1) {
      // Final step - complete checkout
      if (onComplete && createdOrderId) {
        onComplete(createdOrderId);
      }
      return;
    }

    if (activeStep === 1) {
      // Payment step - create order
      await handleCreateOrder();
    } else {
      setActiveStep((prevActiveStep) => prevActiveStep + 1);
    }
  };

  const handleBack = () => {
    if (activeStep === 0 && onBack) {
      onBack();
    } else {
      setActiveStep((prevActiveStep) => prevActiveStep - 1);
    }
  };

  const handleCreateOrder = async () => {
    if (!user) {
      setError('You must be logged in to place an order');
      return;
    }

    if (cart.itemCount === 0) {
      setError('Your cart is empty');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const orderData: CreateOrderDto = {
        customerId: user.id,
        notes: orderNotes || undefined,
        items: cart.items.map(item => ({
          productId: item.product.id,
          quantity: item.quantity
        }))
      };

      const createdOrder = await orderService.createOrder(orderData);
      setCreatedOrderId(createdOrder.id);
      
      // Clear the cart after successful order creation
      clearCart();
      
      // Move to confirmation step
      setActiveStep(2);
    } catch (error) {
      console.error('Error creating order:', error);
      setError('Failed to create order. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  const getStepContent = (step: number) => {
    switch (step) {
      case 0:
        return <ReviewOrderStep cart={cart} orderNotes={orderNotes} setOrderNotes={setOrderNotes} />;
      case 1:
        return <PaymentStep cart={cart} loading={loading} />;
      case 2:
        return <ConfirmationStep orderId={createdOrderId} />;
      default:
        return 'Unknown step';
    }
  };

  return (
    <Box sx={{ width: '100%', maxWidth: 800, mx: 'auto', p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 4 }}>
        <Button
          startIcon={<BackIcon />}
          onClick={handleBack}
          sx={{ mr: 2 }}
        >
          {activeStep === 0 ? 'Back to Cart' : 'Back'}
        </Button>
        <Typography variant="h4" component="h1">
          Checkout
        </Typography>
      </Box>

      {/* Stepper */}
      <Stepper activeStep={activeStep} sx={{ mb: 4 }}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      {/* Error Display */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {/* Step Content */}
      <Paper elevation={1} sx={{ p: 3, mb: 3 }}>
        {getStepContent(activeStep)}
      </Paper>

      {/* Navigation */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <Button
          onClick={handleBack}
          disabled={loading}
        >
          {activeStep === 0 ? 'Back to Cart' : 'Back'}
        </Button>
        
        <Button
          variant="contained"
          onClick={handleNext}
          disabled={loading || (activeStep === 1 && cart.itemCount === 0)}
          startIcon={loading ? <CircularProgress size={20} /> : null}
        >
          {loading 
            ? 'Processing...' 
            : activeStep === steps.length - 1 
              ? 'Complete' 
              : activeStep === 1 
                ? 'Place Order' 
                : 'Next'
          }
        </Button>
      </Box>
    </Box>
  );
};

// Step Components

interface ReviewOrderStepProps {
  cart: Cart;
  orderNotes: string;
  setOrderNotes: (notes: string) => void;
}

const ReviewOrderStep: React.FC<ReviewOrderStepProps> = ({ cart, orderNotes, setOrderNotes }) => {
  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Review Your Order
      </Typography>
      
      {cart.itemCount === 0 ? (
        <Alert severity="warning">
          Your cart is empty. Please add items before proceeding.
        </Alert>
      ) : (
        <Box>
          {/* Order Items */}
          <Box sx={{ mb: 3 }}>
            {cart.items.map((item: CartItemType) => (
              <CartItem key={item.product.id} item={item} compact />
            ))}
          </Box>
          
          <Divider sx={{ my: 3 }} />
          
          {/* Order Summary */}
          <CartSummary compact={false} showCheckoutButton={false} />
          
          <Divider sx={{ my: 3 }} />
          
          {/* Order Notes */}
          <Box>
            <Typography variant="subtitle1" gutterBottom>
              Order Notes (Optional)
            </Typography>
            <textarea
              value={orderNotes}
              onChange={(e) => setOrderNotes(e.target.value)}
              placeholder="Add any special instructions or notes for your order..."
              style={{
                width: '100%',
                minHeight: '80px',
                padding: '12px',
                border: '1px solid #ddd',
                borderRadius: '4px',
                fontFamily: 'inherit',
                fontSize: '14px',
                resize: 'vertical'
              }}
            />
          </Box>
        </Box>
      )}
    </Box>
  );
};

interface PaymentStepProps {
  cart: Cart;
  loading: boolean;
}

const PaymentStep: React.FC<PaymentStepProps> = ({ cart, loading }) => {
  return (
    <Box>
      <Typography variant="h6" gutterBottom>
        Payment Information
      </Typography>
      
      <Box sx={{ display: 'flex', alignItems: 'center', mb: 3 }}>
        <PaymentIcon sx={{ mr: 2, color: 'primary.main' }} />
        <Typography variant="body1">
          Payment processing simulation
        </Typography>
      </Box>
      
      <Alert severity="info" sx={{ mb: 3 }}>
        This is a demo application. No actual payment will be processed.
      </Alert>
      
      <Box sx={{ bgcolor: 'background.default', p: 2, borderRadius: 1 }}>
        <Typography variant="subtitle2" gutterBottom>
          Order Total: ${cart.total.toFixed(2)}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Click "Place Order" to complete your purchase.
        </Typography>
      </Box>
      
      {loading && (
        <Box sx={{ display: 'flex', alignItems: 'center', mt: 2 }}>
          <CircularProgress size={20} sx={{ mr: 2 }} />
          <Typography variant="body2">
            Processing your order...
          </Typography>
        </Box>
      )}
    </Box>
  );
};

interface ConfirmationStepProps {
  orderId: string | null;
}

const ConfirmationStep: React.FC<ConfirmationStepProps> = ({ orderId }) => {
  return (
    <Box sx={{ textAlign: 'center' }}>
      <CheckCircleIcon sx={{ fontSize: 64, color: 'success.main', mb: 2 }} />
      
      <Typography variant="h5" gutterBottom>
        Order Placed Successfully!
      </Typography>
      
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Thank you for your order. We'll send you a confirmation email shortly.
      </Typography>
      
      {orderId && (
        <Box sx={{ bgcolor: 'success.light', p: 2, borderRadius: 1, mb: 3 }}>
          <Typography variant="subtitle2" color="success.dark">
            Order ID: {orderId}
          </Typography>
        </Box>
      )}
      
      <Typography variant="body2" color="text.secondary">
        You can track your order status in the Orders section.
      </Typography>
    </Box>
  );
};