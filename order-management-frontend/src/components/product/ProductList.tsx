import React from 'react';
import {
  Box,
  Typography,
  Pagination,
  Alert,
  Skeleton,
} from '@mui/material';
import ProductCard from './ProductCard';
import type { ProductDto, PagedResponse } from '../../types/entities';

interface ProductListProps {
  products: PagedResponse<ProductDto> | null;
  loading?: boolean;
  error?: string | null;
  onPageChange?: (page: number) => void;
  onViewDetails?: (product: ProductDto) => void;
  showQuantityControls?: boolean;
  emptyMessage?: string;
  skeletonCount?: number;
}

const ProductListSkeleton: React.FC<{ count: number }> = ({ count }) => (
  <Box sx={{ 
    display: 'grid', 
    gridTemplateColumns: { 
      xs: 'repeat(1, 1fr)', 
      sm: 'repeat(2, 1fr)', 
      md: 'repeat(3, 1fr)', 
      lg: 'repeat(4, 1fr)' 
    },
    gap: 3 
  }}>
    {Array.from({ length: count }).map((_, index) => (
      <Box key={index}>
        <Skeleton variant="rectangular" height={200} sx={{ mb: 1 }} />
        <Skeleton variant="text" height={32} sx={{ mb: 1 }} />
        <Skeleton variant="text" height={20} width="60%" sx={{ mb: 1 }} />
        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
          <Skeleton variant="text" height={24} width="40%" />
          <Skeleton variant="text" height={20} width="30%" />
        </Box>
        <Skeleton variant="rectangular" height={36} />
      </Box>
    ))}
  </Box>
);

const ProductList: React.FC<ProductListProps> = ({
  products,
  loading = false,
  error = null,
  onPageChange,
  onViewDetails,
  showQuantityControls = true,
  emptyMessage = "No products found",
  skeletonCount = 8,
}) => {
  const handlePageChange = (_: React.ChangeEvent<unknown>, page: number) => {
    if (onPageChange) {
      onPageChange(page);
    }
  };

  if (loading) {
    return <ProductListSkeleton count={skeletonCount} />;
  }

  if (error) {
    return (
      <Alert severity="error" sx={{ mb: 3 }}>
        {error}
      </Alert>
    );
  }

  if (!products || !products.items || products.items.length === 0) {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          minHeight: 300,
          textAlign: 'center',
        }}
      >
        <Typography variant="h6" color="text.secondary" gutterBottom>
          {emptyMessage}
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Try adjusting your search or filter criteria
        </Typography>
      </Box>
    );
  }

  const totalPages = Math.ceil(products.totalCount / products.pageSize);

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="body2" color="text.secondary">
          Showing {products.items.length} of {products.totalCount} products
          {products.currentPage > 1 && ` (Page ${products.currentPage} of ${totalPages})`}
        </Typography>
      </Box>

      <Box sx={{ 
        display: 'grid', 
        gridTemplateColumns: { 
          xs: 'repeat(1, 1fr)', 
          sm: 'repeat(2, 1fr)', 
          md: 'repeat(3, 1fr)', 
          lg: 'repeat(4, 1fr)' 
        },
        gap: 3 
      }}>
        {products.items.map((product: ProductDto) => (
          <ProductCard
            key={product.id}
            product={product}
            onViewDetails={onViewDetails}
            showQuantityControls={showQuantityControls}
          />
        ))}
      </Box>

      {totalPages > 1 && (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            mt: 4,
            mb: 2,
          }}
        >
          <Pagination
            count={totalPages}
            page={products.currentPage}
            onChange={handlePageChange}
            color="primary"
            size="large"
            showFirstButton
            showLastButton
          />
        </Box>
      )}
    </Box>
  );
};

export default ProductList;