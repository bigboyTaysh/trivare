import React, { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import TransportSection from "./TransportSection";
import AccommodationSection from "./AccommodationSection";
import DaysSection from "./DaysSection";
import { Calendar, Home, Plane } from "lucide-react";
import type { DayWithPlacesDto } from "@/types/trips";

interface TripContentProps {
  tripId: string;
  totalFileCount: number;
  onFileChange: () => void;
  tripStartDate?: string;
  tripEndDate?: string;
}

const TripContent: React.FC<TripContentProps> = ({
  tripId,
  totalFileCount,
  onFileChange,
  tripStartDate,
  tripEndDate,
}) => {
  const [activeSection, setActiveSection] = useState<string | null>("days");
  // Shared state for day selection across mobile and desktop views
  const [selectedDay, setSelectedDay] = useState<DayWithPlacesDto | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);

  const handleDaySelect = (day: DayWithPlacesDto | null, date: Date | null) => {
    setSelectedDay(day);
    setSelectedDate(date);
  };

  return (
    <>
      {/* Desktop: Tabs */}
      <div className="hidden md:block">
        <Tabs value={activeSection || ""} onValueChange={setActiveSection}>
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="days" className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Days
            </TabsTrigger>
            <TabsTrigger value="accommodation" className="flex items-center gap-2">
              <Home className="h-4 w-4" />
              Accommodation
            </TabsTrigger>
            <TabsTrigger value="transport" className="flex items-center gap-2">
              <Plane className="h-4 w-4" />
              Transport
            </TabsTrigger>
          </TabsList>
          <TabsContent value="days" className="mt-6">
            {activeSection === "days" && (
              <DaysSection
                tripId={tripId}
                totalFileCount={totalFileCount}
                onFileChange={onFileChange}
                tripStartDate={tripStartDate}
                tripEndDate={tripEndDate}
                selectedDay={selectedDay}
                selectedDate={selectedDate}
                onDaySelect={handleDaySelect}
              />
            )}
          </TabsContent>
          <TabsContent value="accommodation" className="mt-6">
            {activeSection === "accommodation" && (
              <AccommodationSection tripId={tripId} totalFileCount={totalFileCount} onFileChange={onFileChange} />
            )}
          </TabsContent>
          <TabsContent value="transport" className="mt-6">
            {activeSection === "transport" && (
              <TransportSection tripId={tripId} totalFileCount={totalFileCount} onFileChange={onFileChange} />
            )}
          </TabsContent>
        </Tabs>
      </div>

      {/* Mobile: Accordion */}
      <div className="md:hidden">
        <Accordion type="single" collapsible value={activeSection || ""} onValueChange={setActiveSection}>
          <AccordionItem value="days">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Days
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "days" && (
                <DaysSection
                  tripId={tripId}
                  totalFileCount={totalFileCount}
                  onFileChange={onFileChange}
                  tripStartDate={tripStartDate}
                  tripEndDate={tripEndDate}
                  selectedDay={selectedDay}
                  selectedDate={selectedDate}
                  onDaySelect={handleDaySelect}
                />
              )}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="accommodation">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Home className="h-4 w-4" />
                Accommodation
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "accommodation" && (
                <AccommodationSection tripId={tripId} totalFileCount={totalFileCount} onFileChange={onFileChange} />
              )}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="transport">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Plane className="h-4 w-4" />
                Transport
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "transport" && (
                <TransportSection tripId={tripId} totalFileCount={totalFileCount} onFileChange={onFileChange} />
              )}
            </AccordionContent>
          </AccordionItem>
        </Accordion>
      </div>
    </>
  );
};

export default TripContent;
