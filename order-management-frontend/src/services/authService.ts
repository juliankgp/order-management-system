import { customerApiClient } from './apiClient';
import { API_ENDPOINTS } from '../constants/api';
import { 
  type LoginCustomerDto, 
  type RegisterCustomerDto, 
  type AuthResponse,
  type ApiResponse
} from '../types';

/**
 * Servicio de autenticación que maneja login, registro y operaciones relacionadas
 * Integra con CustomerService del backend
 */
class AuthService {
  /**
   * Realizar login de usuario
   * @param credentials - Email y contraseña del usuario
   * @returns Promise con datos de autenticación
   */
  async login(credentials: LoginCustomerDto): Promise<AuthResponse> {
    try {
      console.log('🔑 Iniciando login para:', credentials.email);
      
      const response = await customerApiClient.post<ApiResponse<AuthResponse>>(
        API_ENDPOINTS.CUSTOMERS.LOGIN,
        credentials
      ) as unknown as ApiResponse<AuthResponse>;

      // Validar que la respuesta sea exitosa
      if (!response.success || !response.data) {
        throw new Error(response.message || 'Error de autenticación');
      }

      const authData = response.data;

      // Validar que los datos requeridos estén presentes
      if (!authData.token || !authData.id || !authData.email) {
        throw new Error('Respuesta de autenticación incompleta');
      }

      console.log('✅ Login exitoso para:', authData.email);
      return authData;

    } catch (error) {
      console.error('❌ Error en login:', error);
      
      // Manejar diferentes tipos de errores
      if (error && typeof error === 'object' && 'message' in error) {
        throw new Error(error.message as string);
      }
      
      if (error && typeof error === 'object' && 'status' in error) {
        const status = error.status as number;
        switch (status) {
          case 400:
            throw new Error('Datos de login inválidos');
          case 401:
            throw new Error('Credenciales incorrectas');
          case 429:
            throw new Error('Demasiados intentos de login. Intenta más tarde.');
          case 500:
            throw new Error('Error del servidor. Intenta más tarde.');
          default:
            throw new Error('Error de conexión. Verifica tu internet.');
        }
      }

      throw new Error('Error desconocido durante el login');
    }
  }

  /**
   * Registrar nuevo usuario
   * @param userData - Datos del nuevo usuario
   * @returns Promise con datos de autenticación
   */
  async register(userData: RegisterCustomerDto): Promise<AuthResponse> {
    try {
      console.log('📝 Iniciando registro para:', userData.email);

      const response = await customerApiClient.post<ApiResponse<AuthResponse>>(
        API_ENDPOINTS.CUSTOMERS.REGISTER,
        userData
      ) as unknown as ApiResponse<AuthResponse>;

      // Validar que la respuesta sea exitosa
      if (!response.success || !response.data) {
        throw new Error(response.message || 'Error de registro');
      }

      const authData = response.data;

      // Validar que los datos requeridos estén presentes
      if (!authData.token || !authData.id || !authData.email) {
        throw new Error('Respuesta de registro incompleta');
      }

      console.log('✅ Registro exitoso para:', authData.email);
      return authData;

    } catch (error) {
      console.error('❌ Error en registro:', error);
      
      // Manejar diferentes tipos de errores
      if (error && typeof error === 'object' && 'message' in error) {
        throw new Error(error.message as string);
      }

      if (error && typeof error === 'object' && 'status' in error) {
        const status = error.status as number;
        switch (status) {
          case 400:
            throw new Error('Datos de registro inválidos');
          case 409:
            throw new Error('El email ya está registrado');
          case 429:
            throw new Error('Demasiados intentos de registro. Intenta más tarde.');
          case 500:
            throw new Error('Error del servidor. Intenta más tarde.');
          default:
            throw new Error('Error de conexión. Verifica tu internet.');
        }
      }

      throw new Error('Error desconocido durante el registro');
    }
  }

