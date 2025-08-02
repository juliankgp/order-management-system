import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './BasePage';

export class LoginPage extends BasePage {
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly loginButton: Locator;
  readonly registerLink: Locator;
  readonly forgotPasswordLink: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    super(page);
    this.emailInput = page.locator('input[name="email"]');
    this.passwordInput = page.locator('input[name="password"]');
    this.loginButton = page.locator('button[type="submit"]:has-text("Sign In")');
    this.registerLink = page.locator('button:has-text("Sign up here")');
    this.errorMessage = page.locator('.MuiAlert-root, [role="alert"]');
  }

  async goto() {
    await super.goto('/login');
    await this.waitForPageLoad();
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
  }

  async loginWithTestUser() {
    await this.login('e2e.test@example.com', 'TestPass123!');
    await this.page.waitForURL('/products', { timeout: 10000 });
  }

  async expectLoginForm() {
    await expect(this.emailInput).toBeVisible();
    await expect(this.passwordInput).toBeVisible();
    await expect(this.loginButton).toBeVisible();
  }

  async expectErrorMessage(message?: string) {
    await expect(this.errorMessage).toBeVisible();
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }

  async goToRegister() {
    await this.registerLink.click();
    await this.page.waitForURL('/register');
  }

  async expectRedirectAfterLogin(expectedUrl: string = '/products') {
    await this.page.waitForURL(expectedUrl, { timeout: 10000 });
  }
}