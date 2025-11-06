import { useSyncExternalStore } from "react";
import { getCurrentUser, isAuthenticated } from "@/lib/auth";
import type { UserDto } from "@/types/user";

// Cache to avoid creating new objects on every render
let cachedUser: UserDto | null = null;
let cachedUserJson: string | null = null;
let cachedIsAuthenticated = false;

// Listeners for auth state changes
const listeners = new Set<() => void>();

function subscribe(callback: () => void) {
  listeners.add(callback);
  return () => {
    listeners.delete(callback);
  };
}

function notifyListeners() {
  listeners.forEach((listener) => listener());
}

function getUserSnapshot(): UserDto | null {
  const user = getCurrentUser();
  const userJson = JSON.stringify(user);

  // Return cached user if the data hasn't changed
  if (userJson === cachedUserJson) {
    return cachedUser;
  }

  // Update cache
  cachedUser = user;
  cachedUserJson = userJson;
  return user;
}

function getAuthSnapshot(): boolean {
  const authenticated = isAuthenticated();

  // Update cache if changed
  if (authenticated !== cachedIsAuthenticated) {
    cachedIsAuthenticated = authenticated;
  }

  return authenticated;
}

function getServerSnapshot(): null {
  return null; // Always return null during SSR
}

function getServerAuthSnapshot(): boolean {
  return false; // Always return false during SSR
}

/**
 * Hook to get the current authenticated user
 * Returns null during SSR and if user is not authenticated
 */
export function useCurrentUser(): UserDto | null {
  return useSyncExternalStore(subscribe, getUserSnapshot, getServerSnapshot);
}

/**
 * Hook to check if user is authenticated
 * Returns false during SSR
 */
export function useIsAuthenticated(): boolean {
  return useSyncExternalStore(subscribe, getAuthSnapshot, getServerAuthSnapshot);
}

/**
 * Notify all subscribers that auth state has changed
 * Call this after login/logout to update all components
 */
export function notifyAuthChange(): void {
  // Clear cache to force re-evaluation
  cachedUser = null;
  cachedUserJson = null;
  cachedIsAuthenticated = false;
  notifyListeners();
}
