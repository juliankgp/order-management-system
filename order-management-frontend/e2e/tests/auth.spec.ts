import { test, expect } from '@playwright/test';
import { LoginPage } from '../pages/LoginPage';
import { BasePage } from '../pages/BasePage';

test.describe('Authentication Flow', () => {
  let loginPage: LoginPage;
  let basePage: BasePage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    basePage = new BasePage(page);
    
    // Ensure user is logged out before each test
    await basePage.logout();
  });

  test('should display login form correctly', async ({ page }) => {
    await loginPage.goto();
    
    await loginPage.expectLoginForm();
    await expect(page).toHaveTitle(/Order Management/);
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await loginPage.goto();
    
    await loginPage.login('invalid@email.com', 'wrongpassword');
    
    await loginPage.expectErrorMessage();
  });

  test('should show error for empty fields', async ({ page }) => {
    await loginPage.goto();
    
    await loginPage.loginButton.click();
    
    // Should show validation errors
    await expect(page.locator('text=required, text=Required')).toBeVisible();
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    await loginPage.goto();
    
    await loginPage.loginWithTestUser();
    
    // Should redirect to products page
    await loginPage.expectRedirectAfterLogin('/products');
    
    // Should show user info in header
    await expect(page.locator('text=E2E TestUser, text=E2E')).toBeVisible();
  });

  test('should logout successfully', async ({ page }) => {
    // First login
    await loginPage.goto();
    await loginPage.loginWithTestUser();
    
    // Then logout
    await basePage.logout();
    
    // Should redirect to home page
    await expect(page).toHaveURL('/');
    
    // Should not show user info
    await expect(page.locator('text=Login')).toBeVisible();
  });

  test('should redirect to intended page after login', async ({ page }) => {
    // Try to access protected page without authentication
    await page.goto('/orders');
    
    // Should redirect to login
    await expect(page).toHaveURL('/login');
    
    // Login
    await loginPage.loginWithTestUser();
    
    // Should redirect back to originally requested page
    await expect(page).toHaveURL('/orders');
  });

  test('should navigate between login and register', async ({ page }) => {
    await loginPage.goto();
    
    // Go to register
    await loginPage.goToRegister();
    await expect(page).toHaveURL('/register');
    
    // Go back to login
    await page.click('a:has-text("Login"), a:has-text("Sign in")');
    await expect(page).toHaveURL('/login');
  });
});