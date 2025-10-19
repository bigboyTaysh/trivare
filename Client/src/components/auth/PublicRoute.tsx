import { useEffect, useState } from "react";
import { isAuthenticated } from "@/lib/auth";

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
  const [isChecking, setIsChecking] = useState(true);
  const [shouldRender, setShouldRender] = useState(false);

  useEffect(() => {
    const checkAuth = () => {
      const authed = isAuthenticated();
      setIsChecking(false);

      if (authed && redirectIfAuthenticated) {
        window.location.href = redirectTo;
        return;
      }

      setShouldRender(true);
    };

    checkAuth();
  }, [redirectTo, redirectIfAuthenticated]);

  // Show fallback while checking authentication
  if (isChecking) {
    return <>{fallback}</>;
  }

  // Render children if should render
  return shouldRender ? <>{children}</> : null;
}
