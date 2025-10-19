import { useEffect, useState } from "react";
import { isAuthenticated } from "@/lib/auth";

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
  const [isChecking, setIsChecking] = useState(true);
  const [isAuthed, setIsAuthed] = useState(false);

  useEffect(() => {
    const checkAuth = () => {
      const authed = isAuthenticated();
      setIsAuthed(authed);
      setIsChecking(false);

      if (!authed) {
        // Store the current path to redirect back after login
        const currentPath = window.location.pathname;
        if (currentPath !== "/login") {
          sessionStorage.setItem("redirectAfterLogin", currentPath);
        }
        window.location.href = redirectTo;
      }
    };

    checkAuth();
  }, [redirectTo]);

  // Show fallback while checking authentication
  if (isChecking) {
    return <>{fallback}</>;
  }

  // Only render children if authenticated
  return isAuthed ? <>{children}</> : null;
}
