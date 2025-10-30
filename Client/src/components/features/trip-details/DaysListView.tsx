import React from "react";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Plus, Calendar } from "lucide-react";
import type { DayWithPlacesDto } from "@/types/trips";
import { PlacesList } from "./PlacesList";

// No-op function to satisfy ESLint no-empty-function rule
const noop = () => {
  // Intentionally empty - used as fallback when no callback is provided
};

interface DaysListViewProps {
  days: DayWithPlacesDto[];
  isLoading?: boolean;
  onDaySelect?: (day: DayWithPlacesDto | null, date: Date | null) => void;
  onPlacesChange?: () => void;
  selectedDayId?: string;
  selectedDate?: Date;
  onAddDay?: (date?: Date) => void;
}

const DaysListView: React.FC<DaysListViewProps> = ({
  days,
  isLoading = false,
  onDaySelect,
  onPlacesChange,
  selectedDayId,
  selectedDate,
  onAddDay,
}) => {
  const sortedDays = [...days].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[...Array(3)].map((_, i) => (
          <Card key={i}>
            <CardHeader>
              <div className="h-6 bg-gray-200 rounded animate-pulse" />
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                <div className="h-4 bg-gray-200 rounded animate-pulse" />
                <div className="h-4 bg-gray-200 rounded animate-pulse" />
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    );
  }

  if (days.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-12">
          <Calendar className="h-12 w-12 text-gray-400 mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No trip days yet</h3>
          <p className="text-gray-500 text-center mb-4">Start planning your trip by adding days to your itinerary.</p>
          <Button onClick={() => onAddDay?.()} className="flex items-center gap-2">
            <Plus className="h-4 w-4" />
            Add First Day
          </Button>
        </CardContent>
      </Card>
    );
  }

  // Check if there's a selected date without a corresponding day
  const hasSelectedDateWithoutDay =
    selectedDate &&
    !selectedDayId &&
    !days.some((d) => new Date(d.date).toDateString() === selectedDate.toDateString());

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Trip Days ({days.length})</h2>
        <Button onClick={() => onAddDay?.()} size="sm" className="flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Add Day
        </Button>
      </div>

      {/* Show info card when a date without a day is selected */}
      {hasSelectedDateWithoutDay && (
        <Card className="border-blue-200 bg-blue-50">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="font-medium text-blue-900">
                  {selectedDate.toLocaleDateString("en-US", {
                    weekday: "long",
                    month: "long",
                    day: "numeric",
                  })}
                </h3>
                <p className="text-sm text-blue-700 mt-1">No itinerary created for this date yet</p>
              </div>
              <Button
                onClick={() => selectedDate && onAddDay?.(selectedDate)}
                size="sm"
                className="flex items-center gap-2"
              >
                <Plus className="h-4 w-4" />
                Add Day
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      <Accordion
        type="single"
        collapsible
        value={selectedDayId || undefined}
        onValueChange={(value) => {
          const selectedDay = value ? days.find((d) => d.id === value) || null : null;
          const date = selectedDay ? new Date(selectedDay.date) : null;
          onDaySelect?.(selectedDay, date);
        }}
        className="space-y-4"
      >
        {sortedDays.map((day) => {
          const formattedDate = new Date(day.date).toLocaleDateString("en-US", {
            weekday: "short",
            month: "short",
            day: "numeric",
          });
          const isSelected = selectedDayId === day.id;

          return (
            <AccordionItem key={day.id} value={day.id} className="border rounded-lg">
              <AccordionTrigger className="px-4 py-3 hover:no-underline">
                <div className="flex items-center justify-between w-full mr-4">
                  <div className="flex items-center gap-3">
                    <div>
                      <h3 className="font-medium text-left">{formattedDate}</h3>
                      {day.notes && <p className="text-sm text-gray-600 text-left truncate max-w-xs">{day.notes}</p>}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant="secondary" className="text-xs">
                      {day.places?.length} places
                    </Badge>
                    {isSelected && (
                      <Badge variant="default" className="text-xs">
                        Selected
                      </Badge>
                    )}
                  </div>
                </div>
              </AccordionTrigger>
              <AccordionContent className="px-4 pb-4">
                <PlacesList dayId={day.id} places={day.places} onPlacesChange={onPlacesChange || noop} />
              </AccordionContent>
            </AccordionItem>
          );
        })}
      </Accordion>
    </div>
  );
};

export default DaysListView;
