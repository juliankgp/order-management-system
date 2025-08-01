import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography,
  Box,
  Chip,
  TextField,
  IconButton,
  Divider,
  Alert,
} from '@mui/material';
import {
  Close as CloseIcon,
  Add as AddIcon,
  Remove as RemoveIcon,
  ShoppingCart as CartIcon,
} from '@mui/icons-material';
import type { ProductDto } from '../../types';

interface ProductDetailProps {
  product: ProductDto | null;
  open: boolean;
  onClose: () => void;
  onAddToCart?: (product: ProductDto, quantity: number) => void;
  cartQuantity?: number;
  loading?: boolean;
}

const ProductDetail: React.FC<ProductDetailProps> = ({
  product,
  open,
  onClose,
  onAddToCart,
  cartQuantity = 0,
  loading = false,
}) => {
  const [quantity, setQuantity] = useState(1);

  if (!product) return null;

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity >= 1 && newQuantity <= product.stockQuantity) {
      setQuantity(newQuantity);
    }
  };

  const handleAddToCart = () => {
    if (onAddToCart && quantity > 0) {
      onAddToCart(product, quantity);
      onClose();
    }
  };

  const isOutOfStock = product.stockQuantity === 0;
  const isLowStock = product.stockQuantity > 0 && product.stockQuantity <= 10;
  const maxQuantity = Math.min(product.stockQuantity, 99);

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="md"
      fullWidth
      PaperProps={{
        sx: {
          minHeight: 500,
        },
      }}
    >
      <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h5" component="div">
          Product Details
        </Typography>
        <IconButton onClick={onClose} edge="end">
          <CloseIcon />
        </IconButton>
      </DialogTitle>

      <DialogContent>
        <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, gap: 3 }}>
          <Box sx={{ flex: 1 }}>
            <Box
              sx={{
                width: '100%',
                height: 300,
                backgroundColor: '#f5f5f5',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                borderRadius: 1,
                position: 'relative',
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
                    borderRadius: 4,
                  }}
                  onError={(e) => {
                    (e.target as HTMLImageElement).style.display = 'none';
                  }}
                />
              ) : (
                <Typography variant="h6" color="text.secondary">
                  No Image Available
                </Typography>
              )}

              {!product.isActive && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 16,
                    left: 16,
                  }}
                >
                  <Chip label="Inactive" color="error" />
                </Box>
              )}

              {isOutOfStock && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 16,
                    right: 16,
                  }}
                >
                  <Chip label="Out of Stock" color="error" />
                </Box>
              )}

              {isLowStock && !isOutOfStock && (
                <Box
                  sx={{
                    position: 'absolute',
                    top: 16,
                    right: 16,
                  }}
                >
                  <Chip label="Low Stock" color="warning" />
                </Box>
              )}
            </Box>
          </Box>

          <Box sx={{ flex: 1 }}>
            <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
              <Typography variant="h4" component="h1" gutterBottom>
                {product.name}
              </Typography>

              {product.category && (
                <Box sx={{ mb: 2 }}>
                  <Chip
                    label={product.category}
                    color="primary"
                    variant="outlined"
                  />
                </Box>
              )}

              <Typography variant="h5" color="primary" fontWeight="bold" gutterBottom>
                ${product.price.toFixed(2)}
              </Typography>

              <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                <Typography variant="body1">
                  <strong>Stock:</strong> {product.stockQuantity} units
                </Typography>
                <Typography variant="body1">
                  <strong>SKU:</strong> {product.sku || 'N/A'}
                </Typography>
              </Box>

              {product.description && (
                <>
                  <Divider sx={{ my: 2 }} />
                  <Typography variant="h6" gutterBottom>
                    Description
                  </Typography>
                  <Typography variant="body1" paragraph>
                    {product.description}
                  </Typography>
                </>
              )}

              <Box sx={{ mt: 'auto' }}>
                {product.brand && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    <strong>Brand:</strong> {product.brand}
                  </Typography>
                )}

                {product.weight && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    <strong>Weight:</strong> {product.weight} kg
                  </Typography>
                )}

                {(product.length || product.width || product.height) && (
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    <strong>Dimensions:</strong> {product.length || 0} × {product.width || 0} × {product.height || 0} cm
                  </Typography>
                )}

                <Typography variant="body2" color="text.secondary" gutterBottom>
                  <strong>Created:</strong> {new Date(product.createdAt).toLocaleDateString()}
                </Typography>

                <Typography variant="body2" color="text.secondary">
                  <strong>Last Updated:</strong> {new Date(product.updatedAt).toLocaleDateString()}
                </Typography>
              </Box>
            </Box>
          </Box>
        </Box>

        {onAddToCart && product.isActive && (
          <>
            <Divider sx={{ my: 3 }} />
            
            {isOutOfStock ? (
              <Alert severity="error" sx={{ mb: 2 }}>
                This product is currently out of stock
              </Alert>
            ) : (
              <Box>
                <Typography variant="h6" gutterBottom>
                  Add to Cart
                </Typography>
                
                {cartQuantity > 0 && (
                  <Alert severity="info" sx={{ mb: 2 }}>
                    You already have {cartQuantity} of this item in your cart
                  </Alert>
                )}

                <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                  <Typography variant="body1">Quantity:</Typography>
                  
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <IconButton
                      size="small"
                      onClick={() => handleQuantityChange(quantity - 1)}
                      disabled={quantity <= 1}
                    >
                      <RemoveIcon />
                    </IconButton>
                    
                    <TextField
                      type="number"
                      value={quantity}
                      onChange={(e) => handleQuantityChange(parseInt(e.target.value) || 1)}
                      inputProps={{
                        min: 1,
                        max: maxQuantity,
                        style: { textAlign: 'center', width: '60px' },
                      }}
                      size="small"
                    />
                    
                    <IconButton
                      size="small"
                      onClick={() => handleQuantityChange(quantity + 1)}
                      disabled={quantity >= maxQuantity}
                    >
                      <AddIcon />
                    </IconButton>
                  </Box>

                  <Typography variant="body2" color="text.secondary">
                    (Max: {maxQuantity})
                  </Typography>
                </Box>

                <Typography variant="body1" sx={{ mb: 2 }}>
                  <strong>Total: ${(product.price * quantity).toFixed(2)}</strong>
                </Typography>
              </Box>
            )}
          </>
        )}
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 3 }}>
        <Button onClick={onClose} variant="outlined">
          Close
        </Button>
        
        {onAddToCart && product.isActive && !isOutOfStock && (
          <Button
            onClick={handleAddToCart}
            variant="contained"
            startIcon={<CartIcon />}
            disabled={loading || quantity < 1}
          >
            Add {quantity} to Cart
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
};

export default ProductDetail;