import { useEffect } from "react";
import { useIsAuthenticated } from "@/hooks/useAuth";

interface ProtectedRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
  fallback?: React.ReactNode;
}

/**
 * Component that protects routes by checking authentication status.
 * Redirects to login if user is not authenticated.
 */
export function ProtectedRoute({ children, redirectTo = "/login", fallback = null }: ProtectedRouteProps) {
  const isAuthed = useIsAuthenticated();

  useEffect(() => {
    if (!isAuthed) {
      // Store the current path to redirect back after login
      const currentPath = window.location.pathname;
      if (currentPath !== "/login") {
        sessionStorage.setItem("redirectAfterLogin", currentPath);
      }
      window.location.href = redirectTo;
    }
  }, [isAuthed, redirectTo]);

  // Show fallback while not authenticated (before redirect happens)
  if (!isAuthed) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
