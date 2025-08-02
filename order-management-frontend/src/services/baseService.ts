import axios, { type AxiosInstance, type AxiosResponse } from 'axios';

export abstract class BaseService {
  protected apiClient: AxiosInstance;

  constructor(baseURL: string) {
    this.apiClient = axios.create({
      baseURL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add request interceptor to include JWT token
    this.apiClient.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('authToken');
        if (token && config.headers) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Add response interceptor for error handling
    this.apiClient.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Handle unauthorized access
          localStorage.removeItem('authToken');
          localStorage.removeItem('authUser');
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  protected async get<T>(url: string): Promise<T> {
    const response: AxiosResponse<T> = await this.apiClient.get(url);
    return response.data;
  }

  protected async post<T>(url: string, data?: any): Promise<T> {
    const response: AxiosResponse<T> = await this.apiClient.post(url, data);
    return response.data;
  }

  protected async put<T>(url: string, data?: any): Promise<T> {
    const response: AxiosResponse<T> = await this.apiClient.put(url, data);
    return response.data;
  }

  protected async delete<T>(url: string): Promise<T> {
    const response: AxiosResponse<T> = await this.apiClient.delete(url);
    return response.data;
  }

  protected async patch<T>(url: string, data?: any): Promise<T> {
    const response: AxiosResponse<T> = await this.apiClient.patch(url, data);
    return response.data;
  }
}