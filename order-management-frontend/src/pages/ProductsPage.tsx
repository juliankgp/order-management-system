import React from 'react';
import { Typography, Box } from '@mui/material';

const ProductsPage: React.FC = () => {
  return (
    <Box sx={{ 
      width: '100%', 
      py: 4, 
      px: { xs: 2, sm: 4, md: 6, lg: 8 } 
    }}>
      <Typography variant="h4" component="h1" gutterBottom>
        Products
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Product catalog will be implemented in Phase 3
      </Typography>
    </Box>
  );
};

export default ProductsPage;