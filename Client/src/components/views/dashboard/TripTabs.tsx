import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { TripList } from "./TripList";
import type { TripListDto } from "@/types/trips";

interface TripTabsProps {
  ongoingTrips: TripListDto[];
  pastTrips: TripListDto[];
}

/**
 * Tabbed interface for categorized trip lists
 * Ongoing tab shows current and future trips, Past tab shows completed trips
 */
export function TripTabs({ ongoingTrips, pastTrips }: TripTabsProps) {
  return (
    <Tabs defaultValue="ongoing" className="w-full">
      <TabsList className="grid w-full max-w-md grid-cols-2">
        <TabsTrigger value="ongoing">Ongoing {ongoingTrips.length > 0 && `(${ongoingTrips.length})`}</TabsTrigger>
        <TabsTrigger value="past">Past {pastTrips.length > 0 && `(${pastTrips.length})`}</TabsTrigger>
      </TabsList>

      <TabsContent value="ongoing" className="mt-6">
        <TripList trips={ongoingTrips} category="Ongoing" />
      </TabsContent>

      <TabsContent value="past" className="mt-6">
        <TripList trips={pastTrips} category="Past" />
      </TabsContent>
    </Tabs>
  );
}
