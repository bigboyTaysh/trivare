import React, { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import TransportSection from "./TransportSection";
import AccommodationSection from "./AccommodationSection";
import DaysSection from "./DaysSection";
import FilesSection from "./FilesSection";

interface TripContentProps {
  tripId: string;
}

const TripContent: React.FC<TripContentProps> = ({ tripId }) => {
  const [activeSection, setActiveSection] = useState<string | null>("accommodation");

  return (
    <>
      {/* Desktop: Tabs */}
      <div className="hidden md:block">
        <Tabs value={activeSection || "accommodation"} onValueChange={setActiveSection}>
          <TabsList className="grid w-full grid-cols-4">
            <TabsTrigger value="accommodation">Accommodation</TabsTrigger>
            <TabsTrigger value="transport">Transport</TabsTrigger>
            <TabsTrigger value="days">Days</TabsTrigger>
            <TabsTrigger value="files">Files</TabsTrigger>
          </TabsList>
          <TabsContent value="accommodation" className="mt-6">
            {activeSection === "accommodation" && <AccommodationSection tripId={tripId} />}
          </TabsContent>
          <TabsContent value="transport" className="mt-6">
            {activeSection === "transport" && <TransportSection tripId={tripId} />}
          </TabsContent>
          <TabsContent value="days" className="mt-6">
            {activeSection === "days" && <DaysSection tripId={tripId} />}
          </TabsContent>
          <TabsContent value="files" className="mt-6">
            {activeSection === "files" && <FilesSection tripId={tripId} />}
          </TabsContent>
        </Tabs>
      </div>

      {/* Mobile: Accordion */}
      <div className="md:hidden">
        <Accordion type="single" collapsible value={activeSection || "accommodation"} onValueChange={setActiveSection}>
          <AccordionItem value="accommodation">
            <AccordionTrigger>Accommodation</AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "accommodation" && <AccommodationSection tripId={tripId} />}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="transport">
            <AccordionTrigger>Transport</AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "transport" && <TransportSection tripId={tripId} />}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="days">
            <AccordionTrigger>Days</AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "days" && <DaysSection tripId={tripId} />}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="files">
            <AccordionTrigger>Files</AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "files" && <FilesSection tripId={tripId} />}
            </AccordionContent>
          </AccordionItem>
        </Accordion>
      </div>
    </>
  );
};

export default TripContent;
