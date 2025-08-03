/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useReducer, useEffect, useCallback, type ReactNode } from 'react';
import { type AuthResponse, type LoginCustomerDto, type RegisterCustomerDto, type User } from '../types';

// Tipos para el contexto de autenticación
interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthContextType extends AuthState {
  login: (credentials: LoginCustomerDto) => Promise<void>;
  register: (userData: RegisterCustomerDto) => Promise<void>;
  logout: () => void;
  clearError: () => void;
  updateProfile: (userData: Partial<User>) => void;
}

// Acciones del reducer
type AuthAction =
  | { type: 'AUTH_START' }
  | { type: 'AUTH_SUCCESS'; payload: { user: User; token: string } }
  | { type: 'AUTH_FAILURE'; payload: string }
  | { type: 'AUTH_LOGOUT' }
  | { type: 'CLEAR_ERROR' }
  | { type: 'UPDATE_PROFILE'; payload: Partial<User> }
  | { type: 'SET_LOADING'; payload: boolean };

// Estado inicial
const initialState: AuthState = {
  user: null,
  token: null,
  isAuthenticated: false,
  isLoading: true, // Inicialmente true para verificar sesión existente
  error: null,
};

// Reducer para manejar el estado de autenticación
const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'AUTH_START':
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case 'AUTH_SUCCESS':
      return {
        ...state,
        user: action.payload.user,
        token: action.payload.token,
        isAuthenticated: true,
        isLoading: false,
        error: null,
      };

    case 'AUTH_FAILURE':
      return {
        ...state,
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
        error: action.payload,
      };

    case 'AUTH_LOGOUT':
      return {
        ...state,
        user: null,
        token: null,
        isAuthenticated: false,
        isLoading: false,
        error: null,
      };

    case 'CLEAR_ERROR':
      return {
        ...state,
        error: null,
      };

    case 'UPDATE_PROFILE':
      return {
        ...state,
        user: state.user ? { ...state.user, ...action.payload } : null,
      };

    case 'SET_LOADING':
      return {
        ...state,
        isLoading: action.payload,
      };

    default:
      return state;
  }
};

// Crear el contexto
const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Constantes para localStorage
const AUTH_TOKEN_KEY = 'authToken';
const AUTH_USER_KEY = 'user';

// Función helper para convertir AuthResponse a User
const authResponseToUser = (authResponse: AuthResponse): User => ({
  id: authResponse.id,
  email: authResponse.email,
  fullName: authResponse.fullName,
  emailVerified: authResponse.emailVerified,
});

// Función helper para configurar timer de expiración
const setupTokenExpirationTimer = (
  tokenExpires: string,
  onExpire: () => void
): (() => void) => {
  const expirationTime = new Date(tokenExpires).getTime();
  const currentTime = new Date().getTime();
  const timeUntilExpiration = expirationTime - currentTime;

  // Si el token ya expiró, ejecutar logout inmediatamente
  if (timeUntilExpiration <= 0) {
    onExpire();
    return () => {}; // Retornar función vacía
  }

  // Configurar timer para logout automático
  const timerId = setTimeout(() => {
    onExpire();
  }, timeUntilExpiration);

  // Retornar función para limpiar el timer
  return () => clearTimeout(timerId);
};

// Props del provider
interface AuthProviderProps {
  children: ReactNode;
}

// AuthProvider component
export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);
  const [clearExpirationTimer, setClearExpirationTimer] = React.useState<(() => void) | null>(null);

  // Función para limpiar la sesión
  const clearSession = useCallback(() => {
    localStorage.removeItem(AUTH_TOKEN_KEY);
    localStorage.removeItem(AUTH_USER_KEY);
    if (clearExpirationTimer) {
      clearExpirationTimer();
      setClearExpirationTimer(null);
    }
  }, [clearExpirationTimer]);

  // Función de logout
  const logout = () => {
    clearSession();
    dispatch({ type: 'AUTH_LOGOUT' });
  };

  // Función para establecer la sesión
  const setSession = (authResponse: AuthResponse) => {
    const user = authResponseToUser(authResponse);
    
    // Guardar en localStorage
    localStorage.setItem(AUTH_TOKEN_KEY, authResponse.token);
    localStorage.setItem(AUTH_USER_KEY, JSON.stringify(user));

    // Actualizar estado
    dispatch({
      type: 'AUTH_SUCCESS',
      payload: { user, token: authResponse.token }
    });

    // Configurar timer de expiración
    const clearTimer = setupTokenExpirationTimer(authResponse.tokenExpires, logout);
    setClearExpirationTimer(() => clearTimer);

  };

  // Función de login
  const login = async (credentials: LoginCustomerDto): Promise<void> => {
    dispatch({ type: 'AUTH_START' });

    try {
      // Importar el servicio dinámicamente para evitar dependencias circulares
      const { authService } = await import('../services/authService');
      const authResponse = await authService.login(credentials);
      
      setSession(authResponse);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error de login desconocido';
      dispatch({ type: 'AUTH_FAILURE', payload: errorMessage });
      throw error;
    }
  };

  // Función de register
  const register = async (userData: RegisterCustomerDto): Promise<void> => {
    dispatch({ type: 'AUTH_START' });

    try {
      // Importar el servicio dinámicamente
      const { authService } = await import('../services/authService');
      const authResponse = await authService.register(userData);
      
      setSession(authResponse);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Error de registro desconocido';
      dispatch({ type: 'AUTH_FAILURE', payload: errorMessage });
      throw error;
    }
  };

  // Función para limpiar errores
  const clearError = () => {
    dispatch({ type: 'CLEAR_ERROR' });
  };

  // Función para actualizar perfil
  const updateProfile = (userData: Partial<User>) => {
    dispatch({ type: 'UPDATE_PROFILE', payload: userData });
    
    // Actualizar localStorage
    if (state.user) {
      const updatedUser = { ...state.user, ...userData };
      localStorage.setItem(AUTH_USER_KEY, JSON.stringify(updatedUser));
    }
  };

  // Efecto para verificar sesión existente al cargar la aplicación
  useEffect(() => {
    const checkExistingSession = () => {
      try {
        const token = localStorage.getItem(AUTH_TOKEN_KEY);
        const userString = localStorage.getItem(AUTH_USER_KEY);

        if (token && userString) {
          const user = JSON.parse(userString) as User;
          
          // Verificar que el token no haya expirado (básico)
          // En una implementación real, podrías decodificar el JWT para verificar la expiración
          dispatch({
            type: 'AUTH_SUCCESS',
            payload: { user, token }
          });

        } else {
          dispatch({ type: 'SET_LOADING', payload: false });
        }
      } catch {
        clearSession();
        dispatch({ type: 'SET_LOADING', payload: false });
      }
    };

    checkExistingSession();
  }, [clearSession]);

  // Cleanup al desmontar el componente
  useEffect(() => {
    return () => {
      if (clearExpirationTimer) {
        clearExpirationTimer();
      }
    };
  }, [clearExpirationTimer]);

  // Valor del contexto
  const contextValue: AuthContextType = {
    ...state,
    login,
    register,
    logout,
    clearError,
    updateProfile,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

// Hook personalizado para usar el contexto
export const useAuth = (): AuthContextType => {
  const context = React.useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export default AuthContext;