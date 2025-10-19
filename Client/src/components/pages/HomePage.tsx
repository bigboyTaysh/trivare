import { ProtectedRoute } from "@/components/auth/ProtectedRoute";
import { LoadingSpinner } from "@/components/common/LoadingSpinner";
import DataFetcher from "@/components/DataFetcher";

export function HomePage() {
  return (
    <ProtectedRoute fallback={<LoadingSpinner />}>
      <main className="container mx-auto p-8">
        <h1 className="text-4xl font-bold mb-8">Welcome to Trivare</h1>
        <DataFetcher />
      </main>
    </ProtectedRoute>
  );
}
