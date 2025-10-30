import React, { useState, useRef, useLayoutEffect, useCallback } from "react";
import { useTripDays } from "@/hooks/useTripDays";
import TripCalendarView from "./TripCalendarView";
import type { DayWithPlacesDto } from "@/types/trips";

interface DaysSectionProps {
  tripId: string;
  totalFileCount: number;
  onFileChange: () => void;
  tripStartDate?: string;
  tripEndDate?: string;
  selectedDay?: DayWithPlacesDto | null;
  selectedDate?: Date | null;
  onDaySelect?: (day: DayWithPlacesDto | null, date: Date | null) => void;
}

const DaysSection: React.FC<DaysSectionProps> = ({
  tripId,
  tripStartDate,
  tripEndDate,
  selectedDay: externalSelectedDay,
  selectedDate: externalSelectedDate,
  onDaySelect: externalOnDaySelect,
}) => {
  const { days, isLoading, createDay } = useTripDays(tripId);
  const [internalSelectedDay, setInternalSelectedDay] = useState<DayWithPlacesDto | null>(null);
  const [internalSelectedDate, setInternalSelectedDate] = useState<Date | null>(null);
  const hasInitializedRef = useRef(false);
  const previousDaysLengthRef = useRef(0);
  const lastAddedDateRef = useRef<string | null>(null);

  // Use external state if provided, otherwise use internal state
  const selectedDay = externalSelectedDay !== undefined ? externalSelectedDay : internalSelectedDay;
  const selectedDate = externalSelectedDate !== undefined ? externalSelectedDate : internalSelectedDate;

  // Helper function to determine the correct date to select
  const getDefaultSelection = useCallback(
    (todayStr: string): { date: Date | null; day: DayWithPlacesDto | null } => {
      if (!tripStartDate || !tripEndDate) {
        return { date: null, day: null };
      }

      if (todayStr < tripStartDate) {
        // Current date is before trip - don't select any day
        return { date: null, day: null };
      } else if (todayStr > tripEndDate) {
        // Trip is in the past - don't select any day
        return { date: null, day: null };
      } else {
        // Current date is within trip dates - select today regardless of whether a day entry exists
        const [year, month, dayNum] = todayStr.split("-").map(Number);
        const defaultDate = new Date(year, month - 1, dayNum);
        const defaultDay = days.find((day) => day.date === todayStr) || null;
        return { date: defaultDate, day: defaultDay };
      }
    },
    [days, tripStartDate, tripEndDate]
  );

  // Auto-select default day when component mounts or when days are added
  useLayoutEffect(() => {
    if (!tripStartDate || !tripEndDate) {
      return;
    }

    const daysChanged = days.length !== previousDaysLengthRef.current;
    previousDaysLengthRef.current = days.length;

    // Get today's date in YYYY-MM-DD format
    const today = new Date().toISOString().split("T")[0];

    // Initial load - select the appropriate day
    if (!hasInitializedRef.current) {
      const { date: defaultDate, day: defaultDay } = getDefaultSelection(today);

      if (externalOnDaySelect) {
        externalOnDaySelect(defaultDay, defaultDate);
      } else {
        // Initial state setup - this is a valid use case for setState in effects
        // We're initializing state based on external data (days, trip dates)
        // eslint-disable-next-line
        setInternalSelectedDay(defaultDay);
        setInternalSelectedDate(defaultDate);
      }
      hasInitializedRef.current = true;
      return;
    }

    // If days were added, re-evaluate selection to ensure we're on the correct day
    if (daysChanged && days.length > 0) {
      const currentSelectedDateStr = selectedDate
        ? `${selectedDate.getFullYear()}-${String(selectedDate.getMonth() + 1).padStart(2, "0")}-${String(selectedDate.getDate()).padStart(2, "0")}`
        : null;

      // Check if we just added a day - if so, keep selection on that date
      if (lastAddedDateRef.current && currentSelectedDateStr === lastAddedDateRef.current) {
        // Find the newly created day for the selected date
        const newDay = days.find((day) => day.date === currentSelectedDateStr) || null;

        // Update the day reference if it was just created
        if (newDay && selectedDay?.id !== newDay.id) {
          if (externalOnDaySelect) {
            externalOnDaySelect(newDay, selectedDate);
          } else {
            setInternalSelectedDay(newDay);
          }
        }

        // Clear the last added date
        lastAddedDateRef.current = null;
      } else {
        // Normal behavior: reset to today if within trip dates
        const { date: defaultDate, day: defaultDay } = getDefaultSelection(today);
        const defaultDateStr = defaultDate
          ? `${defaultDate.getFullYear()}-${String(defaultDate.getMonth() + 1).padStart(2, "0")}-${String(defaultDate.getDate()).padStart(2, "0")}`
          : null;

        if (currentSelectedDateStr !== defaultDateStr) {
          if (externalOnDaySelect) {
            externalOnDaySelect(defaultDay, defaultDate);
          } else {
            setInternalSelectedDay(defaultDay);
            setInternalSelectedDate(defaultDate);
          }
        } else if (defaultDate && currentSelectedDateStr === defaultDateStr && selectedDay !== defaultDay) {
          // Same date but day object might have been created, update the day reference
          if (externalOnDaySelect) {
            externalOnDaySelect(defaultDay, defaultDate);
          } else {
            setInternalSelectedDay(defaultDay);
          }
        }
      }
    }
  }, [days, tripStartDate, tripEndDate, externalOnDaySelect, selectedDay, selectedDate, getDefaultSelection]);

  const handleDaySelect = (day: DayWithPlacesDto | null, date: Date | null) => {
    // Normalize the date to midnight to ensure proper comparison across views
    const normalizedDate = date ? new Date(date.getFullYear(), date.getMonth(), date.getDate()) : null;

    // Use external handler if provided, otherwise use internal state
    if (externalOnDaySelect) {
      externalOnDaySelect(day, normalizedDate);
    } else {
      setInternalSelectedDay(day);
      setInternalSelectedDate(normalizedDate);
    }
  };

  const handleAddDay = async (date?: Date) => {
    try {
      // Use the provided date, or selectedDate, or current date as fallback
      const dateToUse = date || selectedDate || new Date();
      // Format date as YYYY-MM-DD using local time to avoid timezone conversion issues
      const year = dateToUse.getFullYear();
      const month = String(dateToUse.getMonth() + 1).padStart(2, "0");
      const day = String(dateToUse.getDate()).padStart(2, "0");
      const dateString = `${year}-${month}-${day}`;

      // Store the date we're adding to preserve selection after creation
      lastAddedDateRef.current = dateString;

      await createDay({ date: dateString });
    } catch (error) {
      // eslint-disable-next-line no-console
      console.error("Failed to add day:", error);
      // Clear the last added date on error
      lastAddedDateRef.current = null;
    }
  };

  return (
    <div className="space-y-6">
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
  );
};

export default DaysSection;
