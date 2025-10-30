import React, { useState, useRef, useLayoutEffect } from "react";
import { useTripDays } from "@/hooks/useTripDays";
import TripCalendarView from "./TripCalendarView";
import DaysMobileView from "./DaysMobileView";
import type { DayWithPlacesDto } from "@/types/trips";

interface DaysSectionProps {
  tripId: string;
  totalFileCount: number;
  onFileChange: () => void;
  tripStartDate?: string;
  tripEndDate?: string;
}

const DaysSection: React.FC<DaysSectionProps> = ({ tripId, tripStartDate, tripEndDate }) => {
  const { days, isLoading, createDay, refetch } = useTripDays(tripId);
  const [selectedDay, setSelectedDay] = useState<DayWithPlacesDto | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const hasInitializedRef = useRef(false);

  // Auto-select default day when days data loads and no day is selected
  useLayoutEffect(() => {
    if (days.length === 0 || !tripStartDate || !tripEndDate || hasInitializedRef.current) {
      return;
    }

    // Get today's date in YYYY-MM-DD format
    const today = new Date().toISOString().split("T")[0];

    let defaultDay: DayWithPlacesDto;

    if (today < tripStartDate) {
      // Current date is before trip - select first day
      defaultDay = days[0];
    } else if (today > tripEndDate) {
      // Trip is in the past - select last day
      defaultDay = days[days.length - 1];
    } else {
      // Current date is within trip dates - find current day or closest
      const foundDay = days.find((day) => day.date === today);
      defaultDay = foundDay || days[0];
    }

    // eslint-disable-next-line
    setSelectedDay(defaultDay);
    setSelectedDate(new Date(defaultDay.date));
    hasInitializedRef.current = true;
  }, [days, tripStartDate, tripEndDate]);

  const handleDaySelect = (day: DayWithPlacesDto | null, date: Date | null) => {
    setSelectedDay(day);
    // Normalize the date to midnight to ensure proper comparison across views
    if (date) {
      const normalizedDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
      setSelectedDate(normalizedDate);
    } else {
      setSelectedDate(null);
    }
  };

  const handleAddDay = async (date?: Date) => {
    try {
      // Use the provided date, or selectedDate, or current date as fallback
      const dateToUse = date || selectedDate || new Date();
      const dateString = dateToUse.toISOString().split("T")[0]; // Format as YYYY-MM-DD
      await createDay({ date: dateString });
    } catch (error) {
      // eslint-disable-next-line no-console
      console.error("Failed to add day:", error);
    }
  };

  const handlePlacesChange = () => {
    refetch();
  };

  return (
    <div className="space-y-6">
      {/* Desktop Layout */}
      <div className="hidden lg:block">
        <TripCalendarView
          days={days}
          isLoading={isLoading}
          onDaySelect={handleDaySelect}
          onAddDay={handleAddDay}
          selectedDayId={selectedDay?.id}
          selectedDate={selectedDate || undefined}
          tripStartDate={tripStartDate}
          tripEndDate={tripEndDate}
        />
      </div>

      {/* Mobile Layout */}
      <div className="lg:hidden">
        <DaysMobileView
          days={days}
          isLoading={isLoading}
          onDaySelect={handleDaySelect}
          onAddDay={handleAddDay}
          onPlacesChange={handlePlacesChange}
          selectedDayId={selectedDay?.id}
          selectedDate={selectedDate || undefined}
          tripStartDate={tripStartDate}
          tripEndDate={tripEndDate}
        />
      </div>
    </div>
  );
};

export default DaysSection;
