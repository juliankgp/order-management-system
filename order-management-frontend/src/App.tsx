import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { ErrorBoundary, NotificationProvider, ConfirmProvider } from './components/common';
import AppRouter from './routes';

function App() {
  return (
    <ErrorBoundary>
      <NotificationProvider>
        <ConfirmProvider>
          <AuthProvider>
            <CartProvider>
              <AppRouter />
            </CartProvider>
          </AuthProvider>
        </ConfirmProvider>
      </NotificationProvider>
    </ErrorBoundary>
  );
}

export default App;
