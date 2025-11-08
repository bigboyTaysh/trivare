import { useEffect } from "react";
import { useDashboardData } from "@/hooks/useDashboardData";
import { isAuthenticated } from "@/lib/auth";
import { DashboardSkeleton } from "./dashboard/DashboardSkeleton";
import { DashboardHeader } from "./dashboard/DashboardHeader";
import { TripTabs } from "./dashboard/TripTabs";
import { ErrorDisplay } from "@/components/common/ErrorDisplay";

/**
 * Main dashboard view component
 * Displays user's trips categorized into Ongoing and Past
 */
export function DashboardView() {
  // Check authentication immediately during render
  const authenticated = isAuthenticated();
  const { ongoingTrips, pastTrips, totalTripCount, tripLimit, isLoading, error } = useDashboardData();

  // Redirect to login if not authenticated
  useEffect(() => {
    if (!authenticated) {
      window.location.href = "/login";
    }
  }, [authenticated]);

  // Show loading skeleton while checking auth or during initial data fetch
  if (!authenticated || isLoading) {
    return <DashboardSkeleton />;
  }

  // Show error message if data fetch failed
  if (error) {
    return <ErrorDisplay message={error.message} />;
  }

  // Render main dashboard layout
  return (
    <div className="container mx-auto p-6">
      <DashboardHeader totalTripCount={totalTripCount} tripLimit={tripLimit} />
      <TripTabs ongoingTrips={ongoingTrips} pastTrips={pastTrips} />
    </div>
  );
}
