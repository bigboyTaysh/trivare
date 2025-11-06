import { Card, CardHeader, CardTitle, CardDescription, CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { Badge } from "@/components/ui/badge";
import type { TripListDto } from "@/types/trips";
import { formatDate } from "@/lib/dateUtils";
import { Calendar, MapPin, Plane, FileText, Home, CheckCircle2 } from "lucide-react";

interface TripCardProps {
  trip: TripListDto;
}

/**
 * Card component displaying a summary of a single trip
 * The entire card is clickable and navigates to the trip detail page
 */
export function TripCard({ trip }: TripCardProps) {
  const handleClick = () => {
    window.location.href = `/trips/${trip.id}`;
  };

  const stats = trip.statistics;
  const isOngoing = stats?.progressPercentage !== undefined && stats.progressPercentage < 100;
  const isCompleted = stats?.progressPercentage === 100;

  return (
    <Card className="cursor-pointer transition-all hover:shadow-lg hover:scale-[1.02]" onClick={handleClick}>
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <CardTitle className="line-clamp-1 text-lg">{trip.name}</CardTitle>
            {trip.destination && (
              <CardDescription className="line-clamp-1 flex items-center gap-1 mt-1">
                <MapPin className="h-3 w-3" />
                {trip.destination}
              </CardDescription>
            )}
          </div>
          {isCompleted && (
            <Badge variant="secondary" className="ml-2 flex items-center gap-1">
              <CheckCircle2 className="h-3 w-3" />
              Completed
            </Badge>
          )}
        </div>
      </CardHeader>

      <CardContent className="space-y-4">
        {/* Dates */}
        <div className="flex items-center gap-2 text-sm text-muted-foreground">
          <Calendar className="h-4 w-4" />
          <span>
            {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
          </span>
          {stats && (
            <Badge variant="outline" className="ml-auto">
              {stats.durationInDays} day{stats.durationInDays !== 1 ? "s" : ""}
            </Badge>
          )}
        </div>

        {/* Progress bar for ongoing trips */}
        {isOngoing && stats && (
          <div className="space-y-2">
            <div className="flex items-center justify-between text-xs text-muted-foreground">
              <span>Trip Progress</span>
              <span>
                {stats.daysCompleted}/{stats.durationInDays} days
              </span>
            </div>
            <Progress value={stats.progressPercentage} className="h-2" />
          </div>
        )}

        {/* Statistics */}
        {stats && (
          <div className="grid grid-cols-2 gap-3 pt-2 border-t">
            <div className="flex items-center gap-2 text-sm">
              <Calendar className="h-4 w-4 text-blue-500" />
              <span className="text-muted-foreground">{stats.dayCount} days planned</span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <MapPin className="h-4 w-4 text-green-500" />
              <span className="text-muted-foreground">{stats.placeCount} places</span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <Plane className="h-4 w-4 text-purple-500" />
              <span className="text-muted-foreground">
                {stats.transportCount} transport{stats.transportCount !== 1 ? "s" : ""}
              </span>
            </div>

            <div className="flex items-center gap-2 text-sm">
              <FileText className="h-4 w-4 text-orange-500" />
              <span className="text-muted-foreground">
                {stats.fileCount} file{stats.fileCount !== 1 ? "s" : ""}
              </span>
            </div>
          </div>
        )}

        {/* Accommodation status */}
        {stats && (
          <div className="flex items-center gap-2 text-sm pt-2 border-t">
            <Home className={`h-4 w-4 ${stats.hasAccommodation ? "text-green-500" : "text-muted-foreground"}`} />
            <span className={stats.hasAccommodation ? "text-green-700" : "text-muted-foreground"}>
              {stats.hasAccommodation ? "Accommodation booked" : "No accommodation"}
            </span>
          </div>
        )}

        {/* Notes */}
        {trip.notes && <p className="line-clamp-2 text-xs text-muted-foreground mt-3 italic">{trip.notes}</p>}
      </CardContent>
    </Card>
  );
}
