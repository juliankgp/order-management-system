import React from 'react';
import {
  Card,
  CardContent,
  CardMedia,
  Typography,
  Button,
  Box,
  Chip,
  IconButton,
} from '@mui/material';
import {
  Add as AddIcon,
  Remove as RemoveIcon,
  Visibility as ViewIcon,
  ShoppingCart as CartIcon,
} from '@mui/icons-material';
import type { ProductDto } from '../../types/entities';
import { useCart } from '../../contexts/CartContext';

interface ProductCardProps {
  product: ProductDto;
  onViewDetails?: (product: ProductDto) => void;
  showQuantityControls?: boolean;
}

const ProductCard: React.FC<ProductCardProps> = ({
  product,
  onViewDetails,
  showQuantityControls = true,
}) => {
  const { addItem, updateQuantity, getItemQuantity, isInCart } = useCart();
  const cartQuantity = getItemQuantity(product.id);
  const inCart = isInCart(product.id);

  const handleAddToCart = () => {
    addItem(product, 1);
  };

  const handleIncreaseQuantity = () => {
    if (cartQuantity < product.stock) {
      updateQuantity(product.id, cartQuantity + 1);
    }
  };

  const handleDecreaseQuantity = () => {
    if (cartQuantity > 0) {
      updateQuantity(product.id, cartQuantity - 1);
    }
  };

  const handleViewDetails = () => {
    if (onViewDetails) {
      onViewDetails(product);
    }
  };

  const isOutOfStock = product.stock === 0;
  const isLowStock = product.stock > 0 && product.stock <= 10;

  return (
    <Card
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        position: 'relative',
        transition: 'all 0.3s ease-in-out',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 6,
        },
        opacity: isOutOfStock ? 0.7 : 1,
      }}
    >
      {!product.isActive && (
        <Box
          sx={{
            position: 'absolute',
            top: 8,
            left: 8,
            zIndex: 1,
          }}
        >
          <Chip
            label="Inactive"
            color="error"
            size="small"
            variant="filled"
          />
        </Box>
      )}

      {isOutOfStock && (
        <Box
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
            zIndex: 1,
          }}
        >
          <Chip
            label="Out of Stock"
            color="error"
            size="small"
            variant="filled"
          />
        </Box>
      )}

      {isLowStock && !isOutOfStock && (
        <Box
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
            zIndex: 1,
          }}
        >
          <Chip
            label="Low Stock"
            color="warning"
            size="small"
            variant="filled"
          />
        </Box>
      )}

      <CardMedia
        component="div"
        sx={{
          height: 200,
          backgroundColor: '#f5f5f5',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          position: 'relative',
          cursor: onViewDetails ? 'pointer' : 'default',
        }}
        onClick={handleViewDetails}
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
              (e.target as HTMLImageElement).style.display = 'none';
            }}
          />
        ) : (
          <Typography variant="h6" color="text.secondary">
            No Image
          </Typography>
        )}
        
        {onViewDetails && (
          <Box
            sx={{
              position: 'absolute',
              top: 8,
              left: 8,
              opacity: 0,
              transition: 'opacity 0.3s ease-in-out',
              '.MuiCard-root:hover &': {
                opacity: 1,
              },
            }}
          >
            <IconButton
              size="small"
              sx={{
                backgroundColor: 'rgba(255, 255, 255, 0.9)',
                '&:hover': {
                  backgroundColor: 'rgba(255, 255, 255, 1)',
                },
              }}
              onClick={handleViewDetails}
            >
              <ViewIcon fontSize="small" />
            </IconButton>
          </Box>
        )}
      </CardMedia>

      <CardContent sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        <Typography
          variant="h6"
          component="h3"
          gutterBottom
          sx={{
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            display: '-webkit-box',
            WebkitLineClamp: 2,
            WebkitBoxOrient: 'vertical',
            minHeight: '3.5em',
            cursor: onViewDetails ? 'pointer' : 'default',
          }}
          onClick={handleViewDetails}
        >
          {product.name}
        </Typography>

        {product.description && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              mb: 1,
              flexGrow: 1,
            }}
          >
            {product.description}
          </Typography>
        )}

        <Box sx={{ mt: 'auto' }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
            <Typography variant="h6" color="primary" fontWeight="bold">
              ${product.price.toFixed(2)}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Stock: {product.stock}
            </Typography>
          </Box>

          {product.category && (
            <Box sx={{ mb: 1 }}>
              <Chip
                label={product.category}
                size="small"
                variant="outlined"
                color="primary"
              />
            </Box>
          )}

          {showQuantityControls && inCart && cartQuantity > 0 && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
              <IconButton
                size="small"
                onClick={handleDecreaseQuantity}
                disabled={cartQuantity <= 0}
              >
                <RemoveIcon />
              </IconButton>
              <Typography variant="body2" sx={{ minWidth: '2ch', textAlign: 'center' }}>
                {cartQuantity}
              </Typography>
              <IconButton
                size="small"
                onClick={handleIncreaseQuantity}
                disabled={cartQuantity >= product.stock}
              >
                <AddIcon />
              </IconButton>
            </Box>
          )}

          <Button
            variant={inCart ? "outlined" : "contained"}
            fullWidth
            startIcon={<CartIcon />}
            onClick={handleAddToCart}
            disabled={isOutOfStock || !product.isActive}
            sx={{ mt: 1 }}
          >
            {isOutOfStock 
              ? 'Out of Stock' 
              : inCart 
                ? `Add More (${cartQuantity} in cart)` 
                : 'Add to Cart'
            }
          </Button>
        </Box>
      </CardContent>
    </Card>
  );
};

export default ProductCard;