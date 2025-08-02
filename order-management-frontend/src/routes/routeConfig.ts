import { type RouteConfig } from '../types';

// Layout components
import MainLayout from '../components/layout/MainLayout';

// Page components
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import RegisterPage from '../pages/RegisterPage';
import ProductsPage from '../pages/ProductsPage';
import OrdersPage from '../pages/OrdersPage';
import CheckoutPage from '../pages/CheckoutPage';

// Configuración de rutas de la aplicación
export const routes: RouteConfig[] = [
  {
    path: '/',
    component: HomePage,
    exact: true,
    requiresAuth: false,
    title: 'Home',
    description: 'Welcome to Order Management System'
  },
  {
    path: '/login',
    component: LoginPage,
    requiresAuth: false,
    title: 'Login',
    description: 'Login to your account'
  },
  {
    path: '/register',
    component: RegisterPage,
    requiresAuth: false,
    title: 'Register',
    description: 'Create a new account'
  },
  {
    path: '/products',
    component: ProductsPage,
    requiresAuth: true,
    title: 'Products',
    description: 'Browse our product catalog'
  },
  {
    path: '/orders',
    component: OrdersPage,
    requiresAuth: true,
    title: 'My Orders',
    description: 'View and manage your orders'
  },
  {
    path: '/checkout',
    component: CheckoutPage,
    requiresAuth: true,
    title: 'Checkout',
    description: 'Complete your order'
  }
];

export { MainLayout };