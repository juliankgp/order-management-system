import React from 'react';
import { 
  Container, 
  Typography, 
  Box, 
  Button, 
  Paper,
  Grid,
  useTheme,
  useMediaQuery
} from '@mui/material';
import { 
  Home as HomeIcon, 
  ArrowBack as BackIcon,
  Search as SearchIcon 
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';

const NotFoundPage: React.FC = () => {
  const navigate = useNavigate();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const handleGoHome = () => {
    navigate('/');
  };

  const handleGoBack = () => {
    navigate(-1);
  };

  const handleGoToProducts = () => {
    navigate('/products');
  };

  return (
    <Container maxWidth="md" sx={{ py: 4 }}>
      <Paper 
        elevation={2} 
        sx={{ 
          p: { xs: 3, md: 6 }, 
          textAlign: 'center',
          borderRadius: 3
        }}
      >
        <Box mb={4}>
          {/* Large 404 Number */}
          <Typography
            variant="h1"
            component="div"
            sx={{
              fontSize: { xs: '6rem', md: '8rem' },
              fontWeight: 'bold',
              color: 'primary.main',
              textShadow: '2px 2px 4px rgba(0,0,0,0.1)',
              mb: 2
            }}
          >
            404
          </Typography>

          {/* Error Message */}
          <Typography variant="h4" component="h1" gutterBottom color="text.primary">
            Page Not Found
          </Typography>
          
          <Typography variant="h6" color="text.secondary" sx={{ mb: 4, maxWidth: 500, mx: 'auto' }}>
            Oops! The page you're looking for doesn't exist. It might have been moved, deleted, or you entered the wrong URL.
          </Typography>
        </Box>

        {/* Illustration/Icon */}
        <Box sx={{ mb: 4 }}>
          <SearchIcon 
            sx={{ 
              fontSize: { xs: 60, md: 80 }, 
              color: 'text.disabled',
              opacity: 0.5 
            }} 
          />
        </Box>

        {/* Action Buttons */}
        <Grid container spacing={2} justifyContent="center">
          <Grid size={{ xs: 12, sm: "auto" }}>
            <Button
              variant="contained"
              size="large"
              startIcon={<HomeIcon />}
              onClick={handleGoHome}
              fullWidth={isMobile}
              sx={{ minWidth: { sm: 150 } }}
            >
              Go Home
            </Button>
          </Grid>
          
          <Grid size={{ xs: 12, sm: "auto" }}>
            <Button
              variant="outlined"
              size="large"
              startIcon={<BackIcon />}
              onClick={handleGoBack}
              fullWidth={isMobile}
              sx={{ minWidth: { sm: 150 } }}
            >
              Go Back
            </Button>
          </Grid>
          
          <Grid size={{ xs: 12, sm: "auto" }}>
            <Button
              variant="text"
              size="large"
              onClick={handleGoToProducts}
              fullWidth={isMobile}
              sx={{ minWidth: { sm: 150 } }}
            >
              Browse Products
            </Button>
          </Grid>
        </Grid>

        {/* Help Text */}
        <Box sx={{ mt: 4, pt: 3, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="body2" color="text.secondary">
            If you believe this is an error, please contact our support team or try searching for what you need.
          </Typography>
        </Box>
      </Paper>
    </Container>
  );
};

export default NotFoundPage;