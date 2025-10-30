import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus, Calendar } from "lucide-react";
import type { DayWithPlacesDto } from "@/types/trips";
import { PlacesList } from "./PlacesList";

interface DayViewProps {
  day: DayWithPlacesDto | null;
  selectedDate?: Date;
  onAddDay?: (date: Date) => void;
  isLoading?: boolean;
}

const DayView: React.FC<DayViewProps> = ({ day, selectedDate, onAddDay, isLoading = false }) => {
  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <div className="h-6 bg-gray-200 rounded animate-pulse" />
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!day) {
    // If no day exists but a date is selected, show option to add the day
    if (selectedDate) {
      const formattedDate = selectedDate.toLocaleDateString("en-US", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
      });

      return (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">{formattedDate}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex flex-col items-center justify-center py-12">
              <Calendar className="h-12 w-12 text-gray-400 mb-4" />
              <h3 className="text-lg font-medium text-gray-900 mb-2">No trip day yet</h3>
              <p className="text-gray-500 text-center mb-4">
                Add this day to your trip itinerary to start planning activities and places to visit.
              </p>
              <Button onClick={() => onAddDay?.(selectedDate)} className="flex items-center gap-2">
                <Plus className="h-4 w-4" />
                Add Day
              </Button>
            </div>
          </CardContent>
        </Card>
      );
    }

    return (
      <Card>
        <CardContent className="flex items-center justify-center h-64">
          <p className="text-gray-500">Select a day to view details</p>
        </CardContent>
      </Card>
    );
  }

  const formattedDate = new Date(day.date).toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg">{formattedDate}</CardTitle>
        {day.notes && <p className="text-sm text-gray-600">{day.notes}</p>}
      </CardHeader>
      <CardContent>
        <PlacesList
          dayId={day.id}
          places={day.places}
          onPlacesChange={() => {
            // This will be handled by the parent component
            // to trigger a refetch of days data
          }}
        />
      </CardContent>
    </Card>
  );
};

export default DayView;
