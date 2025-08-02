import { AuthProvider } from './context/AuthContext';
import { CartProvider } from './contexts/CartContext';
import AppRouter from './routes';

function App() {
  return (
    <AuthProvider>
      <CartProvider>
        <AppRouter />
      </CartProvider>
    </AuthProvider>
  );
}

export default App;
