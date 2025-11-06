import { test, expect } from "@playwright/test";

// Test constants - avoid hardcoding sensitive data
const TEST_PASSWORD = "Password123!";

test.describe("Authentication", () => {
  test("should allow user to login with valid credentials", async ({ page }) => {
    // Generate unique email to avoid conflicts
    const uniqueId = Date.now();
    const email = `test${uniqueId}@example.com`;
    const username = `testuser${uniqueId}`;

    // First register a test user
    await page.goto("/register");
    await page.fill('input[name="userName"]', username);
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', TEST_PASSWORD);
    await page.fill('input[name="confirmPassword"]', TEST_PASSWORD);
    await page.click('button[type="submit"]');

    // Wait for successful registration toast and redirect to login
    await page.waitForURL("/login", { timeout: 10000 });

    // Now login with the registered user
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', TEST_PASSWORD);
    await page.click('button[type="submit"]');

    // Wait for navigation to dashboard
    await page.waitForURL("/", { timeout: 10000 });

    // Verify we're logged in - check for dashboard content
    await expect(page.locator("text=My Trips")).toBeVisible();
  });

  test("should show error message for invalid credentials", async ({ page }) => {
    // Navigate to login page
    await page.goto("/login");

    // Fill in invalid credentials
    await page.fill('input[type="email"]', "invalid@example.com");
    await page.fill('input[type="password"]', "wrongpassword");

    // Click login button
    await page.click('button[type="submit"]');

    // Verify error message appears
    await expect(page.locator("text=Invalid email or password")).toBeVisible();
  });

  test("should redirect unauthenticated users to login", async ({ page }) => {
    // Try to access protected route
    await page.goto("/profile");

    // Should be redirected to login
    await page.waitForURL("/login");
    expect(page.url()).toContain("/login");
  });

  test("should allow user to logout", async ({ page }) => {
    // Generate unique email to avoid conflicts
    const uniqueId = Date.now() + 1; // Add 1 to ensure different from login test
    const email = `test${uniqueId}@example.com`;
    const username = `testuser${uniqueId}`;

    // First register a test user
    await page.goto("/register");
    await page.fill('input[name="userName"]', username);
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', TEST_PASSWORD);
    await page.fill('input[name="confirmPassword"]', TEST_PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL("/login", { timeout: 10000 });

    // Now login
    await page.fill('input[type="email"]', email);
    await page.fill('input[type="password"]', TEST_PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL("/", { timeout: 10000 });

    // Click user menu button to open dropdown
    await page.click('[data-testid="user-menu"]');

    // Wait for logout option to appear and click it
    await page.waitForSelector("text=Log out", { timeout: 5000 });
    await page.click("text=Log out");

    // Should be redirected to login
    await page.waitForURL("/login", { timeout: 10000 });
    expect(page.url()).toContain("/login");
  });
});
