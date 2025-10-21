import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import type { TripListDto } from "@/types/trips";

interface TripCardProps {
  trip: TripListDto;
}

/**
 * Card component displaying a summary of a single trip
 * The entire card is clickable and navigates to the trip detail page
 */
export function TripCard({ trip }: TripCardProps) {
  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      month: "short",
      day: "numeric",
      year: "numeric",
    });
  };

  const handleClick = () => {
    window.location.href = `/trips/${trip.id}`;
  };

  return (
    <Card className="cursor-pointer transition-all hover:shadow-lg hover:scale-[1.02]" onClick={handleClick}>
      <CardHeader>
        <CardTitle className="line-clamp-1">{trip.name}</CardTitle>
        {trip.destination && <CardDescription className="line-clamp-1">{trip.destination}</CardDescription>}
      </CardHeader>
      <CardContent>
        <div className="flex flex-col gap-2 text-sm text-muted-foreground">
          <div className="flex items-center gap-2">
            <span className="font-medium">Dates:</span>
            <span>
              {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
            </span>
          </div>
          {trip.notes && <p className="line-clamp-2 text-xs mt-2">{trip.notes}</p>}
        </div>
      </CardContent>
    </Card>
  );
}
