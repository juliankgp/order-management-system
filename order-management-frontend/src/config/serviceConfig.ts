// Configuración de servicios basada en variables de entorno
// Este archivo centraliza las URLs de los servicios backend

interface ServiceConfig {
  orderService: string;
  productService: string;
  customerService: string;
  loggingService: string;
}

// Función para obtener la URL base dependiendo del entorno
const getServiceUrl = (serviceName: string, defaultPort: string): string => {
  // En desarrollo con Docker, usar URLs externas (localhost) para que el navegador pueda acceder
  if (import.meta.env.MODE === 'development' && import.meta.env.VITE_DOCKER_MODE === 'true') {
    const dockerUrls: Record<string, string> = {
      order: import.meta.env.VITE_ORDER_SERVICE_EXTERNAL_URL || 'http://localhost:5001',
      product: import.meta.env.VITE_PRODUCT_SERVICE_EXTERNAL_URL || 'http://localhost:5002',
      customer: import.meta.env.VITE_CUSTOMER_SERVICE_EXTERNAL_URL || 'http://localhost:5003',
      logging: import.meta.env.VITE_LOGGING_SERVICE_EXTERNAL_URL || 'http://localhost:5004'
    };
    return dockerUrls[serviceName] || `http://localhost:${defaultPort}`;
  }

  // En desarrollo local o producción, usar variables de entorno o localhost
  const envUrls: Record<string, string> = {
    order: import.meta.env.VITE_ORDER_SERVICE_URL || `http://localhost:5001`,
    product: import.meta.env.VITE_PRODUCT_SERVICE_URL || `http://localhost:5002`,
    customer: import.meta.env.VITE_CUSTOMER_SERVICE_URL || `http://localhost:5003`,
    logging: import.meta.env.VITE_LOGGING_SERVICE_URL || `http://localhost:5004`
  };

  return envUrls[serviceName] || `http://localhost:${defaultPort}`;
};

// Configuración de servicios
export const serviceConfig: ServiceConfig = {
  orderService: getServiceUrl('order', '5001'),
  productService: getServiceUrl('product', '5002'),
  customerService: getServiceUrl('customer', '5003'),
  loggingService: getServiceUrl('logging', '5004')
};

// URLs para desarrollo y debugging
export const developmentUrls = {
  orderSwagger: `${serviceConfig.orderService}/swagger`,
  productSwagger: `${serviceConfig.productService}/swagger`,
  customerSwagger: `${serviceConfig.customerService}/swagger`,
  loggingSwagger: `${serviceConfig.loggingService}/swagger`,
  rabbitmqManagement: 'http://localhost:15672'
};

// Función para detectar el modo de ejecución
export const getExecutionMode = () => {
  if (import.meta.env.VITE_DOCKER_MODE === 'true') {
    return 'docker';
  } else if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    return 'local';
  } else {
    return 'production';
  }
};

// Información del entorno actual
export const environmentInfo = {
  mode: import.meta.env.MODE,
  isDevelopment: import.meta.env.DEV,
  isProduction: import.meta.env.PROD,
  dockerMode: import.meta.env.VITE_DOCKER_MODE === 'true',
  executionMode: getExecutionMode()
};

// Función de utilidad para logging
export const logServiceUrls = () => {
  if (import.meta.env.MODE === 'development') {
    console.log('🔧 Service URLs Configuration:');
    console.log('Execution Mode:', getExecutionMode());
    console.log('Docker Mode:', environmentInfo.dockerMode);
    console.log('Environment:', import.meta.env.MODE);
    console.log('---');
    console.log('Order Service:', serviceConfig.orderService);
    console.log('Product Service:', serviceConfig.productService);
    console.log('Customer Service:', serviceConfig.customerService);
    console.log('Logging Service:', serviceConfig.loggingService);
    console.log('---');
    console.log('🔗 Development URLs:');
    Object.entries(developmentUrls).forEach(([key, url]) => {
      console.log(`${key}:`, url);
    });
  }
};

export default serviceConfig;
