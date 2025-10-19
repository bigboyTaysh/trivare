/**
 * Check if user is authenticated by verifying the presence of access token
 * @returns true if user has valid authentication token, false otherwise
 */
export function isAuthenticated(): boolean {
  if (typeof window === "undefined") {
    return false;
  }

  const accessToken = localStorage.getItem("accessToken");
  return !!accessToken;
}

/**
 * Get the current authenticated user from localStorage
 * @returns User object or null if not authenticated
 */
export function getCurrentUser() {
  if (typeof window === "undefined") {
    return null;
  }

  const userJson = localStorage.getItem("user");
  if (!userJson) {
    return null;
  }

  try {
    return JSON.parse(userJson);
  } catch {
    return null;
  }
}

/**
 * Clear authentication data from localStorage
 */
export function clearAuthData(): void {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");
}

/**
 * Redirect to login page if not authenticated
 */
export function requireAuth(): void {
  if (typeof window === "undefined") {
    return;
  }

  if (!isAuthenticated()) {
    window.location.href = "/login";
  }
}
