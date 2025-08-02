import React from 'react';
import { Typography, Box } from '@mui/material';

const HomePage: React.FC = () => {
  return (
    <Box sx={{ 
      width: '100%', 
      py: 4, 
      px: { xs: 2, sm: 4, md: 6, lg: 8 },
      textAlign: 'center' 
    }}>
      <Typography variant="h3" component="h1" gutterBottom>
        Order Management System
      </Typography>
      <Typography variant="h6" color="text.secondary" sx={{ maxWidth: 800, mx: 'auto' }}>
        Welcome to the Order Management System. Please login to continue.
      </Typography>
    </Box>
  );
};

export default HomePage;