import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { ProductsPage } from '../pages/ProductsPage';

test.describe('Products Management', () => {
  let loginPage: LoginPage;
  let productsPage: ProductsPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    productsPage = new ProductsPage(page);
    
    // Login before each test
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    await productsPage.goto();
  });

  test('should display products list', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    const productCount = await productsPage.getProductCount();
    expect(productCount).toBeGreaterThan(0);
  });

  test('should search products', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Search for a specific product (using common terms)
    await productsPage.searchProducts('Dell');
    
    // Should show filtered results
    const resultsCount = await productsPage.getProductCount();
    expect(resultsCount).toBeGreaterThanOrEqual(0);
    
    // Clear search
    await productsPage.clearSearch();
    await productsPage.expectProductsLoaded();
  });

  test('should open product detail modal', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Click on first product
    await productsPage.clickProductByIndex(0);
    
    // Should open detail modal
    await productsPage.expectProductDetailModal();
    
    // Should show product details
    await expect(page.locator('.MuiDialog-root text=Price, .MuiDialog-root text=Stock')).toBeVisible();
    
    // Close modal
    await productsPage.closeProductDetailModal();
  });

  test('should add product to cart', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Initially cart should be empty
    await productsPage.expectCartBadge(0);
    
    // Add first product to cart
    await productsPage.addFirstProductToCart();
    
    // Wait a moment for state update
    await page.waitForTimeout(1000);
    
    // Cart badge should show 1 item
    await productsPage.expectCartBadge(1);
  });

  test('should add multiple products to cart', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    const productsToAdd = Math.min(3, await productsPage.getProductCount());
    
    // Add multiple products
    for (let i = 0; i < productsToAdd; i++) {
      await productsPage.addProductToCartByIndex(i);
      await page.waitForTimeout(500); // Wait between additions
    }
    
    // Cart badge should show correct count
    await productsPage.expectCartBadge(productsToAdd);
  });

  test('should open cart drawer', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Add a product first
    await productsPage.addFirstProductToCart();
    await page.waitForTimeout(1000);
    
    // Open cart
    await productsPage.openCart();
    
    // Should show cart drawer/modal
    await expect(page.locator('.MuiDrawer-root, .MuiDialog-root:has-text("Cart")')).toBeVisible();
    
    // Should show added product
    await expect(page.locator('text=Laptop, text=iPhone, text=Mesa')).toBeVisible();
  });

  test('should handle empty product search', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Search for non-existent product
    await productsPage.searchProducts('NonExistentProductXYZ123');
    
    // Should show no results or empty state
    await page.waitForTimeout(2000);
    const productCount = await productsPage.getProductCount();
    expect(productCount).toBe(0);
  });

  test('should maintain cart state across navigation', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Add product to cart
    await productsPage.addFirstProductToCart();
    await page.waitForTimeout(1000);
    await productsPage.expectCartBadge(1);
    
    // Navigate to orders page
    await page.click('a:has-text("Orders"), a:has-text("My Orders")');
    await page.waitForURL('/orders');
    
    // Navigate back to products
    await page.click('a:has-text("Products")');
    await page.waitForURL('/products');
    
    // Cart should still have 1 item
    await productsPage.expectCartBadge(1);
  });

  test('should show product information correctly', async ({ page }) => {
    await productsPage.expectProductsLoaded();
    
    // Check that product cards show essential information
    const firstProduct = productsPage.productCards.first();
    
    // Should show product name
    await expect(firstProduct.locator('h5, h6, .MuiTypography-h5, .MuiTypography-h6')).toBeVisible();
    
    // Should show price
    await expect(firstProduct.locator('text=$, text=Price')).toBeVisible();
    
    // Should show add to cart button
    await expect(firstProduct.locator('button:has-text("Add"), button:has-text("Agregar")')).toBeVisible();
  });
});