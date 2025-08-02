export const TEST_USERS = {
  valid: {
    email: 'e2e.test@example.com',
    password: 'TestPass123!',
    firstName: 'E2E',
    lastName: 'TestUser',
    fullName: 'E2E TestUser'
  },
  invalid: {
    email: 'invalid@email.com',
    password: 'wrongpassword'
  },
  newUser: {
    email: 'new.user@example.com',
    password: 'NewPass123!',
    firstName: 'New',
    lastName: 'User'
  }
};

export const TEST_PRODUCTS = {
  searchTerms: [
    'Laptop',
    'Dell',
    'iPhone',
    'Mesa'
  ],
  categories: [
    'Electronics',
    'Furniture'
  ]
};

export const TEST_URLS = {
  home: '/',
  login: '/login',
  register: '/register',
  products: '/products',
  orders: '/orders',
  checkout: '/checkout',
  notFound: '/non-existent-page'
};

export const SELECTORS = {
  // Authentication
  emailInput: 'input[name="email"]',
  passwordInput: 'input[name="password"]',
  loginButton: 'button[type="submit"]:has-text("Login")',
  logoutButton: 'button[title="Cerrar sesión"], button:has(svg)',
  
  // Products
  productCards: '[data-testid="product-card"], .MuiCard-root',
  searchInput: 'input[placeholder*="Search"], input[placeholder*="search"]',
  addToCartButton: 'button:has-text("Add to Cart"), button:has-text("Agregar")',
  
  // Cart
  cartIcon: 'button:has(svg):has-text(""), button[aria-label*="cart"]',
  cartBadge: '.MuiBadge-badge',
  cartDrawer: '.MuiDrawer-root',
  
  // Navigation
  productsLink: 'a:has-text("Products")',
  ordersLink: 'a:has-text("Orders"), a:has-text("My Orders")',
  homeLink: 'a:has-text("Home"), .MuiTypography-root:has-text("Order Management")',
  
  // Common
  loadingSpinner: '.MuiCircularProgress-root',
  errorAlert: '.MuiAlert-root, [role="alert"]',
  modal: '.MuiDialog-root, .MuiModal-root',
  notification: '.MuiSnackbar-root'
};

export const TIMEOUTS = {
  short: 2000,
  medium: 5000,
  long: 10000,
  pageLoad: 15000
};

export const VIEWPORT_SIZES = {
  mobile: { width: 375, height: 667 },
  tablet: { width: 768, height: 1024 },
  desktop: { width: 1280, height: 720 },
  largeDesktop: { width: 1920, height: 1080 }
};