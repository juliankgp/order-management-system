import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  IconButton,
  TextField,
  Button,
  Chip,
} from '@mui/material';
import {
  Add as AddIcon,
  Remove as RemoveIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material';
import type { CartItem as CartItemType } from '../../types';

export interface CartItemProps {
  item: CartItemType;
  onUpdateQuantity: (productId: string, quantity: number) => void;
  onRemove: (productId: string) => void;
  loading?: boolean;
}

const CartItem: React.FC<CartItemProps> = ({
  item,
  onUpdateQuantity,
  onRemove,
  loading = false
}) => {
  const { product, quantity, subtotal } = item;
  const isOutOfStock = product.stockQuantity === 0;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity < product.minimumStock;
  const maxQuantity = Math.min(product.stockQuantity, 99);

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity < 1) {
      onRemove(product.id);
    } else if (newQuantity <= maxQuantity) {
      onUpdateQuantity(product.id, newQuantity);
    }
  };

  const handleInputChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const value = parseInt(event.target.value, 10);
    if (!isNaN(value)) {
      handleQuantityChange(value);
    }
  };

  return (
    <Card sx={{ mb: 2, opacity: loading ? 0.7 : 1 }}>
      <CardContent>
        <Box sx={{ display: 'flex', gap: 2 }}>
          {/* Product Image */}
          <Box
            sx={{
              width: 80,
              height: 80,
              backgroundColor: 'grey.200',
              borderRadius: 1,
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              flexShrink: 0,
              overflow: 'hidden',
            }}
          >
            {product.imageUrl ? (
              <img
                src={product.imageUrl}
                alt={product.name}
                style={{
                  width: '100%',
                  height: '100%',
                  objectFit: 'cover',
                }}
                onError={(e) => {
                  e.currentTarget.style.display = 'none';
                }}
              />
            ) : (
              <Typography variant="caption" color="text.secondary">
                No Image
              </Typography>
            )}
          </Box>

          {/* Product Details */}
          <Box sx={{ flex: 1 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
              <Typography variant="h6" component="h3" sx={{ fontWeight: 600 }}>
                {product.name}
              </Typography>
              <IconButton
                onClick={() => onRemove(product.id)}
                disabled={loading}
                color="error"
                size="small"
                sx={{ ml: 1 }}
              >
                <DeleteIcon />
              </IconButton>
            </Box>

            {product.description && (
              <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                {product.description.length > 100 
                  ? `${product.description.substring(0, 100)}...` 
                  : product.description
                }
              </Typography>
            )}

            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
              <Typography variant="body2" color="text.secondary">
                SKU: {product.sku}
              </Typography>
              
              {isOutOfStock && (
                <Chip label="Out of Stock" color="error" size="small" />
              )}
              
              {isLowStock && !isOutOfStock && (
                <Chip label="Low Stock" color="warning" size="small" />
              )}
            </Box>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              {/* Quantity Controls */}
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <IconButton
                  onClick={() => handleQuantityChange(quantity - 1)}
                  disabled={loading || quantity <= 1}
                  size="small"
                >
                  <RemoveIcon />
                </IconButton>
                
                <TextField
                  value={quantity}
                  onChange={handleInputChange}
                  disabled={loading}
                  size="small"
                  sx={{ width: 60 }}
                  inputProps={{
                    min: 1,
                    max: maxQuantity,
                    type: 'number',
                    style: { textAlign: 'center' }
                  }}
                />
                
                <IconButton
                  onClick={() => handleQuantityChange(quantity + 1)}
                  disabled={loading || quantity >= maxQuantity}
                  size="small"
                >
                  <AddIcon />
                </IconButton>
                
                <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
                  Max: {maxQuantity}
                </Typography>
              </Box>

              {/* Price Information */}
              <Box sx={{ textAlign: 'right' }}>
                <Typography variant="body2" color="text.secondary">
                  ${product.price.toFixed(2)} each
                </Typography>
                <Typography variant="h6" component="span" sx={{ fontWeight: 600 }}>
                  ${subtotal.toFixed(2)}
                </Typography>
              </Box>
            </Box>

            {/* Stock Warning */}
            {quantity > product.stockQuantity && (
              <Typography variant="caption" color="error" sx={{ mt: 1, display: 'block' }}>
                Warning: Only {product.stockQuantity} items available in stock
              </Typography>
            )}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};

export default CartItem;