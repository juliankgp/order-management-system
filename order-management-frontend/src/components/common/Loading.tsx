import React from 'react';
import { 
  Box, 
  CircularProgress, 
  Typography, 
  Backdrop,
  Skeleton,
  Card,
  CardContent,
  Grid
} from '@mui/material';

interface LoadingSpinnerProps {
  size?: number;
  message?: string;
  fullScreen?: boolean;
  color?: 'primary' | 'secondary' | 'inherit';
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({ 
  size = 40, 
  message, 
  fullScreen = false,
  color = 'primary'
}) => {
  const content = (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      gap={2}
      py={fullScreen ? 0 : 3}
    >
      <CircularProgress size={size} color={color} />
      {message && (
        <Typography variant="body2" color="text.secondary">
          {message}
        </Typography>
      )}
    </Box>
  );

  if (fullScreen) {
    return (
      <Backdrop
        sx={{ color: '#fff', zIndex: (theme) => theme.zIndex.drawer + 1 }}
        open={true}
      >
        {content}
      </Backdrop>
    );
  }

  return content;
};

interface LoadingSkeletonProps {
  variant?: 'card' | 'list' | 'table' | 'text';
  rows?: number;
  height?: number | string;
  width?: number | string;
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({ 
  variant = 'card', 
  rows = 3,
  height = 200,
  width = '100%'
}) => {
  if (variant === 'card') {
    return (
      <Grid container spacing={2}>
        {Array.from({ length: rows }).map((_, index) => (
          <Grid size={{ xs: 12, sm: 6, md: 4 }} key={index}>
            <Card>
              <Skeleton variant="rectangular" height={height} />
              <CardContent>
                <Skeleton variant="text" height={24} width="80%" />
                <Skeleton variant="text" height={20} width="60%" />
                <Box mt={1}>
                  <Skeleton variant="text" height={16} width="40%" />
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    );
  }

  if (variant === 'list') {
    return (
      <Box>
        {Array.from({ length: rows }).map((_, index) => (
          <Box key={index} display="flex" alignItems="center" gap={2} py={2}>
            <Skeleton variant="circular" width={40} height={40} />
            <Box flex={1}>
              <Skeleton variant="text" height={20} width="70%" />
              <Skeleton variant="text" height={16} width="50%" />
            </Box>
          </Box>
        ))}
      </Box>
    );
  }

  if (variant === 'table') {
    return (
      <Box>
        {Array.from({ length: rows }).map((_, index) => (
          <Box key={index} display="flex" gap={2} py={1}>
            <Skeleton variant="text" height={20} width="20%" />
            <Skeleton variant="text" height={20} width="30%" />
            <Skeleton variant="text" height={20} width="25%" />
            <Skeleton variant="text" height={20} width="25%" />
          </Box>
        ))}
      </Box>
    );
  }

  // variant === 'text'
  return (
    <Box>
      {Array.from({ length: rows }).map((_, index) => (
        <Skeleton key={index} variant="text" height={height} width={width} />
      ))}
    </Box>
  );
};

interface PageLoadingProps {
  message?: string;
}

export const PageLoading: React.FC<PageLoadingProps> = ({ 
  message = 'Loading...' 
}) => {
  return (
    <Box
      display="flex"
      flexDirection="column"
      alignItems="center"
      justifyContent="center"
      minHeight="50vh"
      gap={2}
    >
      <CircularProgress size={50} />
      <Typography variant="h6" color="text.secondary">
        {message}
      </Typography>
    </Box>
  );
};

interface ButtonLoadingProps {
  loading: boolean;
  children: React.ReactNode;
  size?: 'small' | 'medium' | 'large';
}

export const ButtonLoading: React.FC<ButtonLoadingProps> = ({ 
  loading, 
  children, 
  size = 'small' 
}) => {
  const spinnerSize = {
    small: 16,
    medium: 20,
    large: 24
  }[size];

  if (loading) {
    return (
      <Box display="flex" alignItems="center" gap={1}>
        <CircularProgress size={spinnerSize} color="inherit" />
        {children}
      </Box>
    );
  }

  return <>{children}</>;
};

export default LoadingSpinner;