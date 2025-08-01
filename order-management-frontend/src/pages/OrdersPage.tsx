import React from 'react';
import { Typography, Box } from '@mui/material';

const OrdersPage: React.FC = () => {
  return (
    <Box sx={{ 
      width: '100%', 
      py: 4, 
      px: { xs: 2, sm: 4, md: 6, lg: 8 } 
    }}>
      <Typography variant="h4" component="h1" gutterBottom>
        My Orders
      </Typography>
      <Typography variant="body1" color="text.secondary">
        Order management will be implemented in Phase 4
      </Typography>
    </Box>
  );
};

export default OrdersPage;