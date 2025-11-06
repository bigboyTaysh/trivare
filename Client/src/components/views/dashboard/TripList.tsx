import { TripCard } from "./TripCard";
import { EmptyState } from "./EmptyState";
import type { TripListDto } from "@/types/trips";

interface TripListProps {
  trips: TripListDto[];
  category: "Ongoing" | "Past";
}

/**
 * Renders a grid of trip cards or an empty state
 */
export function TripList({ trips, category }: TripListProps) {
  if (trips.length === 0) {
    return <EmptyState category={category} />;
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {trips.map((trip) => (
        <TripCard key={trip.id} trip={trip} />
      ))}
    </div>
  );
}
