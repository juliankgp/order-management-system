import React from 'react';
import { type RouteConfig } from '../types';

// Layout components
import MainLayout from '../components/layout/MainLayout';

// Lazy-loaded page components for better performance
const HomePage = React.lazy(() => import('../pages/HomePage'));
const LoginPage = React.lazy(() => import('../pages/LoginPage'));
const RegisterPage = React.lazy(() => import('../pages/RegisterPage'));
const ProductsPage = React.lazy(() => import('../pages/ProductsPage'));
const OrdersPage = React.lazy(() => import('../pages/OrdersPage'));
const CheckoutPage = React.lazy(() => import('../pages/CheckoutPage'));
const NotFoundPage = React.lazy(() => import('../pages/NotFoundPage'));

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
    path: '*',
    component: NotFoundPage,
    requiresAuth: false,
    title: '404 - Page Not Found',
    description: 'The requested page could not be found'
  }
];

export { MainLayout };