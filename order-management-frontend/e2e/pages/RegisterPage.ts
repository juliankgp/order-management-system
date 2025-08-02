import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './BasePage';

export class RegisterPage extends BasePage {
  readonly firstNameInput: Locator;
  readonly lastNameInput: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly confirmPasswordInput: Locator;
  readonly registerButton: Locator;
  readonly loginLink: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.firstNameInput = page.locator('input[name="firstName"]');
    this.lastNameInput = page.locator('input[name="lastName"]');
    this.emailInput = page.locator('input[name="email"]');
    this.passwordInput = page.locator('input[name="password"]');
    this.confirmPasswordInput = page.locator('input[name="confirmPassword"]');
    this.registerButton = page.locator('button[type="submit"]:has-text("Crear Cuenta")');
    this.loginLink = page.locator('button:has-text("Inicia sesión aquí")');
    this.errorMessage = page.locator('.MuiAlert-root, [role="alert"]');
  }

  async goto() {
    await super.goto('/register');
    await this.waitForPageLoad();
  }

  async register(firstName: string, lastName: string, email: string, password: string, confirmPassword: string) {
    await this.firstNameInput.fill(firstName);
    await this.lastNameInput.fill(lastName);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.confirmPasswordInput.fill(confirmPassword);
    await this.registerButton.click();
  }

  async registerTestUser() {
    await this.register('E2E', 'TestUser', 'e2e.test@example.com', 'TestPass123!', 'TestPass123!');
    await this.page.waitForURL('/products', { timeout: 10000 });
  }

  async expectRegisterForm() {
    await expect(this.firstNameInput).toBeVisible();
    await expect(this.lastNameInput).toBeVisible();
    await expect(this.emailInput).toBeVisible();
    await expect(this.passwordInput).toBeVisible();
    await expect(this.confirmPasswordInput).toBeVisible();
    await expect(this.registerButton).toBeVisible();
  }

  async expectErrorMessage(message?: string) {
    await expect(this.errorMessage).toBeVisible();
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }

  async goToLogin() {
    await this.loginLink.click();
    await this.page.waitForURL('/login');
  }

  async expectRedirectAfterRegister(expectedUrl: string = '/products') {
    await this.page.waitForURL(expectedUrl, { timeout: 10000 });
  }
}