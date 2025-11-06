import { Button } from "@/components/ui/button";

interface CreateTripButtonProps {
  totalTripCount: number;
  tripLimit: number;
}

/**
 * Button that navigates to trip creation page
 * Disabled when user has reached their trip limit
 */
export function CreateTripButton({ totalTripCount, tripLimit }: CreateTripButtonProps) {
  const isLimitReached = totalTripCount >= tripLimit;

  const handleClick = () => {
    if (!isLimitReached) {
      window.location.href = "/trips/create";
    }
  };

  return (
    <Button
      onClick={handleClick}
      disabled={isLimitReached}
      title={isLimitReached ? `You have reached the maximum of ${tripLimit} trips` : "Create a new trip"}
    >
      Create New Trip
    </Button>
  );
}