  /**
   * Validar si un token JWT sigue siendo válido
   * @param token - Token JWT a validar
   * @returns Promise<boolean> indicando si el token es válido
   */
  async validateToken(token: string): Promise<boolean> {
    try {
      // Hacer una petición de prueba que requiera autenticación
      const response = await customerApiClient.get<ApiResponse<unknown>>(
        API_ENDPOINTS.CUSTOMERS.PROFILE,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      ) as unknown as ApiResponse<unknown>;

      return response.success;
    } catch {
      console.warn('⚠️ Token inválido o expirado');
      return false;
    }
  }

  /**
   * Obtener perfil del usuario autenticado
   * @returns Promise con datos del perfil
   */
  async getProfile(): Promise<AuthResponse> {
    try {
      const response = await customerApiClient.get<ApiResponse<AuthResponse>>(
        API_ENDPOINTS.CUSTOMERS.PROFILE
      ) as unknown as ApiResponse<AuthResponse>;

      if (!response.success || !response.data) {
        throw new Error(response.message || 'Error al obtener perfil');
      }

      return response.data;
    } catch (error) {
      console.error('❌ Error al obtener perfil:', error);
      throw error;
    }
  }

  /**
   * Actualizar perfil del usuario
   * @param profileData - Datos a actualizar
   * @returns Promise con datos actualizados
   */
  async updateProfile(profileData: Partial<RegisterCustomerDto>): Promise<AuthResponse> {
    try {
      console.log('📝 Actualizando perfil...');

      const response = await customerApiClient.put<ApiResponse<AuthResponse>>(
        API_ENDPOINTS.CUSTOMERS.PROFILE,
        profileData
      ) as unknown as ApiResponse<AuthResponse>;

      if (!response.success || !response.data) {
        throw new Error(response.message || 'Error al actualizar perfil');
      }

      console.log('✅ Perfil actualizado exitosamente');
      return response.data;
    } catch (error) {
      console.error('❌ Error al actualizar perfil:', error);
      throw error;
    }
  }

  /**
   * Decodificar información básica del JWT (sin verificar firma)
   * Usado solo para obtener información de expiración
   * @param token - Token JWT
   * @returns Objeto con claims del token
   */
  decodeJWT(token: string): { exp?: number; email?: string; nameid?: string } | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }

      const payload = parts[1];
      const decoded = JSON.parse(atob(payload));
      
      return decoded;
    } catch (error) {
      console.error('❌ Error decodificando JWT:', error);
      return null;
    }
  }

  /**
   * Verificar si un token JWT ha expirado
   * @param token - Token JWT
   * @returns true si el token ha expirado
   */
  isTokenExpired(token: string): boolean {
    const decoded = this.decodeJWT(token);
    if (!decoded || !decoded.exp) {
      return true;
    }

    const currentTime = Math.floor(Date.now() / 1000);
    return decoded.exp < currentTime;
  }

  /**
   * Obtener tiempo restante hasta la expiración del token en milisegundos
   * @param token - Token JWT
   * @returns Tiempo en milisegundos hasta expiración, 0 si ya expiró
   */
  getTokenTimeToExpiry(token: string): number {
    const decoded = this.decodeJWT(token);
    if (!decoded || !decoded.exp) {
      return 0;
    }

    const currentTime = Math.floor(Date.now() / 1000);
    const timeToExpiry = decoded.exp - currentTime;
    
    return timeToExpiry > 0 ? timeToExpiry * 1000 : 0;
  }

  /**
   * Verificar conectividad con el servicio de autenticación
   * @returns Promise<boolean> indicando si el servicio está disponible
   */
  async checkServiceHealth(): Promise<boolean> {
    try {
      await customerApiClient.get(API_ENDPOINTS.CUSTOMERS.TEST);
      return true;
    } catch {
      console.warn('⚠️ Servicio de autenticación no disponible');
      return false;
    }
  }
}

// Exportar instancia singleton del servicio
export const authService = new AuthService();

// Exportar la clase para testing
export { AuthService };

export default authService;