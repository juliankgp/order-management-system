import React from 'react';
import { Container, Typography, Box } from '@mui/material';

const ProductsPage: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Products
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Product catalog will be implemented in Phase 3
        </Typography>
      </Box>
    </Container>
  );
};

export default ProductsPage;