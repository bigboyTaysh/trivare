import { TripCounter } from "./TripCounter";
import { CreateTripButton } from "./CreateTripButton";

interface DashboardHeaderProps {
  totalTripCount: number;
  tripLimit: number;
}

/**
 * Header section of the dashboard containing title, trip counter, and create button
 */
export function DashboardHeader({ totalTripCount, tripLimit }: DashboardHeaderProps) {
  return (
    <div className="mb-8">
      <h1 className="text-3xl font-bold mb-4">My Trips</h1>
      <div className="flex items-center justify-between">
        <TripCounter totalTripCount={totalTripCount} tripLimit={tripLimit} />
        <CreateTripButton totalTripCount={totalTripCount} tripLimit={tripLimit} />
      </div>
    </div>
  );
}
