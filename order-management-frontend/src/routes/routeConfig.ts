import { type RouteConfig } from '../types';

// Layout components
import MainLayout from '../components/layout/MainLayout';

import { lazy } from 'react';

// Page components with code splitting
const HomePage = lazy(() => import('../pages/HomePage'));
const LoginPage = lazy(() => import('../pages/LoginPage'));
const RegisterPage = lazy(() => import('../pages/RegisterPage'));
const ProductsPage = lazy(() => import('../pages/ProductsPage'));
const OrdersPage = lazy(() => import('../pages/OrdersPage'));
const CheckoutPage = lazy(() => import('../pages/CheckoutPage'));
const ProfilePage = lazy(() => import('../pages/ProfilePage'));

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
  },
  {
    path: '/profile',
    component: ProfilePage,
    requiresAuth: true,
    title: 'Profile',
    description: 'Manage your profile settings'
  }
];

export { MainLayout };