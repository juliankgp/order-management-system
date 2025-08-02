# E2E Testing with Playwright

This directory contains end-to-end tests for the Order Management System frontend using Playwright.

## Setup

The E2E testing framework is already configured. Make sure you have:

1. Node.js 18+ installed
2. All npm dependencies installed (`npm install`)
3. Playwright browsers installed (`npx playwright install`)

## Running Tests

### Prerequisites

Before running E2E tests, ensure:

1. **Backend services are running:**
   ```bash
   # In the backend directory
   .\infra\scripts\start-services.ps1
   # OR manually start each service:
   # dotnet run --project services/CustomerService/src/Api --urls="https://localhost:5003"
   # dotnet run --project services/ProductService/src/Api --urls="https://localhost:5002"
   # dotnet run --project services/OrderService/src/Api --urls="https://localhost:5001"
   ```

2. **Database is set up with test data:**
   ```bash
   # In the backend directory
   .\infra\scripts\setup-databases.ps1
   ```

### Test Commands

```bash
# Run all E2E tests (headless)
npm run test:e2e

# Run tests with browser UI visible
npm run test:e2e:headed

# Run tests with Playwright UI (recommended for development)
npm run test:e2e:ui

# Debug tests (step by step)
npm run test:e2e:debug

# Show test results report
npm run test:e2e:report
```

### Running Specific Tests

```bash
# Run only authentication tests
npx playwright test auth.spec.ts

# Run only products tests
npx playwright test products.spec.ts

# Run only end-to-end flow tests
npx playwright test end-to-end-flow.spec.ts

# Run tests in specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

## Test Structure

### Directory Layout

```
e2e/
├── tests/           # Test specifications
│   ├── auth.spec.ts          # Authentication flow tests
│   ├── products.spec.ts      # Product management tests
│   └── end-to-end-flow.spec.ts # Complete user journey tests
├── pages/           # Page Object Models
│   ├── BasePage.ts           # Base page with common functionality
│   ├── LoginPage.ts          # Login page interactions
│   └── ProductsPage.ts       # Products page interactions
├── fixtures/        # Test data and utilities
│   └── test-data.ts          # Test users, products, selectors
├── global-setup.ts  # Global test setup (creates test user)
├── global-teardown.ts # Global test cleanup
└── README.md        # This file
```

### Test Categories

1. **Authentication Tests** (`auth.spec.ts`)
   - Login form validation
   - Successful login/logout
   - Error handling
   - Navigation redirects

2. **Products Tests** (`products.spec.ts`)
   - Product listing
   - Search functionality
   - Add to cart
   - Product details modal
   - Cart state management

3. **End-to-End Flow Tests** (`end-to-end-flow.spec.ts`)
   - Complete purchase journey
   - Navigation flow
   - Error handling and recovery
   - Responsive design
   - Session persistence

## Test User

The tests use a dedicated test user:
- **Email:** `e2e.test@example.com`
- **Password:** `TestPass123!`
- **Name:** E2E TestUser

This user is automatically created during global setup if it doesn't exist.

## Configuration

Test configuration is in `playwright.config.ts`:

- **Base URL:** `http://localhost:5174` (Vite dev server)
- **Browsers:** Chrome, Firefox, Safari, Mobile Chrome, Mobile Safari
- **Timeout:** 30 seconds per test
- **Retries:** 2 retries on CI, 0 on local
- **Video:** Recorded on failure
- **Screenshots:** Taken on failure
- **Traces:** Collected on retry

## Best Practices

### Writing Tests

1. **Use Page Object Models** - Keep selectors and actions in page classes
2. **Wait for Elements** - Use `expect()` and `waitFor()` methods
3. **Independent Tests** - Each test should be able to run in isolation
4. **Descriptive Names** - Test names should clearly describe what they test
5. **Clean State** - Ensure each test starts with a clean state

### Debugging Tests

1. **Use UI Mode** - `npm run test:e2e:ui` for visual debugging
2. **Use Headed Mode** - `npm run test:e2e:headed` to see browser
3. **Add Debug Statements** - Use `await page.pause()` to stop execution
4. **Check Screenshots** - Failed tests automatically capture screenshots
5. **Review Traces** - Use `npx playwright show-trace` for detailed debugging

### Common Issues

1. **Test Timeouts** - Increase timeout in test if operations are slow
2. **Element Not Found** - Use more specific selectors or wait for elements
3. **Race Conditions** - Add explicit waits for async operations
4. **Backend Dependencies** - Ensure backend services are running and healthy

## Continuous Integration

For CI environments:
- Tests run in headless mode
- Retry failed tests up to 2 times
- Generate HTML and JUnit reports
- Collect artifacts (videos, screenshots, traces)

## Maintenance

- **Update Selectors** - When UI changes, update page object selectors
- **Add New Tests** - Cover new features with appropriate E2E tests
- **Review Test Data** - Ensure test user and data remain valid
- **Monitor Performance** - Keep test execution time reasonable

## Troubleshooting

### Frontend Not Starting
```bash
# Check if port 5174 is available
netstat -ano | findstr :5174

# Manually start frontend
npm run dev
```

### Backend Services Not Running
```bash
# Check backend service status
curl -k https://localhost:5003/health
curl -k https://localhost:5002/api/products/health
curl -k https://localhost:5001/health
```

### Test User Issues
```bash
# Reset test environment by deleting test user from database
# Then run tests again to recreate the user
```

### Browser Issues
```bash
# Reinstall browsers
npx playwright install --force
```

For more detailed information, see the [Playwright documentation](https://playwright.dev/docs/intro).