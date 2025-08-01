import React from 'react';
import { Container, Paper, Box, Typography } from '@mui/material';

const LoginPage: React.FC = () => {
  return (
    <Container maxWidth="sm">
      <Box sx={{ mt: 8 }}>
        <Paper elevation={3} sx={{ p: 4 }}>
          <Typography variant="h4" component="h1" gutterBottom align="center">
            Login
          </Typography>
          <Typography variant="body1" color="text.secondary" align="center">
            Login form will be implemented in Phase 2
          </Typography>
        </Paper>
      </Box>
    </Container>
  );
};

export default LoginPage;