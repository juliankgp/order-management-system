/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useReducer, useEffect } from 'react';
import type { CartItem, Cart, ProductDto } from '../types/entities';

interface CartState {
  cart: Cart;
  isLoading: boolean;
  error: string | null;
}

type CartAction = 
  | { type: 'ADD_ITEM'; payload: { product: ProductDto; quantity: number } }
  | { type: 'REMOVE_ITEM'; payload: { productId: string } }
  | { type: 'UPDATE_QUANTITY'; payload: { productId: string; quantity: number } }
  | { type: 'CLEAR_CART' }
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'LOAD_CART'; payload: Cart };

interface CartContextType {
  state: CartState;
  addItem: (product: ProductDto, quantity: number) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  getItemQuantity: (productId: string) => number;
  isInCart: (productId: string) => boolean;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

const CartContext = createContext<CartContextType | undefined>(undefined);

const TAX_RATE = 0.1; // 10% tax rate
const SHIPPING_COST = 9.99; // Fixed shipping cost

// Helper function to calculate cart totals
const calculateCartTotals = (items: CartItem[]): Omit<Cart, 'items' | 'itemCount'> => {
  const subtotal = items.reduce((sum, item) => sum + item.subtotal, 0);
  const taxAmount = subtotal * TAX_RATE;
  const shippingCost = items.length > 0 ? SHIPPING_COST : 0;
  const total = subtotal + taxAmount + shippingCost;

  return {
    subtotal: Number(subtotal.toFixed(2)),
    taxAmount: Number(taxAmount.toFixed(2)),
    shippingCost: Number(shippingCost.toFixed(2)),
    total: Number(total.toFixed(2))
  };
};

// Helper function to create cart item
const createCartItem = (product: ProductDto, quantity: number): CartItem => ({
  product,
  quantity,
  subtotal: Number((product.price * quantity).toFixed(2))
});

// Helper function to create empty cart
const createEmptyCart = (): Cart => ({
  items: [],
  itemCount: 0,
  subtotal: 0,
  taxAmount: 0,
  shippingCost: 0,
  total: 0
});

// Cart reducer
const cartReducer = (state: CartState, action: CartAction): CartState => {
  switch (action.type) {
    case 'ADD_ITEM': {
      const { product, quantity } = action.payload;
      const existingItemIndex = state.cart.items.findIndex(item => item.product.id === product.id);
      
      let newItems: CartItem[];
      
      if (existingItemIndex >= 0) {
        // Update existing item quantity
        newItems = state.cart.items.map((item, index) => 
          index === existingItemIndex 
            ? createCartItem(product, item.quantity + quantity)
            : item
        );
      } else {
        // Add new item
        newItems = [...state.cart.items, createCartItem(product, quantity)];
      }
      
      const totals = calculateCartTotals(newItems);
      
      return {
        ...state,
        cart: {
          items: newItems,
          itemCount: newItems.reduce((sum, item) => sum + item.quantity, 0),
          ...totals
        },
        error: null
      };
    }
    
    case 'REMOVE_ITEM': {
      const { productId } = action.payload;
      const newItems = state.cart.items.filter(item => item.product.id !== productId);
      const totals = calculateCartTotals(newItems);
      
      return {
        ...state,
        cart: {
          items: newItems,
          itemCount: newItems.reduce((sum, item) => sum + item.quantity, 0),
          ...totals
        },
        error: null
      };
    }
    
    case 'UPDATE_QUANTITY': {
      const { productId, quantity } = action.payload;
      
      if (quantity <= 0) {
        // Remove item if quantity is 0 or negative
        return cartReducer(state, { type: 'REMOVE_ITEM', payload: { productId } });
      }
      
      const newItems = state.cart.items.map(item => 
        item.product.id === productId 
          ? createCartItem(item.product, quantity)
          : item
      );
      
      const totals = calculateCartTotals(newItems);
      
      return {
        ...state,
        cart: {
          items: newItems,
          itemCount: newItems.reduce((sum, item) => sum + item.quantity, 0),
          ...totals
        },
        error: null
      };
    }
    
    case 'CLEAR_CART':
      return {
        ...state,
        cart: createEmptyCart(),
        error: null
      };
    
    case 'SET_LOADING':
      return {
        ...state,
        isLoading: action.payload
      };
    
    case 'SET_ERROR':
      return {
        ...state,
        error: action.payload,
        isLoading: false
      };
    
    case 'LOAD_CART':
      return {
        ...state,
        cart: action.payload,
        error: null,
        isLoading: false
      };
    
    default:
      return state;
  }
};

// Initial state
const initialState: CartState = {
  cart: createEmptyCart(),
  isLoading: false,
  error: null
};

// Local storage key
const CART_STORAGE_KEY = 'orderManagement_cart';

export const CartProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [state, dispatch] = useReducer(cartReducer, initialState);

  // Load cart from localStorage on initialization
  useEffect(() => {
    try {
      const savedCart = localStorage.getItem(CART_STORAGE_KEY);
      if (savedCart) {
        const parsedCart: Cart = JSON.parse(savedCart);
        dispatch({ type: 'LOAD_CART', payload: parsedCart });
      }
    } catch (error) {
      console.error('Error loading cart from localStorage:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to load saved cart' });
    }
  }, []);

  // Save cart to localStorage whenever cart changes
  useEffect(() => {
    try {
      localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(state.cart));
    } catch (error) {
      console.error('Error saving cart to localStorage:', error);
    }
  }, [state.cart]);

  // Context methods
  const addItem = (product: ProductDto, quantity: number) => {
    if (quantity <= 0) {
      dispatch({ type: 'SET_ERROR', payload: 'Quantity must be greater than 0' });
      return;
    }
    
    if (product.stock < quantity) {
      dispatch({ type: 'SET_ERROR', payload: `Only ${product.stock} items available in stock` });
      return;
    }
    
    dispatch({ type: 'ADD_ITEM', payload: { product, quantity } });
  };

  const removeItem = (productId: string) => {
    dispatch({ type: 'REMOVE_ITEM', payload: { productId } });
  };

  const updateQuantity = (productId: string, quantity: number) => {
    if (quantity < 0) {
      dispatch({ type: 'SET_ERROR', payload: 'Quantity cannot be negative' });
      return;
    }
    
    // Find the product in cart to check stock
    const cartItem = state.cart.items.find(item => item.product.id === productId);
    if (cartItem && quantity > cartItem.product.stock) {
      dispatch({ type: 'SET_ERROR', payload: `Only ${cartItem.product.stock} items available in stock` });
      return;
    }
    
    dispatch({ type: 'UPDATE_QUANTITY', payload: { productId, quantity } });
  };

  const clearCart = () => {
    dispatch({ type: 'CLEAR_CART' });
  };

  const getItemQuantity = (productId: string): number => {
    const item = state.cart.items.find(item => item.product.id === productId);
    return item ? item.quantity : 0;
  };

  const isInCart = (productId: string): boolean => {
    return state.cart.items.some(item => item.product.id === productId);
  };

  const setLoading = (loading: boolean) => {
    dispatch({ type: 'SET_LOADING', payload: loading });
  };

  const setError = (error: string | null) => {
    dispatch({ type: 'SET_ERROR', payload: error });
  };

  const contextValue: CartContextType = {
    state,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    getItemQuantity,
    isInCart,
    setLoading,
    setError
  };

  return (
    <CartContext.Provider value={contextValue}>
      {children}
    </CartContext.Provider>
  );
};

// Custom hook to use cart context
export const useCart = (): CartContextType => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};