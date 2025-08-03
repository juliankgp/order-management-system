import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingSpinner, LoadingSkeleton } from './Loading';

describe('LoadingSpinner', () => {
  it('renders loading spinner', () => {
    render(<LoadingSpinner />);
    expect(screen.getByRole('progressbar')).toBeInTheDocument();
  });

  it('shows message when provided', () => {
    render(<LoadingSpinner message="Loading data..." />);
    expect(screen.getByText('Loading data...')).toBeInTheDocument();
  });
});

describe('LoadingSkeleton', () => {
  it('renders card skeleton by default', () => {
    render(<LoadingSkeleton />);
    expect(document.querySelector('.MuiSkeleton-root')).toBeInTheDocument();
  });
});