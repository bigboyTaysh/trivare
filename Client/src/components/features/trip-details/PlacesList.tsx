import React from "react";
import { Button } from "@/components/ui/button";
import { Plus, MapPin } from "lucide-react";
import type { DayAttractionDto } from "@/types/trips";
import { PlaceCard } from "./PlaceCard";

interface PlacesListProps {
  dayId: string;
  places: DayAttractionDto[] | null | undefined;
  onPlacesChange: () => void;
  onOpenAddDialog?: () => void;
}

export const PlacesList: React.FC<PlacesListProps> = ({ places, onPlacesChange, onOpenAddDialog }) => {
  const placesArray = places || [];
  const sortedPlaces = [...placesArray].sort((a, b) => a.order - b.order);

  const handleAddPlace = () => {
    onOpenAddDialog?.();
  };

  if (placesArray.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center h-full">
        <MapPin className="h-8 w-8 text-gray-400 mb-2" />
        <h3 className="text-sm font-medium text-gray-900 mb-1.5">No places planned yet</h3>
        <p className="text-gray-500 text-center text-xs mb-2">
          Add attractions, restaurants, or other points of interest to your day.
        </p>
        <Button onClick={handleAddPlace} className="flex items-center gap-1 h-7">
          <Plus className="h-3 w-3" />
          Add Place
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-medium">Places ({placesArray.length})</h3>
        <Button onClick={handleAddPlace} size="sm" className="flex items-center gap-1 h-7">
          <Plus className="h-3 w-3" />
          Add Place
        </Button>
      </div>

      <div className="space-y-1">
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
