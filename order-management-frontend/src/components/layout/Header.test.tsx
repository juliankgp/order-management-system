import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import Header from './Header';

// Mock del AuthContext
const mockAuthContext = {
  isAuthenticated: false,
  user: null,
  login: vi.fn(),
  logout: vi.fn(),
  isLoading: false
};

// Mock del CartContext
const mockCartContext = {
  items: [],
  itemCount: 0,
  subtotal: 0,
  total: 0,
  addItem: vi.fn(),
  removeItem: vi.fn(),
  updateQuantity: vi.fn(),
  clearCart: vi.fn()
};

vi.mock('../../context/AuthContext', () => ({
  useAuth: () => mockAuthContext
}));

vi.mock('../../contexts/CartContext', () => ({
  useCart: () => mockCartContext
}));

const renderWithRouter = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('Header', () => {
  it('renders app title', () => {
    renderWithRouter(<Header />);
    expect(screen.getByText('Order Management System')).toBeInTheDocument();
  });

  it('shows login and register buttons when not authenticated', () => {
    renderWithRouter(<Header />);
    expect(screen.getByText('Login')).toBeInTheDocument();
    expect(screen.getByText('Register')).toBeInTheDocument();
  });

  it('shows navigation links when authenticated', () => {
    mockAuthContext.isAuthenticated = true;
    mockAuthContext.user = { 
      id: '1', 
      fullName: 'Test User', 
      email: 'test@test.com',
      firstName: 'Test',
      lastName: 'User'
    };
    
    renderWithRouter(<Header />);
    expect(screen.getByText('Products')).toBeInTheDocument();
    expect(screen.getByText('My Orders')).toBeInTheDocument();
  });
});