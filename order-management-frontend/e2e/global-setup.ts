import { chromium, FullConfig } from '@playwright/test';

async function globalSetup(config: FullConfig) {
  console.log('🚀 Starting E2E Test Suite Setup');
  
  // Launch browser for setup operations
  const browser = await chromium.launch();
  const page = await browser.newPage();
  
  try {
    // Wait for the application to be ready
    console.log('⏳ Waiting for application to be ready...');
    await page.goto(config.webServer?.url || 'http://localhost:5173');
    
    // Check if the app loads correctly
    await page.waitForSelector('h1:has-text("Order Management System")', { timeout: 30000 });
    console.log('✅ Application is ready');
    
    // Optional: Create test user if needed (commented out for now)
    // await setupTestUser(page);
    
    // Optional: Setup test products if needed (commented out for now)  
    // await setupTestData(page);
    
  } catch (error) {
    console.error('❌ Global setup failed:', error);
    throw error;
  } finally {
    await browser.close();
  }
  
  console.log('✅ E2E Test Suite Setup Complete');
}

async function setupTestUser(page: any) {
  try {
    // Navigate to register page
    await page.goto('/register');
    
    // Check if test user already exists by attempting login first
    await page.goto('/login');
    await page.fill('input[name="email"]', 'e2e.test@example.com');
    await page.fill('input[name="password"]', 'TestPass123!');
    
    const loginButton = page.locator('button[type="submit"]:has-text("Login")');
    await loginButton.click();
    
    // If login succeeds, user exists
    try {
      await page.waitForURL('/products', { timeout: 5000 });
      console.log('✅ Test user already exists');
      
      // Logout for clean state
      await page.click('button[title="Cerrar sesión"], button:has(svg)');
      await page.waitForURL('/');
      return;
    } catch {
      // User doesn't exist, create it
      console.log('📝 Creating test user...');
    }
    
    // Navigate to register and create test user
    await page.goto('/register');
    await page.fill('input[name="firstName"]', 'E2E');
    await page.fill('input[name="lastName"]', 'TestUser');
    await page.fill('input[name="email"]', 'e2e.test@example.com');
    await page.fill('input[name="password"]', 'TestPass123!');
    await page.fill('input[name="confirmPassword"]', 'TestPass123!');
    
    const registerButton = page.locator('button[type="submit"]:has-text("Register")');
    await registerButton.click();
    
    // Wait for successful registration
    await page.waitForURL('/products', { timeout: 10000 });
    console.log('✅ Test user created successfully');
    
    // Logout for clean state
    await page.click('button[title="Cerrar sesión"], button:has(svg)');
    await page.waitForURL('/');
    
  } catch (error) {
    console.log('⚠️ Test user setup failed (may already exist):', error.message);
  }
}

async function setupTestData(page: any) {
  try {
    console.log('📦 Checking test data availability...');
    
    // Login as test user to check products
    await page.goto('/login');
    await page.fill('input[name="email"]', 'e2e.test@example.com');
    await page.fill('input[name="password"]', 'TestPass123!');
    await page.click('button[type="submit"]:has-text("Login")');
    await page.waitForURL('/products');
    
    // Check if products are available
    const productCards = await page.locator('[data-testid="product-card"], .MuiCard-root').count();
    if (productCards > 0) {
      console.log(`✅ Found ${productCards} products available for testing`);
    } else {
      console.log('⚠️ No products found - tests may need backend data');
    }
    
    // Logout
    await page.click('button[title="Cerrar sesión"], button:has(svg)');
    await page.waitForURL('/');
    
  } catch (error) {
    console.log('⚠️ Test data check failed:', error.message);
  }
}

export default globalSetup;