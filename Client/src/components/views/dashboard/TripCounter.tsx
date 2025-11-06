interface TripCounterProps {
  totalTripCount: number;
  tripLimit: number;
}

/**
 * Displays the current trip usage count vs the limit
 */
export function TripCounter({ totalTripCount, tripLimit }: TripCounterProps) {
  const isNearLimit = totalTripCount >= tripLimit * 0.8;
  const isAtLimit = totalTripCount >= tripLimit;

  return (
    <div className="flex items-center gap-2">
      <span className="text-sm text-muted-foreground">Trips:</span>
      <span
        className={`text-sm font-medium ${
          isAtLimit ? "text-destructive" : isNearLimit ? "text-yellow-600 dark:text-yellow-500" : "text-foreground"
        }`}
      >
        {totalTripCount}/{tripLimit}
      </span>
    </div>
  );
}
