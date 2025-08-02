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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
} from '@mui/material';
import {
  Visibility,
  VisibilityOff,
  Email as EmailIcon,
  Lock as LockIcon,
  Person as PersonIcon,
  Phone as PhoneIcon,
  Cake as CakeIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../hooks/useAuth';
import { registerValidationSchema, genderOptions } from '../../utils/validations';
import { type RegisterFormData, Gender } from '../../types';

const RegisterForm: React.FC = () => {
  const navigate = useNavigate();
  const { register, isLoading, error, clearError } = useAuth();
  
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [registerError, setRegisterError] = useState<string | null>(null);

  // Configurar React Hook Form
  const {
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    watch,
  } = useForm({
    resolver: yupResolver(registerValidationSchema),
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      firstName: '',
      lastName: '',
      phoneNumber: '',
      dateOfBirth: undefined,
      gender: undefined,
    },
    mode: 'onChange' as const,
  });

  // Observar cambios en contraseña para validación en tiempo real
  const watchPassword = watch('password', '');

  // Manejar el envío del formulario
  const onSubmit = async (data: RegisterFormData) => {
    try {
      // Limpiar errores previos
      setRegisterError(null);
      clearError();

      // Preparar datos para el backend
      const registerData = {
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: data.phoneNumber || null,
        dateOfBirth: data.dateOfBirth || null,
        gender: data.gender !== undefined && data.gender !== null ? (data.gender as Gender) : null,
      };

      // Intentar registro
      await register(registerData);

      // Si llegamos aquí, el registro fue exitoso
      navigate('/products', { replace: true });

    } catch (error) {
      const errorMessage = error instanceof Error 
        ? error.message 
        : 'Error desconocido durante el registro';
      
      setRegisterError(errorMessage);
    }
  };

  // Manejar cambio de visibilidad de contraseñas
  const handleTogglePasswordVisibility = () => {
    setShowPassword(prev => !prev);
  };

  const handleToggleConfirmPasswordVisibility = () => {
    setShowConfirmPassword(prev => !prev);
  };

  // Limpiar errores cuando el usuario empiece a escribir
  const handleClearErrors = () => {
    if (registerError) setRegisterError(null);
    if (error) clearError();
  };

  // Navegar a login
  const handleGoToLogin = () => {
    navigate('/login');
  };

  const currentError = registerError || error;

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
      {/* Título */}
      <Typography variant="h4" component="h1" textAlign="center" gutterBottom>
        Crear Cuenta
      </Typography>

      <Typography variant="body2" color="text.secondary" textAlign="center">
        Completa tus datos para crear tu cuenta
      </Typography>

      {/* Error global */}
      {currentError && (
        <Alert 
          severity="error" 
          onClose={handleClearErrors}
          sx={{ mb: 2 }}
        >
          {currentError}
        </Alert>
      )}

      {/* Datos básicos */}
      <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
        {/* Nombre */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="firstName"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Nombre *"
                fullWidth
                error={!!errors.firstName}
                helperText={errors.firstName?.message}
                disabled={isLoading || isSubmitting}
                onChange={(e) => {
                  field.onChange(e);
                  handleClearErrors();
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                placeholder="Tu nombre"
              />
            )}
          />
        </Box>

        {/* Apellido */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="lastName"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Apellido *"
                fullWidth
                error={!!errors.lastName}
                helperText={errors.lastName?.message}
                disabled={isLoading || isSubmitting}
                onChange={(e) => {
                  field.onChange(e);
                  handleClearErrors();
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PersonIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                placeholder="Tu apellido"
              />
            )}
          />
        </Box>
      </Box>

      {/* Email */}
      <Controller
        name="email"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Email *"
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
            placeholder="ejemplo@correo.com"
          />
        )}
      />

      {/* Contraseñas */}
      <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
        {/* Contraseña */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Contraseña *"
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
                placeholder="Min. 8 caracteres"
              />
            )}
          />
        </Box>

        {/* Confirmar Contraseña */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="confirmPassword"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Confirmar Contraseña *"
                type={showConfirmPassword ? 'text' : 'password'}
                fullWidth
                error={!!errors.confirmPassword}
                helperText={errors.confirmPassword?.message}
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
                        onClick={handleToggleConfirmPasswordVisibility}
                        edge="end"
                        disabled={isLoading || isSubmitting}
                      >
                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
                placeholder="Repite tu contraseña"
              />
            )}
          />
        </Box>
      </Box>

      {/* Información adicional */}
      <Typography variant="h6" color="text.secondary" sx={{ mt: 2, mb: 1 }}>
        Información Adicional (Opcional)
      </Typography>

      <Box sx={{ display: 'flex', gap: 2, flexDirection: { xs: 'column', sm: 'row' } }}>
        {/* Teléfono */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="phoneNumber"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Teléfono"
                fullWidth
                error={!!errors.phoneNumber}
                helperText={errors.phoneNumber?.message}
                disabled={isLoading || isSubmitting}
                onChange={(e) => {
                  field.onChange(e);
                  handleClearErrors();
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <PhoneIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                placeholder="+1-555-123-4567"
              />
            )}
          />
        </Box>

        {/* Fecha de Nacimiento */}
        <Box sx={{ flex: 1 }}>
          <Controller
            name="dateOfBirth"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Fecha de Nacimiento"
                type="date"
                fullWidth
                error={!!errors.dateOfBirth}
                helperText={errors.dateOfBirth?.message}
                disabled={isLoading || isSubmitting}
                onChange={(e) => {
                  field.onChange(e);
                  handleClearErrors();
                }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <CakeIcon color="action" />
                    </InputAdornment>
                  ),
                }}
                InputLabelProps={{
                  shrink: true,
                }}
              />
            )}
          />
        </Box>
      </Box>

      {/* Género */}
      <Controller
        name="gender"
        control={control}
        render={({ field }) => (
          <FormControl fullWidth error={!!errors.gender}>
            <InputLabel>Género</InputLabel>
            <Select
              {...field}
              label="Género"
              disabled={isLoading || isSubmitting}
              onChange={(e) => {
                field.onChange(e);
                handleClearErrors();
              }}
            >
              <MenuItem value="">
                <em>Seleccionar</em>
              </MenuItem>
              {genderOptions.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
            {errors.gender && (
              <Typography variant="caption" color="error" sx={{ mt: 1, ml: 2 }}>
                {errors.gender.message}
              </Typography>
            )}
          </FormControl>
        )}
      />

      {/* Requisitos de contraseña */}
      {watchPassword && (
        <Alert severity="info" sx={{ mt: 1 }}>
          <Typography variant="caption">
            <strong>Requisitos de contraseña:</strong><br />
            • Al menos 8 caracteres<br />
            • Una letra mayúscula<br />
            • Una letra minúscula<br />
            • Un número<br />
            • Un carácter especial
          </Typography>
        </Alert>
      )}

      {/* Botón de Submit */}
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
            Creando cuenta...
          </>
        ) : (
          'Crear Cuenta'
        )}
      </Button>

      {/* Botón de Limpiar */}
      <Button
        variant="outlined"
        onClick={() => {
          reset();
          handleClearErrors();
        }}
        disabled={isLoading || isSubmitting}
        sx={{ py: 1.5 }}
      >
        Limpiar Formulario
      </Button>

      {/* Link a login */}
      <Box textAlign="center" mt={2}>
        <Typography variant="body2" color="text.secondary">
          ¿Ya tienes una cuenta?{' '}
          <Link
            component="button"
            type="button"
            onClick={handleGoToLogin}
            disabled={isLoading || isSubmitting}
            sx={{
              textDecoration: 'none',
              fontWeight: 'medium',
              '&:hover': {
                textDecoration: 'underline',
              },
            }}
          >
            Inicia sesión aquí
          </Link>
        </Typography>
      </Box>

      {/* Información de desarrollo */}
      {import.meta.env.DEV && (
        <Alert severity="info" sx={{ mt: 2 }}>
          <Typography variant="caption">
            <strong>Modo Desarrollo:</strong><br />
            Los campos marcados con * son obligatorios. 
            La contraseña debe cumplir todos los requisitos de seguridad.
          </Typography>
        </Alert>
      )}
    </Box>
  );
};

export default RegisterForm;