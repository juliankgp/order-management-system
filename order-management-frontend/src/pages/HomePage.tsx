import React from 'react';
import { Container, Typography, Box } from '@mui/material';

const HomePage: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h3" component="h1" gutterBottom>
          Order Management System
        </Typography>
        <Typography variant="h6" color="text.secondary">
          Welcome to the Order Management System. Please login to continue.
        </Typography>
      </Box>
    </Container>
  );
};

export default HomePage;