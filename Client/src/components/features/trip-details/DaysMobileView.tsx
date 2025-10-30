import React from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Calendar, List } from "lucide-react";
import type { DayWithPlacesDto } from "@/types/trips";
import TripCalendarView from "./TripCalendarView";
import DaysListView from "./DaysListView";

interface DaysMobileViewProps {
  days: DayWithPlacesDto[];
  isLoading?: boolean;
  onDaySelect?: (day: DayWithPlacesDto | null, date: Date | null) => void;
  onAddDay?: (date?: Date) => void;
  onPlacesChange?: () => void;
  selectedDayId?: string;
  selectedDate?: Date;
  tripStartDate?: string;
  tripEndDate?: string;
}

const DaysMobileView: React.FC<DaysMobileViewProps> = ({
  days,
  isLoading = false,
  onDaySelect,
  onAddDay,
  onPlacesChange,
  selectedDayId,
  selectedDate,
  tripStartDate,
  tripEndDate,
}) => {
  const handleDaySelect = (day: DayWithPlacesDto | null, date: Date | null) => {
    onDaySelect?.(day, date);
  };

  const handlePlacesChange = () => {
    onPlacesChange?.();
  };

  return (
    <Tabs defaultValue="calendar" className="w-full">
      <TabsList className="grid w-full grid-cols-2">
        <TabsTrigger value="calendar" className="flex items-center gap-2">
          <Calendar className="h-4 w-4" />
          Calendar
        </TabsTrigger>
        <TabsTrigger value="list" className="flex items-center gap-2">
          <List className="h-4 w-4" />
          List
        </TabsTrigger>
      </TabsList>

      <TabsContent value="calendar" className="mt-6">
        <TripCalendarView
          days={days}
          isLoading={isLoading}
          onDaySelect={handleDaySelect}
          onAddDay={onAddDay}
          selectedDayId={selectedDayId}
          selectedDate={selectedDate}
          tripStartDate={tripStartDate}
          tripEndDate={tripEndDate}
        />
      </TabsContent>

      <TabsContent value="list" className="mt-6">
        <DaysListView
          days={days}
          isLoading={isLoading}
          onDaySelect={handleDaySelect}
          onPlacesChange={handlePlacesChange}
          onAddDay={onAddDay}
          selectedDayId={selectedDayId}
          selectedDate={selectedDate}
        />
      </TabsContent>
    </Tabs>
  );
};

export default DaysMobileView;
