import React from "react";
import { Button } from "@/components/ui/button";
import { Plus, MapPin } from "lucide-react";
import type { DayAttractionDto } from "@/types/trips";
import { PlaceCard } from "./PlaceCard";

interface PlacesListProps {
  dayId: string;
  places: DayAttractionDto[] | null | undefined;
  onPlacesChange: () => void;
}

export const PlacesList: React.FC<PlacesListProps> = ({ dayId, places, onPlacesChange }) => {
  const placesArray = places || [];
  const sortedPlaces = [...placesArray].sort((a, b) => a.order - b.order);

  const handleAddPlace = () => {
    // TODO: Implement add place functionality
    console.log("Add place to day", dayId);
  };

  if (placesArray.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full">
        <MapPin className="h-12 w-12 text-gray-400 mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">No places planned yet</h3>
        <p className="text-gray-500 text-center mb-4">
          Add attractions, restaurants, or other points of interest to your day.
        </p>
        <Button onClick={handleAddPlace} className="flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Add Place
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-medium">Places ({placesArray.length})</h3>
        <Button onClick={handleAddPlace} size="sm" className="flex items-center gap-2">
          <Plus className="h-4 w-4" />
          Add Place
        </Button>
      </div>

      <div className="space-y-3">
        {sortedPlaces.map((placeAttraction, index) => (
          <PlaceCard
            key={placeAttraction.placeId}
            placeAttraction={placeAttraction}
            index={index}
            onChange={onPlacesChange}
          />
        ))}
      </div>
    </div>
  );
};
