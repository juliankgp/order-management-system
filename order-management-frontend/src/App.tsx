import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './contexts/CartContext';
import { NotificationProvider } from './components/common/NotificationSystem';
import ErrorBoundary from './components/common/ErrorBoundary';
import AppRouter from './routes';

function App() {
  return (
    <ErrorBoundary>
      <NotificationProvider>
        <AuthProvider>
          <CartProvider>
            <AppRouter />
          </CartProvider>
        </AuthProvider>
      </NotificationProvider>
    </ErrorBoundary>
  );
}

export default App;
