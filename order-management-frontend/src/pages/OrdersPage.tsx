import React from 'react';
import { Container, Typography, Box } from '@mui/material';

const OrdersPage: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          My Orders
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Order management will be implemented in Phase 4
        </Typography>
      </Box>
    </Container>
  );
};

export default OrdersPage;