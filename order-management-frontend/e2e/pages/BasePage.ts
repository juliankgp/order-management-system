import { Page, Locator, expect } from '@playwright/test';

export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async goto(path: string = '/') {
    await this.page.goto(path);
    await this.waitForPageLoad();
  }

  async waitForPageLoad() {
    await this.page.waitForLoadState('networkidle');
  }

  async takeScreenshot(name: string) {
    await this.page.screenshot({ path: `test-results/screenshots/${name}.png` });
  }

  async expectUrl(url: string) {
    await expect(this.page).toHaveURL(url);
  }

  async expectTitle(title: string) {
    await expect(this.page).toHaveTitle(title);
  }

  async fillInput(selector: string, value: string) {
    await this.page.fill(selector, value);
  }

  async clickButton(selector: string) {
    await this.page.click(selector);
  }

  async waitForSelector(selector: string, timeout: number = 10000) {
    await this.page.waitForSelector(selector, { timeout });
  }

  async waitForText(text: string, timeout: number = 10000) {
    await this.page.waitForSelector(`text=${text}`, { timeout });
  }

  async expectElementVisible(selector: string) {
    await expect(this.page.locator(selector)).toBeVisible();
  }

  async expectElementHidden(selector: string) {
    await expect(this.page.locator(selector)).toBeHidden();
  }

  async expectTextContent(selector: string, text: string) {
    await expect(this.page.locator(selector)).toContainText(text);
  }

  // Navigation helpers
  async isUserLoggedIn(): Promise<boolean> {
    try {
      await this.page.waitForSelector('button[title="Cerrar sesión"], button:has(svg)', { timeout: 2000 });
      return true;
    } catch {
      return false;
    }
  }

  async logout() {
    if (await this.isUserLoggedIn()) {
      await this.page.click('button[title="Cerrar sesión"], button:has(svg)');
      await this.page.waitForURL('/');
    }
  }

  // Common UI interactions
  async dismissNotification() {
    try {
      await this.page.click('button[aria-label="close"], .MuiAlert-action button', { timeout: 2000 });
    } catch {
      // Notification might not be present
    }
  }

  async waitForLoading() {
    // Wait for any loading indicators to disappear
    try {
      await this.page.waitForSelector('.MuiCircularProgress-root', { state: 'detached', timeout: 10000 });
    } catch {
      // Loading indicator might not be present
    }
  }
}