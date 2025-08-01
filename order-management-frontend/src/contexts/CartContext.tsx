import React, { createContext, useContext, useReducer, useEffect, ReactNode } from 'react';
import type { ProductDto, CartItem, Cart } from '../types';
import { orderService } from '../services/orderService';

// Cart Actions
type CartAction =
  | { type: 'ADD_ITEM'; payload: { product: ProductDto; quantity: number } }
  | { type: 'REMOVE_ITEM'; payload: { productId: string } }
  | { type: 'UPDATE_QUANTITY'; payload: { productId: string; quantity: number } }
  | { type: 'CLEAR_CART' }
  | { type: 'LOAD_CART'; payload: CartItem[] };

// Cart State
interface CartState {
  items: CartItem[];
  itemCount: number;
  subtotal: number;
  taxAmount: number;
  shippingCost: number;
  total: number;
}

// Cart Context Type
interface CartContextType {
  cart: CartState;
  addItem: (product: ProductDto, quantity: number) => void;
  removeItem: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  getItemQuantity: (productId: string) => number;
  isInCart: (productId: string) => boolean;
  hasItems: boolean;
}

// Initial State
const initialState: CartState = {
  items: [],
  itemCount: 0,
  subtotal: 0,
  taxAmount: 0,
  shippingCost: 0,
  total: 0
};

// Calculate cart totals
const calculateTotals = (items: CartItem[]): Omit<CartState, 'items'> => {
  const itemCount = items.reduce((sum, item) => sum + item.quantity, 0);
  const subtotal = items.reduce((sum, item) => sum + item.subtotal, 0);
  
  // Use orderService for consistent calculation logic
  const orderCalculation = orderService.calculateOrderTotals(
    items.map(item => ({
      quantity: item.quantity,
      unitPrice: item.product.price
    }))
  );

  return {
    itemCount,
    subtotal: orderCalculation.subtotal,
    taxAmount: orderCalculation.taxAmount,
    shippingCost: orderCalculation.shippingCost,
    total: orderCalculation.total
  };
};

// Create cart item from product and quantity
const createCartItem = (product: ProductDto, quantity: number): CartItem => ({
  product,
  quantity,
  subtotal: Math.round(product.price * quantity * 100) / 100
});

// Cart Reducer
const cartReducer = (state: CartState, action: CartAction): CartState => {
  let newItems: CartItem[];

  switch (action.type) {
    case 'ADD_ITEM': {
      const { product, quantity } = action.payload;
      const existingItemIndex = state.items.findIndex(item => item.product.id === product.id);

      if (existingItemIndex >= 0) {
        // Update existing item quantity
        newItems = state.items.map((item, index) => {
          if (index === existingItemIndex) {
            const newQuantity = item.quantity + quantity;
            return createCartItem(product, newQuantity);
          }
          return item;
        });
      } else {
        // Add new item
        newItems = [...state.items, createCartItem(product, quantity)];
      }

      const totals = calculateTotals(newItems);
      return { items: newItems, ...totals };
    }

    case 'REMOVE_ITEM': {
      const { productId } = action.payload;
      newItems = state.items.filter(item => item.product.id !== productId);
      const totals = calculateTotals(newItems);
      return { items: newItems, ...totals };
    }

    case 'UPDATE_QUANTITY': {
      const { productId, quantity } = action.payload;
      
      if (quantity <= 0) {
        // Remove item if quantity is 0 or less
        newItems = state.items.filter(item => item.product.id !== productId);
      } else {
        // Update item quantity
        newItems = state.items.map(item => {
          if (item.product.id === productId) {
            return createCartItem(item.product, quantity);
          }
          return item;
        });
      }

      const totals = calculateTotals(newItems);
      return { items: newItems, ...totals };
    }

    case 'CLEAR_CART': {
      return initialState;
    }

    case 'LOAD_CART': {
      const items = action.payload;
      const totals = calculateTotals(items);
      return { items, ...totals };
    }

    default:
      return state;
  }
};

// Create Context
const CartContext = createContext<CartContextType | undefined>(undefined);

// Local Storage Key
const CART_STORAGE_KEY = 'orderManagement_cart';

// Cart Provider Props
interface CartProviderProps {
  children: ReactNode;
}

// Cart Provider Component
export const CartProvider: React.FC<CartProviderProps> = ({ children }) => {
  const [cart, dispatch] = useReducer(cartReducer, initialState);

  // Load cart from localStorage on mount
  useEffect(() => {
    try {
      const savedCart = localStorage.getItem(CART_STORAGE_KEY);
      if (savedCart) {
        const parsedCart: CartItem[] = JSON.parse(savedCart);
        if (Array.isArray(parsedCart) && parsedCart.length > 0) {
          dispatch({ type: 'LOAD_CART', payload: parsedCart });
        }
      }
    } catch (error) {
      console.error('Error loading cart from localStorage:', error);
      localStorage.removeItem(CART_STORAGE_KEY);
    }
  }, []);

  // Save cart to localStorage whenever it changes
  useEffect(() => {
    try {
      if (cart.items.length > 0) {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart.items));
      } else {
        localStorage.removeItem(CART_STORAGE_KEY);
      }
    } catch (error) {
      console.error('Error saving cart to localStorage:', error);
    }
  }, [cart.items]);

  // Cart Actions
  const addItem = (product: ProductDto, quantity: number) => {
    if (quantity <= 0) {
      console.warn('Cannot add item with quantity <= 0');
      return;
    }

    if (quantity > product.stockQuantity) {
      console.warn(`Cannot add ${quantity} items. Only ${product.stockQuantity} available.`);
      return;
    }

    dispatch({ type: 'ADD_ITEM', payload: { product, quantity } });
  };

  const removeItem = (productId: string) => {
    dispatch({ type: 'REMOVE_ITEM', payload: { productId } });
  };

  const updateQuantity = (productId: string, quantity: number) => {
    const item = cart.items.find(item => item.product.id === productId);
    if (item && quantity > item.product.stockQuantity) {
      console.warn(`Cannot set quantity to ${quantity}. Only ${item.product.stockQuantity} available.`);
      return;
    }

    dispatch({ type: 'UPDATE_QUANTITY', payload: { productId, quantity } });
  };

  const clearCart = () => {
    dispatch({ type: 'CLEAR_CART' });
  };

  // Helper Functions
  const getItemQuantity = (productId: string): number => {
    const item = cart.items.find(item => item.product.id === productId);
    return item ? item.quantity : 0;
  };

  const isInCart = (productId: string): boolean => {
    return cart.items.some(item => item.product.id === productId);
  };

  const contextValue: CartContextType = {
    cart,
    addItem,
    removeItem,
    updateQuantity,
    clearCart,
    getItemQuantity,
    isInCart,
    hasItems: cart.items.length > 0
  };

  return (
    <CartContext.Provider value={contextValue}>
      {children}
    </CartContext.Provider>
  );
};

// Custom Hook to use Cart Context
export const useCart = (): CartContextType => {
  const context = useContext(CartContext);
  if (context === undefined) {
    throw new Error('useCart must be used within a CartProvider');
  }
  return context;
};

export default CartContext;