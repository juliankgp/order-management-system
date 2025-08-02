import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './BasePage';

export class ProductsPage extends BasePage {
  readonly searchInput: Locator;
  readonly categoryFilter: Locator;
  readonly productCards: Locator;
  readonly cartIcon: Locator;
  readonly cartBadge: Locator;
  readonly addToCartButtons: Locator;
  readonly productDetailModal: Locator;

  constructor(page: Page) {
    super(page);
    this.searchInput = page.locator('input[placeholder*="Search"], input[placeholder*="search"]');
    this.categoryFilter = page.locator('select, .MuiSelect-root');
    this.productCards = page.locator('[data-testid="product-card"], .MuiCard-root');
    this.cartIcon = page.locator('button:has(svg):has-text(""), button[aria-label*="cart"]');
    this.cartBadge = page.locator('.MuiBadge-badge');
    this.addToCartButtons = page.locator('button:has-text("Add to Cart"), button:has-text("Agregar")');
    this.productDetailModal = page.locator('.MuiDialog-root, .MuiModal-root');
  }

  async goto() {
    await super.goto('/products');
    await this.waitForPageLoad();
    await this.waitForLoading();
  }

  async searchProducts(query: string) {
    await this.searchInput.fill(query);
    // Wait for debounced search
    await this.page.waitForTimeout(1000);
    await this.waitForLoading();
  }

  async expectProductsLoaded() {
    await expect(this.productCards.first()).toBeVisible({ timeout: 15000 });
    const count = await this.productCards.count();
    expect(count).toBeGreaterThan(0);
  }

  async getProductCount(): Promise<number> {
    await this.expectProductsLoaded();
    return await this.productCards.count();
  }

  async addFirstProductToCart() {
    await this.expectProductsLoaded();
    const firstProduct = this.productCards.first();
    const addButton = firstProduct.locator('button:has-text("Add to Cart"), button:has-text("Agregar")');
    await addButton.click();
  }

  async addProductToCartByIndex(index: number) {
    await this.expectProductsLoaded();
    const product = this.productCards.nth(index);
    const addButton = product.locator('button:has-text("Add to Cart"), button:has-text("Agregar")');
    await addButton.click();
  }

  async clickProductByIndex(index: number) {
    await this.expectProductsLoaded();
    const product = this.productCards.nth(index);
    await product.click();
  }

  async expectCartBadge(count: number) {
    if (count > 0) {
      await expect(this.cartBadge).toBeVisible();
      await expect(this.cartBadge).toContainText(count.toString());
    } else {
      await expect(this.cartBadge).toBeHidden();
    }
  }

  async openCart() {
    await this.cartIcon.click();
  }

  async expectProductDetailModal() {
    await expect(this.productDetailModal).toBeVisible();
  }

  async closeProductDetailModal() {
    await this.page.keyboard.press('Escape');
    await expect(this.productDetailModal).toBeHidden();
  }

  async expectSearchResults(query: string) {
    await this.expectProductsLoaded();
    // Check if at least one product contains the search query
    const firstProductText = await this.productCards.first().textContent();
    expect(firstProductText?.toLowerCase()).toContain(query.toLowerCase());
  }

  async clearSearch() {
    await this.searchInput.clear();
    await this.page.waitForTimeout(1000);
    await this.waitForLoading();
  }

  async expectEmptyResults() {
    await expect(this.page.locator('text=No products found, text=No hay productos')).toBeVisible();
  }
}