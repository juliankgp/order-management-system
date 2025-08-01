import React, { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import {
  Box,
  TextField,
  Button,
  Typography,
  Alert,
  CircularProgress,
  InputAdornment,
  IconButton,
  Link,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Email as EmailIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { loginValidationSchema } from '../../utils/validations';
import { type LoginFormData } from '../../types';

interface LocationState {
  from?: {
    pathname: string;
  };
}

const LoginForm: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isLoading, error, clearError } = useAuth();
  
  const [showPassword, setShowPassword] = useState(false);
  const [loginError, setLoginError] = useState<string | null>(null);

  // Get the route the user came from
  const from = (location.state as LocationState)?.from?.pathname || '/products';

  // Configure React Hook Form
  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
  } = useForm<LoginFormData>({
    resolver: yupResolver(loginValidationSchema),
    defaultValues: {
      email: '',
      password: '',
    },
    mode: 'onChange',
  });

  // Handle form submission
  const onSubmit = async (data: LoginFormData) => {
    try {
      // Clear previous errors
      setLoginError(null);
      clearError();

      // Attempt login
      await login(data);

      // If we reach here, login was successful
      console.log('✅ Login successful, redirecting to:', from);
      navigate(from, { replace: true });

    } catch (error) {
      console.error('❌ Login error:', error);
      
      const errorMessage = error instanceof Error 
        ? error.message 
        : 'Unknown error during login';
      
      setLoginError(errorMessage);
    }
  };

  // Handle password visibility toggle
  const handleTogglePasswordVisibility = () => {
    setShowPassword(prev => !prev);
  };

  // Clear errors when user starts typing
  const handleClearErrors = () => {
    if (loginError) setLoginError(null);
    if (error) clearError();
  };

  // Navigate to register
  const handleGoToRegister = () => {
    navigate('/register');
  };

  const currentError = loginError || error;

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(onSubmit)}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 3,
        width: '100%',
        maxWidth: '100%',
      }}
    >
      {/* Title */}
      <Typography variant="h4" component="h1" textAlign="center" gutterBottom>
        Sign In
      </Typography>

      <Typography variant="body2" color="text.secondary" textAlign="center" sx={{ mb: 2 }}>
        Enter your credentials to access your account
      </Typography>

      {/* Global Error */}
      {currentError && (
        <Alert 
          severity="error" 
          onClose={handleClearErrors}
          sx={{ mb: 2 }}
        >
          {currentError}
        </Alert>
      )}

      {/* Email Field */}
      <Controller
        name="email"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Email"
            type="email"
            fullWidth
            error={!!errors.email}
            helperText={errors.email?.message}
            disabled={isLoading || isSubmitting}
            onChange={(e) => {
              field.onChange(e);
              handleClearErrors();
            }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <EmailIcon color="action" />
                </InputAdornment>
              ),
            }}
            placeholder="example@email.com"
          />
        )}
      />

      {/* Password Field */}
      <Controller
        name="password"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Password"
            type={showPassword ? 'text' : 'password'}
            fullWidth
            error={!!errors.password}
            helperText={errors.password?.message}
            disabled={isLoading || isSubmitting}
            onChange={(e) => {
              field.onChange(e);
              handleClearErrors();
            }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <LockIcon color="action" />
                </InputAdornment>
              ),
              endAdornment: (
                <InputAdornment position="end">
                  <IconButton
                    onClick={handleTogglePasswordVisibility}
                    edge="end"
                    disabled={isLoading || isSubmitting}
                  >
                    {showPassword ? <VisibilityOff /> : <Visibility />}
                  </IconButton>
                </InputAdornment>
              ),
            }}
            placeholder="Your password"
          />
        )}
      />

      {/* Submit Button */}
      <Button
        type="submit"
        variant="contained"
        size="large"
        fullWidth
        disabled={isLoading || isSubmitting}
        sx={{
          py: 1.5,
          position: 'relative',
          mt: 2,
        }}
      >
        {(isLoading || isSubmitting) ? (
          <>
            <CircularProgress
              size={20}
              sx={{
                position: 'absolute',
                left: '50%',
                top: '50%',
                marginLeft: '-10px',
                marginTop: '-10px',
              }}
            />
            Signing in...
          </>
        ) : (
          'Sign In'
        )}
      </Button>

      {/* Clear Button */}
      <Button
        variant="outlined"
        onClick={() => {
          reset();
          handleClearErrors();
        }}
        disabled={isLoading || isSubmitting}
        sx={{ py: 1.5 }}
      >
        Clear Form
      </Button>

      {/* Link to Register */}
      <Box textAlign="center" mt={2}>
        <Typography variant="body2" color="text.secondary">
          Don't have an account?{' '}
          <Link
            component="button"
            type="button"
            onClick={handleGoToRegister}
            disabled={isLoading || isSubmitting}
            sx={{
              textDecoration: 'none',
              fontWeight: 'medium',
              '&:hover': {
                textDecoration: 'underline',
              },
            }}
          >
            Sign up here
          </Link>
        </Typography>
      </Box>

      {/* Development Information */}
      {import.meta.env.DEV && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <Typography variant="caption">
            <strong>Development Mode:</strong><br />
            You can use any valid email and password that meets requirements to test login.
          </Typography>
        </Alert>
      )}
    </Box>
  );
};

export default LoginForm;