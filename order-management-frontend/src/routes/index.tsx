import React, { Suspense } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Box, CircularProgress } from '@mui/material';

// Import route configuration
import { routes, MainLayout } from './routeConfig';

// Protected route wrapper
import ProtectedRoute from '../components/common/ProtectedRoute';

// Loading component for Suspense
const PageLoader = () => (
  <Box
    sx={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      minHeight: '50vh',
    }}
  >
    <CircularProgress size={48} />
  </Box>
);

// Crear el router con las rutas configuradas
const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: routes.map(route => ({
      path: route.path === '/' ? '' : route.path,
      element: (
        <Suspense fallback={<PageLoader />}>
          {route.requiresAuth ? (
            <ProtectedRoute>
              <route.component />
            </ProtectedRoute>
          ) : (
            <route.component />
          )}
        </Suspense>
      )
    }))
  }
]);

// Componente AppRouter que proporciona el enrutamiento
const AppRouter: React.FC = () => {
  return <RouterProvider router={router} />;
};

export default AppRouter;