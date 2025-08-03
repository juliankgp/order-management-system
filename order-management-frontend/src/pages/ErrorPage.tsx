import React from 'react';
import { 
  Container, 
  Typography, 
  Box, 
  Button, 
  Paper,
  Alert,
  AlertTitle,
  Divider,
  Grid,
  useTheme,
  useMediaQuery
} from '@mui/material';
import { 
  Home as HomeIcon, 
  Refresh as RefreshIcon,
  ArrowBack as BackIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Info as InfoIcon
} from '@mui/icons-material';
import { useNavigate, useRouteError } from 'react-router-dom';

export type ErrorType = 'network' | 'server' | 'auth' | 'forbidden' | 'generic';

interface ErrorPageProps {
  error?: {
    type?: ErrorType;
    title?: string;
    message?: string;
    statusCode?: number;
    details?: string;
  };
}

const ErrorPage: React.FC<ErrorPageProps> = ({ error: propError }) => {
  const navigate = useNavigate();
  const routeError = useRouteError() as { message?: string; status?: number; statusText?: string };
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  // Use prop error or route error
  const error = propError || {
    type: 'generic' as ErrorType,
    title: 'An Error Occurred',
    message: routeError?.message || 'Something went wrong. Please try again.',
    statusCode: routeError?.status || 500,
    details: routeError?.statusText
  };

  const getErrorConfig = (type: ErrorType, statusCode?: number) => {
    switch (type) {
      case 'network':
        return {
          icon: <ErrorIcon sx={{ fontSize: 60, color: 'error.main' }} />,
          severity: 'error' as const,
          title: 'Network Error',
          message: 'Unable to connect to the server. Please check your internet connection and try again.',
          actions: [
            { label: 'Retry', action: () => window.location.reload(), icon: <RefreshIcon /> },
            { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
          ]
        };

      case 'server':
        return {
          icon: <ErrorIcon sx={{ fontSize: 60, color: 'error.main' }} />,
          severity: 'error' as const,
          title: 'Server Error',
          message: 'The server encountered an error. Our team has been notified and is working on a fix.',
          actions: [
            { label: 'Retry', action: () => window.location.reload(), icon: <RefreshIcon /> },
            { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
          ]
        };

      case 'auth':
        return {
          icon: <WarningIcon sx={{ fontSize: 60, color: 'warning.main' }} />,
          severity: 'warning' as const,
          title: 'Authentication Required',
          message: 'You need to log in to access this page.',
          actions: [
            { label: 'Log In', action: () => navigate('/login'), icon: <HomeIcon /> },
            { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
          ]
        };

      case 'forbidden':
        return {
          icon: <WarningIcon sx={{ fontSize: 60, color: 'warning.main' }} />,
          severity: 'warning' as const,
          title: 'Access Forbidden',
          message: 'You don\'t have permission to access this resource.',
          actions: [
            { label: 'Go Back', action: () => navigate(-1), icon: <BackIcon /> },
            { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
          ]
        };

      default:
        // Handle specific HTTP status codes
        if (statusCode === 400) {
          return {
            icon: <InfoIcon sx={{ fontSize: 60, color: 'info.main' }} />,
            severity: 'info' as const,
            title: 'Bad Request',
            message: 'The request was invalid. Please check your input and try again.',
            actions: [
              { label: 'Go Back', action: () => navigate(-1), icon: <BackIcon /> },
              { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
            ]
          };
        }
        
        if (statusCode === 500) {
          return {
            icon: <ErrorIcon sx={{ fontSize: 60, color: 'error.main' }} />,
            severity: 'error' as const,
            title: 'Internal Server Error',
            message: 'The server encountered an internal error. Please try again later.',
            actions: [
              { label: 'Retry', action: () => window.location.reload(), icon: <RefreshIcon /> },
              { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
            ]
          };
        }

        return {
          icon: <ErrorIcon sx={{ fontSize: 60, color: 'error.main' }} />,
          severity: 'error' as const,
          title: error.title || 'An Error Occurred',
          message: error.message || 'Something went wrong. Please try again.',
          actions: [
            { label: 'Retry', action: () => window.location.reload(), icon: <RefreshIcon /> },
            { label: 'Go Back', action: () => navigate(-1), icon: <BackIcon /> },
            { label: 'Go Home', action: () => navigate('/'), icon: <HomeIcon /> }
          ]
        };
    }
  };

  const config = getErrorConfig(error.type!, error.statusCode);

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
        {/* Error Icon */}
        <Box mb={3}>
          {config.icon}
        </Box>

        {/* Status Code (if available) */}
        {error.statusCode && (
          <Typography
            variant="h2"
            component="div"
            sx={{
              fontSize: { xs: '3rem', md: '4rem' },
              fontWeight: 'bold',
              color: 'text.secondary',
              mb: 2
            }}
          >
            {error.statusCode}
          </Typography>
        )}

        {/* Error Alert */}
        <Alert 
          severity={config.severity} 
          sx={{ 
            mb: 4, 
            textAlign: 'left',
            '& .MuiAlert-message': {
              width: '100%'
            }
          }}
        >
          <AlertTitle sx={{ fontSize: '1.25rem', fontWeight: 'bold' }}>
            {config.title}
          </AlertTitle>
          {config.message}
        </Alert>

        {/* Error Details (Development Mode) */}
        {process.env.NODE_ENV === 'development' && error.details && (
          <>
            <Divider sx={{ my: 3 }} />
            <Box
              sx={{
                p: 2,
                backgroundColor: 'grey.100',
                borderRadius: 1,
                textAlign: 'left',
                fontFamily: 'monospace',
                fontSize: '0.875rem',
                mb: 3
              }}
            >
              <Typography variant="subtitle2" gutterBottom fontWeight="bold">
                Error Details (Development Mode):
              </Typography>
              <Typography variant="body2" component="pre" sx={{ whiteSpace: 'pre-wrap' }}>
                {error.details}
              </Typography>
            </Box>
          </>
        )}

        {/* Action Buttons */}
        <Grid container spacing={2} justifyContent="center">
          {config.actions.map((action, index) => (
            <Grid size={{ xs: 12, sm: "auto" }} key={index}>
              <Button
                variant={index === 0 ? 'contained' : 'outlined'}
                size="large"
                startIcon={action.icon}
                onClick={action.action}
                fullWidth={isMobile}
                sx={{ minWidth: { sm: 120 } }}
              >
                {action.label}
              </Button>
            </Grid>
          ))}
        </Grid>

        {/* Help Text */}
        <Box sx={{ mt: 4, pt: 3, borderTop: 1, borderColor: 'divider' }}>
          <Typography variant="body2" color="text.secondary">
            If this problem persists, please contact our support team for assistance.
          </Typography>
        </Box>
      </Paper>
    </Container>
  );
};

// Specific error page components for common use cases
export const NetworkErrorPage: React.FC = () => (
  <ErrorPage error={{ type: 'network' }} />
);

export const ServerErrorPage: React.FC = () => (
  <ErrorPage error={{ type: 'server' }} />
);

export const AuthErrorPage: React.FC = () => (
  <ErrorPage error={{ type: 'auth' }} />
);

export const ForbiddenErrorPage: React.FC = () => (
  <ErrorPage error={{ type: 'forbidden' }} />
);

export default ErrorPage;