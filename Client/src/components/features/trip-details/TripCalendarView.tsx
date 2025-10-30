import React, { useMemo } from "react";
import { Calendar } from "@/components/ui/calendar";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import type { DayWithPlacesDto } from "@/types/trips";
import DayView from "./DayView";

interface TripCalendarViewProps {
  days: DayWithPlacesDto[];
  isLoading?: boolean;
  onDaySelect?: (day: DayWithPlacesDto | null, date: Date | null) => void;
  onAddDay?: (date?: Date) => void;
  selectedDayId?: string;
  selectedDate?: Date;
  tripStartDate?: string;
  tripEndDate?: string;
}

const TripCalendarView: React.FC<TripCalendarViewProps> = ({
  days,
  isLoading = false,
  onDaySelect,
  onAddDay,
  selectedDayId,
  selectedDate: propSelectedDate,
  tripStartDate,
  tripEndDate,
}) => {
  // Use the selected date from props (which can be any date, not just days with data)
  const selectedDate = propSelectedDate;

  // Create modifiers for the calendar
  const modifiers = useMemo(() => {
    const tripDays: Date[] = [];
    const daysWithPlaces: Date[] = [];

    if (days) {
      days.forEach((day) => {
        const date = new Date(day.date);
        tripDays.push(date);
        if (day.places && day.places.length > 0) {
          daysWithPlaces.push(date);
        }
      });
    }

    return {
      tripDay: tripDays,
      hasPlaces: daysWithPlaces,
    };
  }, [days]);

  const modifiersClassNames = {
    tripDay: "bg-blue-100 text-blue-900 font-semibold",
    hasPlaces: "ring-2 ring-blue-400",
  };

  // Calculate disabled dates (dates outside trip range)
  const disabledDates = useMemo(() => {
    const disabled: Date[] = [];

    if (tripStartDate && tripEndDate) {
      const startDate = new Date(tripStartDate);
      const endDate = new Date(tripEndDate);

      // Create a date range from start to end and exclude dates within the range
      // For simplicity, we'll disable dates that are clearly before start or after end
      // This is a basic implementation - in a real app you might want more sophisticated logic

      const currentDate = new Date();
      const currentYear = currentDate.getFullYear();
      const currentMonth = currentDate.getMonth();

      // Generate dates for the current month view and disable those outside the trip range
      for (let monthOffset = -1; monthOffset <= 1; monthOffset++) {
        const lastDay = new Date(currentYear, currentMonth + monthOffset + 1, 0);

        for (let day = 1; day <= lastDay.getDate(); day++) {
          const date = new Date(currentYear, currentMonth + monthOffset, day);
          if (date < startDate || date > endDate) {
            disabled.push(date);
          }
        }
      }
    }

    return disabled;
  }, [tripStartDate, tripEndDate]);

  const handleDateSelect = (date: Date | undefined) => {
    if (date) {
      // Find the day for this date
      const day = days.find((d) => {
        const dayDate = new Date(d.date);
        return dayDate.toDateString() === date.toDateString();
      });

      // Always allow selecting a date - if no day exists, pass null as day but keep the date
      onDaySelect?.(day || null, date);
    } else {
      onDaySelect?.(null, null);
    }
  };

  const selectedDay = selectedDayId ? days.find((d) => d.id === selectedDayId) || null : null;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
      {/* Calendar Section */}
      <div className="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>Trip Calendar</CardTitle>
          </CardHeader>
          <CardContent>
            <Calendar
              selected={selectedDate}
              onSelect={handleDateSelect}
              modifiers={modifiers}
              modifiersClassNames={modifiersClassNames}
              disabled={disabledDates}
              className="w-full"
            />

            <div className="mt-4 space-y-2">
              <div className="flex items-center gap-2 text-sm">
                <div className="w-3 h-3 bg-blue-100 rounded"></div>
                <span>Trip days</span>
                <div className="w-3 h-3 bg-blue-100 ring-2 ring-blue-400 rounded"></div>
                <span>Days with places</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Day Details Section */}
      <div>
        <DayView day={selectedDay} selectedDate={selectedDate} onAddDay={onAddDay} isLoading={isLoading} />
      </div>
    </div>
  );
};

export default TripCalendarView;
