import { useDashboardData } from "@/hooks/useDashboardData";
import { DashboardSkeleton } from "./dashboard/DashboardSkeleton";
import { DashboardHeader } from "./dashboard/DashboardHeader";
import { TripTabs } from "./dashboard/TripTabs";
import { ErrorDisplay } from "@/components/common/ErrorDisplay";

/**
 * Main dashboard view component
 * Displays user's trips categorized into Ongoing and Past
 */
export function DashboardView() {
  const { ongoingTrips, pastTrips, totalTripCount, tripLimit, isLoading, error } = useDashboardData();

  // Show loading skeleton during initial data fetch
  if (isLoading) {
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
