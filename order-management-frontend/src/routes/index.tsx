import React, { Suspense } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';

// Import route configuration
import { routes, MainLayout } from './routeConfig';

// Protected route wrapper
import ProtectedRoute from '../components/common/ProtectedRoute';

// Loading component
import { PageLoading } from '../components/common/Loading';

// Error pages (keep these non-lazy for immediate availability)
import NotFoundPage from '../pages/NotFoundPage';
import ErrorPage from '../pages/ErrorPage';

// Crear el router con las rutas configuradas
const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    errorElement: <ErrorPage />,
    children: [
      ...routes.map(route => ({
        path: route.path === '/' ? '' : route.path,
        element: (
          <Suspense fallback={<PageLoading />}>
            {route.requiresAuth ? (
              <ProtectedRoute>
                <route.component />
              </ProtectedRoute>
            ) : (
              <route.component />
            )}
          </Suspense>
        )
      })),
      {
        path: '*',
        element: <NotFoundPage />
      }
    ]
  }
]);

// Componente AppRouter que proporciona el enrutamiento
const AppRouter: React.FC = () => {
  return <RouterProvider router={router} />;
};

export default AppRouter;