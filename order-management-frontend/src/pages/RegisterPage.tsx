import React from 'react';
import { Paper, Box } from '@mui/material';
import RegisterForm from '../components/auth/RegisterForm';

const RegisterPage: React.FC = () => {
  return (
    <Box 
      sx={{ 
        width: '100%',
        minHeight: '80vh',
        display: 'flex', 
        justifyContent: 'center',
        alignItems: 'center',
        px: { xs: 2, sm: 4, md: 6 },
        py: 4,
      }}
    >
      <Paper 
        elevation={3} 
        sx={{ 
          p: { xs: 3, sm: 4, md: 6 }, 
          width: '100%',
          maxWidth: { xs: 500, sm: 600, md: 700, lg: 800 },
          borderRadius: 2,
        }}
      >
        <RegisterForm />
      </Paper>
    </Box>
  );
};

export default RegisterPage;