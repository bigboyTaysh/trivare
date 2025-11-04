import React, { useMemo } from "react";
import { Calendar } from "@/components/ui/calendar";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import type { DayWithPlacesDto, UpdatePlaceRequest, AddPlaceRequest, DayAttractionDto } from "@/types/trips";
import DayView from "./DayView";

interface TripCalendarViewProps {
  days: DayWithPlacesDto[];
  isLoading?: boolean;
  onDaySelect?: (day: DayWithPlacesDto | null, date: Date | null) => void;
  onAddDay?: (date?: Date) => Promise<DayWithPlacesDto>;
  onDayCreated?: (date: Date) => void;
  selectedDayId?: string;
  selectedDate?: Date;
  tripStartDate?: string;
  tripEndDate?: string;
  tripDestination?: string;
  onPlacesChange?: () => void;
  onPlaceVisitedChange?: (dayId: string, placeId: string, isVisited: boolean) => Promise<void>;
  onPlaceUpdate?: (placeId: string, data: UpdatePlaceRequest) => Promise<void>;
  onAddPlace?: (dayId: string, data: AddPlaceRequest) => Promise<unknown>;
  onDeletePlace?: (dayId: string, placeId: string) => Promise<void>;
  onReorderPlaces?: (dayId: string, reorderedPlaces: DayAttractionDto[]) => Promise<void>;
}

const TripCalendarView: React.FC<TripCalendarViewProps> = ({
  days,
  isLoading = false,
  onDaySelect,
  onAddDay,
  onDayCreated,
  selectedDayId,
  selectedDate: propSelectedDate,
  tripStartDate,
  tripEndDate,
  tripDestination,
  onPlacesChange,
  onPlaceVisitedChange,
  onPlaceUpdate,
  onAddPlace,
  onDeletePlace,
  onReorderPlaces,
}) => {
  // Use the selected date from props (which can be any date, not just days with data)
  const selectedDate = propSelectedDate;

  // Create modifiers for the calendar
  const modifiers = useMemo(() => {
    const daysWithPlaces: Date[] = [];

    if (days) {
      days.forEach((day) => {
        if (day.places && day.places.length > 0) {
          // Parse date string and normalize to midnight local time to avoid timezone issues
          const [year, month, dayNum] = day.date.split("-").map(Number);
          const date = new Date(year, month - 1, dayNum);
          daysWithPlaces.push(date);
        }
      });
    }

    // Add today's date as a modifier
    const today = new Date();
    const todayNormalized = new Date(today.getFullYear(), today.getMonth(), today.getDate());

    return {
      hasPlaces: daysWithPlaces,
      today: [todayNormalized],
    };
  }, [days]);

  const modifiersClassNames = {
    hasPlaces: "bg-blue-100 text-blue-900 font-semibold ring-2 ring-blue-400",
    today: "ring-1 ring-gray-400",
  };

  // Calculate disabled dates (dates outside trip range) and min/max dates for navigation
  const { disabledDates, minDate, maxDate } = useMemo(() => {
    const disabled: Date[] = [];
    let min: Date | undefined;
    let max: Date | undefined;

    if (tripStartDate && tripEndDate) {
      // Parse dates and normalize to midnight local time
      const [startYear, startMonth, startDay] = tripStartDate.split("-").map(Number);
      const [endYear, endMonth, endDay] = tripEndDate.split("-").map(Number);
      const startDate = new Date(startYear, startMonth - 1, startDay);
      const endDate = new Date(endYear, endMonth - 1, endDay);

      min = startDate;
      max = endDate;

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

    return { disabledDates: disabled, minDate: min, maxDate: max };
  }, [tripStartDate, tripEndDate]);

  const handleDateSelect = (date: Date | undefined) => {
    if (date) {
      // Normalize the selected date to midnight to ensure consistent comparison
      const normalizedDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());

      // Find the day for this date
      const day = days.find((d) => {
        // Parse the day date string and normalize to midnight local time
        const [year, month, dayNum] = d.date.split("-").map(Number);
        const dayDate = new Date(year, month - 1, dayNum);
        return dayDate.toDateString() === normalizedDate.toDateString();
      });

      // Always allow selecting a date - if no day exists, pass null as day but keep the date
      onDaySelect?.(day || null, normalizedDate);
    } else {
      onDaySelect?.(null, null);
    }
  };

  const selectedDay = selectedDayId ? days.find((d) => d.id === selectedDayId) || null : null;

  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 lg:items-start">
      {/* Calendar Section */}
      <div className="lg:col-span-1 space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>Trip Calendar</CardTitle>
          </CardHeader>
          <CardContent className="flex justify-center">
            <div className="w-full max-w-md space-y-4">
              <Calendar
                selected={selectedDate}
                onSelect={handleDateSelect}
                modifiers={modifiers}
                modifiersClassNames={modifiersClassNames}
                disabled={disabledDates}
                minDate={minDate}
                maxDate={maxDate}
                className="w-full"
              />

              <div className="space-y-2">
                <div className="flex items-center gap-2 text-sm flex-wrap">
                  <div className="flex items-center gap-1">
                    <div className="w-3 h-3 ring-1 ring-gray-400 rounded"></div>
                    <span>Today</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <div className="w-3 h-3 bg-blue-100 ring-2 ring-blue-400 rounded"></div>
                    <span>Days with places</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <div className="w-3 h-3 bg-blue-500 rounded"></div>
                    <span>Selected day</span>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Day Details Section */}
      <div className="lg:col-span-2 lg:h-full">
        <DayView
          day={selectedDay}
          selectedDate={selectedDate}
          onAddDay={onAddDay}
          onDayCreated={(date) => {
            onDayCreated?.(date);
            onDaySelect?.(null, date);
          }}
          isLoading={isLoading}
          onPlacesChange={onPlacesChange}
          onPlaceVisitedChange={onPlaceVisitedChange}
          onPlaceUpdate={onPlaceUpdate}
          onAddPlace={onAddPlace}
          onDeletePlace={onDeletePlace}
          onReorderPlaces={onReorderPlaces}
          tripDestination={tripDestination}
        />
      </div>
    </div>
  );
};

export default TripCalendarView;
