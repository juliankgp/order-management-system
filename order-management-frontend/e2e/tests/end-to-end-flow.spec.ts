import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductsPage } from '../pages/ProductsPage';

test.describe('Complete E2E User Journey', () => {
  let loginPage: LoginPage;
  let productsPage: ProductsPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    productsPage = new ProductsPage(page);
  });

  test('Complete purchase flow: Login → Browse → Add to Cart → Checkout', async ({ page }) => {
    // Step 1: Login
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    
    // Step 2: Browse Products
    await productsPage.goto();
    await productsPage.expectProductsLoaded();
    
    // Step 3: Search for specific product
    await productsPage.searchProducts('Laptop');
    await page.waitForTimeout(1000);
    
    // Step 4: Add products to cart
    const availableProducts = await productsPage.getProductCount();
    if (availableProducts > 0) {
      await productsPage.addFirstProductToCart();
      await page.waitForTimeout(1000);
      await productsPage.expectCartBadge(1);
      
      // Add another product if available
      if (availableProducts > 1) {
        await productsPage.addProductToCartByIndex(1);
        await page.waitForTimeout(1000);
        await productsPage.expectCartBadge(2);
      }
    }
    
    // Step 5: Open cart and review
    await productsPage.openCart();
    await expect(page.locator('.MuiDrawer-root, .MuiDialog-root')).toBeVisible();
    
    // Step 6: Proceed to checkout (if cart has items)
    const checkoutButton = page.locator('button:has-text("Checkout"), button:has-text("Proceed")');
    const checkoutExists = await checkoutButton.isVisible();
    
    if (checkoutExists) {
      await checkoutButton.click();
      await page.waitForURL('/checkout', { timeout: 10000 });
      
      // Should be on checkout page
      await expect(page.locator('h1, h2, h3, h4')).toContainText(['Checkout', 'Order Review', 'Review']);
    }
    
    // Step 7: Verify order history is accessible
    await page.click('a:has-text("Orders"), a:has-text("My Orders")');
    await page.waitForURL('/orders');
    
    // Should show orders page
    await expect(page.locator('h1, h2, h3, h4')).toContainText(['Orders', 'My Orders']);
  });

  test('Navigation flow: Home → Login → Products → Orders → Profile', async ({ page }) => {
    // Start from home
    await page.goto('/');
    await expect(page.locator('h1, h2, h3:has-text("Order Management")')).toBeVisible();
    
    // Login
    await page.click('button:has-text("Login"), a:has-text("Login")');
    await page.waitForURL('/login');
    await loginPage.loginWithTestUser();
    
    // Should be on products after login
    await page.waitForURL('/products');
    await productsPage.expectProductsLoaded();
    
    // Navigate to orders
    await page.click('a:has-text("Orders"), a:has-text("My Orders")');
    await page.waitForURL('/orders');
    
    // Back to products
    await page.click('a:has-text("Products")');
    await page.waitForURL('/products');
    
    // Test dashboard/home while logged in
    await page.click('a:has-text("Home"), .MuiTypography-root:has-text("Order Management")');
    await page.waitForURL('/');
    
    // Should show dashboard for logged in user
    await expect(page.locator('text=Welcome back, text=Quick Actions')).toBeVisible();
  });

  test('Error handling and recovery', async ({ page }) => {
    // Test 404 page
    await page.goto('/non-existent-page');
    await expect(page.locator('h1:has-text("404"), text=Page Not Found')).toBeVisible();
    
    // Should have navigation options
    await expect(page.locator('button:has-text("Home"), button:has-text("Go Home")')).toBeVisible();
    
    // Go home from 404
    await page.click('button:has-text("Home"), button:has-text("Go Home")');
    await page.waitForURL('/');
    
    // Test protected route without auth
    await page.goto('/products');
    await page.waitForURL('/login');
    
    // Login and should redirect back
    await loginPage.loginWithTestUser();
    await page.waitForURL('/products');
  });

  test('Responsive design and mobile navigation', async ({ page, browserName }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    await productsPage.goto();
    
    // Products should be visible on mobile
    await productsPage.expectProductsLoaded();
    
    // Cart should work on mobile
    await productsPage.addFirstProductToCart();
    await page.waitForTimeout(1000);
    await productsPage.expectCartBadge(1);
    
    // Open cart on mobile
    await productsPage.openCart();
    await expect(page.locator('.MuiDrawer-root, .MuiDialog-root')).toBeVisible();
  });

  test('Session persistence across page refresh', async ({ page }) => {
    // Login
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    await productsPage.goto();
    
    // Add item to cart
    await productsPage.expectProductsLoaded();
    await productsPage.addFirstProductToCart();
    await page.waitForTimeout(1000);
    
    // Refresh page
    await page.reload();
    
    // Should still be logged in
    await page.waitForURL('/products');
    await expect(page.locator('text=E2E, text=TestUser')).toBeVisible();
    
    // Cart should persist
    await productsPage.expectCartBadge(1);
  });

  test('Search and filter functionality', async ({ page }) => {
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    await productsPage.goto();
    
    await productsPage.expectProductsLoaded();
    const totalProducts = await productsPage.getProductCount();
    
    // Test search functionality
    await productsPage.searchProducts('Dell');
    await page.waitForTimeout(1500);
    
    const searchResults = await productsPage.getProductCount();
    expect(searchResults).toBeLessThanOrEqual(totalProducts);
    
    // Clear search
    await productsPage.clearSearch();
    await page.waitForTimeout(1000);
    
    const clearedResults = await productsPage.getProductCount();
    expect(clearedResults).toBe(totalProducts);
  });
});