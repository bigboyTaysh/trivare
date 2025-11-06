import { useEffect } from "react";
import { useIsAuthenticated } from "@/hooks/useAuth";

interface PublicRouteProps {
  children: React.ReactNode;
  redirectTo?: string;
  redirectIfAuthenticated?: boolean;
  fallback?: React.ReactNode;
}

/**
 * Component for public routes that can optionally redirect authenticated users.
 * Useful for login/register pages that should redirect to dashboard if user is already logged in.
 */
export function PublicRoute({
  children,
  redirectTo = "/",
  redirectIfAuthenticated = true,
  fallback = null,
}: PublicRouteProps) {
  const isAuthed = useIsAuthenticated();

  useEffect(() => {
    if (isAuthed && redirectIfAuthenticated) {
      window.location.href = redirectTo;
    }
  }, [isAuthed, redirectTo, redirectIfAuthenticated]);

  // Show fallback if authenticated and about to redirect
  if (isAuthed && redirectIfAuthenticated) {
    return <>{fallback}</>;
  }

  return <>{children}</>;
}
