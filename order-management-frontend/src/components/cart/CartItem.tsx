import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  IconButton,
  TextField,
  Chip,
  Avatar
} from '@mui/material';
import {
  Add as AddIcon,
  Remove as RemoveIcon,
  Delete as DeleteIcon
} from '@mui/icons-material';
import type { CartItem as CartItemType } from '../../types/entities';
import { useCart } from '../../contexts/CartContext';

interface CartItemProps {
  item: CartItemType;
  compact?: boolean;
}

export const CartItem: React.FC<CartItemProps> = ({ item, compact = false }) => {
  const { updateQuantity, removeItem } = useCart();
  const { product, quantity, subtotal } = item;

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity < 1) {
      handleRemove();
    } else {
      updateQuantity(product.id, newQuantity);
    }
  };

  const handleRemove = () => {
    removeItem(product.id);
  };

  const isOutOfStock = product.stockQuantity === 0;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity < 10;
  const maxQuantity = Math.min(product.stockQuantity, 99);

  if (compact) {
    return (
      <Box sx={{ display: 'flex', alignItems: 'center', py: 1, borderBottom: '1px solid #eee' }}>
        <Avatar
          src={product.imageUrl || '/placeholder-product.png'}
          alt={product.name}
          sx={{ width: 40, height: 40, mr: 2 }}
        />
        
        <Box sx={{ flex: 1, minWidth: 0 }}>
          <Typography variant="body2" noWrap>
            {product.name}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            ${product.price.toFixed(2)} × {quantity}
          </Typography>
        </Box>
        
        <Typography variant="body2" fontWeight={600}>
          ${subtotal.toFixed(2)}
        </Typography>
      </Box>
    );
  }

  return (
    <Card sx={{ mb: 2 }}>
      <CardContent sx={{ p: 2, '&:last-child': { pb: 2 } }}>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Avatar
            src={product.imageUrl || '/placeholder-product.png'}
            alt={product.name}
            sx={{ width: 80, height: 80 }}
            variant="rounded"
          />
          
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
              <Typography variant="h6" component="h3" noWrap>
                {product.name}
              </Typography>
              <IconButton
                onClick={handleRemove}
                size="small"
                color="error"
                aria-label="Remove item"
              >
                <DeleteIcon />
              </IconButton>
            </Box>
            
            {product.description && (
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                {product.description}
              </Typography>
            )}
            
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Typography variant="body2" color="text.secondary">
                SKU: {product.sku}
              </Typography>
              
              {isOutOfStock && (
                <Chip
                  label="Out of Stock"
                  color="error"
                  size="small"
                />
              )}
              
              {isLowStock && !isOutOfStock && (
                <Chip
                  label={`Only ${product.stockQuantity} left`}
                  color="warning"
                  size="small"
                />
              )}
            </Box>
            
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <IconButton
                  onClick={() => handleQuantityChange(quantity - 1)}
                  disabled={quantity <= 1 || isOutOfStock}
                  size="small"
                  aria-label="Decrease quantity"
                >
                  <RemoveIcon />
                </IconButton>
                
                <TextField
                  type="number"
                  value={quantity}
                  onChange={(e) => {
                    const value = parseInt(e.target.value) || 1;
                    if (value >= 1 && value <= maxQuantity) {
                      handleQuantityChange(value);
                    }
                  }}
                  disabled={isOutOfStock}
                  size="small"
                  sx={{ width: 80 }}
                  inputProps={{
                    min: 1,
                    max: maxQuantity,
                    'aria-label': 'Quantity'
                  }}
                />
                
                <IconButton
                  onClick={() => handleQuantityChange(quantity + 1)}
                  disabled={quantity >= maxQuantity || isOutOfStock}
                  size="small"
                  aria-label="Increase quantity"
                >
                  <AddIcon />
                </IconButton>
              </Box>
              
              <Box sx={{ textAlign: 'right' }}>
                <Typography variant="body2" color="text.secondary">
                  ${product.price.toFixed(2)} each
                </Typography>
                <Typography variant="h6" color="primary" fontWeight={600}>
                  ${subtotal.toFixed(2)}
                </Typography>
              </Box>
            </Box>
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};