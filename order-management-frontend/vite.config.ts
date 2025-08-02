/// <reference types="vitest" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/setupTests.ts'],
    css: true,
  },
  build: {
    // Optimize chunk splitting for better caching
    rollupOptions: {
      output: {
        manualChunks: {
          // Vendor chunks
          'vendor-react': ['react', 'react-dom', 'react-router-dom'],
          'vendor-mui': ['@mui/material', '@mui/icons-material', '@emotion/react', '@emotion/styled'],
          'vendor-forms': ['react-hook-form', '@hookform/resolvers', 'yup'],
          'vendor-http': ['axios'],
          
          // App chunks
          'pages': [
            './src/pages/HomePage',
            './src/pages/ProductsPage',
            './src/pages/OrdersPage',
            './src/pages/CheckoutPage'
          ],
          'auth': [
            './src/pages/LoginPage',
            './src/pages/RegisterPage',
            './src/context/AuthContext',
            './src/services/authService'
          ],
          'components': [
            './src/components/layout/Header',
            './src/components/layout/Footer',
            './src/components/common/ErrorBoundary',
            './src/components/common/NotificationSystem'
          ]
        }
      }
    },
    // Enable source maps for debugging in production
    sourcemap: true,
    // Set chunk size warning limit
    chunkSizeWarningLimit: 600
  },
  // Optimize dependencies
  optimizeDeps: {
    include: [
      'react',
      'react-dom',
      'react-router-dom',
      '@mui/material',
      '@mui/icons-material',
      'axios'
    ]
  }
})
