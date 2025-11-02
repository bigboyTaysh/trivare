import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

interface CalendarProps {
  selected?: Date;
  onSelect?: (date: Date | undefined) => void;
  modifiers?: Record<string, Date[]>;
  modifiersClassNames?: Record<string, string>;
  disabled?: Date[];
  className?: string;
  minDate?: Date;
  maxDate?: Date;
}

const Calendar: React.FC<CalendarProps> = ({
  selected,
  onSelect,
  modifiers = {},
  modifiersClassNames = {},
  disabled = [],
  className,
  minDate,
  maxDate,
}) => {
  const [currentMonth, setCurrentMonth] = useState(new Date());

  const monthNames = [
    "January",
    "February",
    "March",
    "April",
    "May",
    "June",
    "July",
    "August",
    "September",
    "October",
    "November",
    "December",
  ];

  const dayNames = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];

  const getDaysInMonth = (date: Date) => {
    const year = date.getFullYear();
    const month = date.getMonth();
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();

    const days = [];

    // Add empty cells for days before the first day of the month
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(null);
    }

    // Add all days of the month
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(new Date(year, month, day));
    }

    return days;
  };

  const isDateInModifiers = (date: Date, modifierKey: string) => {
    return modifiers[modifierKey]?.some((modifierDate) => modifierDate.toDateString() === date.toDateString());
  };

  const getDateModifiers = (date: Date) => {
    const applicableModifiers: string[] = [];
    Object.keys(modifiers).forEach((key) => {
      if (isDateInModifiers(date, key)) {
        applicableModifiers.push(key);
      }
    });
    return applicableModifiers;
  };

  const getModifierClassNames = (date: Date) => {
    const applicableModifiers = getDateModifiers(date);
    return applicableModifiers.map((key) => modifiersClassNames[key]).filter(Boolean);
  };

  const isSelected = (date: Date) => {
    return selected && date.toDateString() === selected.toDateString();
  };

  const isDisabled = (date: Date) => {
    return disabled.some((disabledDate) => disabledDate.toDateString() === date.toDateString());
  };

  const handleDateClick = (date: Date) => {
    if (isDisabled(date)) {
      return; // Don't allow clicking disabled dates
    }
    if (onSelect) {
      onSelect(isSelected(date) ? undefined : date);
    }
  };

  const navigateMonth = (direction: "prev" | "next") => {
    setCurrentMonth((prev) => {
      const newMonth = new Date(prev);
      if (direction === "prev") {
        newMonth.setMonth(prev.getMonth() - 1);
      } else {
        newMonth.setMonth(prev.getMonth() + 1);
      }
      return newMonth;
    });
  };

  // Check if we can navigate to previous/next month
  const canNavigatePrev = () => {
    if (!minDate) return true;
    const prevMonth = new Date(currentMonth);
    prevMonth.setMonth(currentMonth.getMonth() - 1);
    // Check if the last day of the previous month is before minDate
    const lastDayOfPrevMonth = new Date(prevMonth.getFullYear(), prevMonth.getMonth() + 1, 0);
    return lastDayOfPrevMonth >= minDate;
  };

  const canNavigateNext = () => {
    if (!maxDate) return true;
    const nextMonth = new Date(currentMonth);
    nextMonth.setMonth(currentMonth.getMonth() + 1);
    // Check if the first day of the next month is after maxDate
    const firstDayOfNextMonth = new Date(nextMonth.getFullYear(), nextMonth.getMonth(), 1);
    return firstDayOfNextMonth <= maxDate;
  };

  const days = getDaysInMonth(currentMonth);

  return (
    <div className={cn("p-3 bg-card border rounded-lg shadow-sm", className)}>
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigateMonth("prev")}
          disabled={!canNavigatePrev()}
          className="h-7 w-7 p-0"
        >
          ‹
        </Button>
        <div className="font-semibold">
          {monthNames[currentMonth.getMonth()]} {currentMonth.getFullYear()}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => navigateMonth("next")}
          disabled={!canNavigateNext()}
          className="h-7 w-7 p-0"
        >
          ›
        </Button>
      </div>

      {/* Day headers */}
      <div className="grid grid-cols-7 gap-1 mb-2">
        {dayNames.map((day) => (
          <div key={day} className="text-center text-sm font-medium text-muted-foreground py-1">
            {day}
          </div>
        ))}
      </div>

      {/* Calendar grid */}
      <div className="grid grid-cols-7 gap-1">
        {days.map((date, index) => (
          <div key={index} className="aspect-square">
            {date ? (
              <Button
                variant="ghost"
                disabled={isDisabled(date)}
                className={cn(
                  "w-full h-full p-0 hover:bg-accent",
                  !isSelected(date) && !isDisabled(date) && getModifierClassNames(date),
                  isDisabled(date) && "text-muted-foreground cursor-not-allowed hover:bg-transparent",
                  isSelected(date) && "bg-primary text-primary-foreground hover:bg-primary/90"
                )}
                onClick={() => handleDateClick(date)}
              >
                {date.getDate()}
              </Button>
            ) : (
              <div className="w-full h-full" />
            )}
          </div>
        ))}
      </div>
    </div>
  );
};

export { Calendar };
