import React from "react";
import { Button } from "@/components/ui/button";
import { Plus, MapPin } from "lucide-react";
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors } from "@dnd-kit/core";
import type { DragEndEvent } from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import type { DayAttractionDto } from "@/types/trips";
import { PlaceCard } from "./PlaceCard";
import { api } from "@/services/api";
import { toast } from "sonner";

interface PlacesListProps {
  dayId: string;
  places: DayAttractionDto[] | null | undefined;
  onPlacesChange: () => void;
  onPlaceVisitedChange?: (dayId: string, placeId: string, isVisited: boolean) => Promise<void>;
  onOpenAddDialog?: () => void;
  onEditPlace?: (placeId: string) => void;
  onDeletePlace?: (dayId: string, placeId: string) => Promise<void>;
  onReorderPlaces?: (dayId: string, reorderedPlaces: DayAttractionDto[]) => Promise<void>;
  selectedDate?: Date;
}

export const PlacesList: React.FC<PlacesListProps> = ({
  places,
  onPlacesChange,
  onPlaceVisitedChange,
  onOpenAddDialog,
  onEditPlace,
  onDeletePlace,
  onReorderPlaces,
  dayId,
  selectedDate,
}) => {
  const placesArray = places || [];
  const sortedPlaces = [...placesArray].sort((a, b) => a.order - b.order);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const handleDragEnd = async (event: DragEndEvent) => {
    const { active, over } = event;

    if (over && active.id !== over.id) {
      const oldIndex = sortedPlaces.findIndex((place) => place.placeId === active.id);
      const newIndex = sortedPlaces.findIndex((place) => place.placeId === over.id);

      if (oldIndex !== -1 && newIndex !== -1) {
        const reorderedPlaces = arrayMove(sortedPlaces, oldIndex, newIndex);

        try {
          // Use optimistic update if available, otherwise fall back to direct API calls
          if (onReorderPlaces) {
            await onReorderPlaces(dayId, reorderedPlaces);
          } else {
            // Fallback: Update each place's order on the backend directly
            const updates = reorderedPlaces.map((place, index) => ({
              placeId: place.placeId,
              newOrder: index + 1,
            }));

            await Promise.all(
              updates.map(({ placeId, newOrder }) => api.updatePlaceOnDay(dayId, placeId, { order: newOrder }))
            );

            // Trigger a refetch as fallback
            onPlacesChange();
          }

          toast.success("Places reordered successfully");
        } catch (error) {
          console.error("Failed to reorder places:", error);
          toast.error("Failed to reorder places");
        }
      }
    }
  };

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
    <DndContext sensors={sensors} collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
      <SortableContext items={sortedPlaces.map((p) => p.placeId)} strategy={verticalListSortingStrategy}>
        <div className="space-y-2">
          {sortedPlaces.map((placeAttraction, index) => (
            <PlaceCard
              key={placeAttraction.placeId}
              placeAttraction={placeAttraction}
              index={index}
              onChange={onPlacesChange}
              onPlaceVisitedChange={onPlaceVisitedChange}
              onEdit={onEditPlace}
              onDeletePlace={onDeletePlace}
              selectedDate={selectedDate}
            />
          ))}
        </div>
      </SortableContext>
    </DndContext>
  );
};
