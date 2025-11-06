import { PublicRoute } from "@/components/auth/PublicRoute";
import { RegisterView } from "@/components/views/RegisterView";

export function RegisterPage() {
  return (
    <PublicRoute redirectIfAuthenticated={true}>
      <RegisterView />
    </PublicRoute>
  );
}
