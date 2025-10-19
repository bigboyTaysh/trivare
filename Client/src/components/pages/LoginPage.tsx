import { PublicRoute } from "@/components/auth/PublicRoute";
import { LoginView } from "@/components/views/LoginView";

export function LoginPage() {
  return (
    <PublicRoute redirectIfAuthenticated={true}>
      <LoginView />
    </PublicRoute>
  );
}
