import React from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';

// Import route configuration
import { routes, MainLayout } from './routeConfig';

// Protected route wrapper
import ProtectedRoute from '../components/common/ProtectedRoute';

// Crear el router con las rutas configuradas
const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: routes.map(route => ({
      path: route.path === '/' ? '' : route.path,
      element: route.requiresAuth ? (
        <ProtectedRoute>
          <route.component />
        </ProtectedRoute>
      ) : (
        <route.component />
      )
    }))
  }
]);

// Componente AppRouter que proporciona el enrutamiento
const AppRouter: React.FC = () => {
  return <RouterProvider router={router} />;
};

export default AppRouter;