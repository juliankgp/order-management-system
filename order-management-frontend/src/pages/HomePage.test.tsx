import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import HomePage from './HomePage';

// Mock del AuthContext
const mockAuthContext = {
  isAuthenticated: false,
  user: null,
  login: vi.fn(),
  logout: vi.fn(),
  isLoading: false
};

vi.mock('../context/AuthContext', () => ({
  useAuth: () => mockAuthContext
}));

const renderWithRouter = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      {component}
    </BrowserRouter>
  );
};

describe('HomePage', () => {
  it('renders welcome message', () => {
    renderWithRouter(<HomePage />);
    expect(screen.getByText('Order Management System')).toBeInTheDocument();
  });

  it('shows login button when not authenticated', () => {
    renderWithRouter(<HomePage />);
    expect(screen.getByText('Login')).toBeInTheDocument();
    expect(screen.getByText('Create Account')).toBeInTheDocument();
  });

  it('shows features section', () => {
    renderWithRouter(<HomePage />);
    expect(screen.getByText('Key Features')).toBeInTheDocument();
    expect(screen.getByText('Product Catalog')).toBeInTheDocument();
    expect(screen.getByText('Order Management')).toBeInTheDocument();
    expect(screen.getByText('Shopping Cart')).toBeInTheDocument();
  });
});