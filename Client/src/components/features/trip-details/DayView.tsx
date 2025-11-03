import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { PlaceForm } from "../../forms/PlaceForm";
import { api } from "@/services/api";
import { toast } from "sonner";
import type { DayWithPlacesDto, AddPlaceRequest, UpdatePlaceRequest, DayAttractionDto } from "@/types/trips";
import { PlacesList } from "./PlacesList";

interface DayViewProps {
  day: DayWithPlacesDto | null;
  selectedDate?: Date;
  onAddDay?: (date: Date) => Promise<DayWithPlacesDto>;
  onDayCreated?: (date: Date) => void;
  isLoading?: boolean;
  onPlacesChange?: () => void;
  onPlaceVisitedChange?: (dayId: string, placeId: string, isVisited: boolean) => Promise<void>;
  onPlaceUpdate?: (placeId: string, data: UpdatePlaceRequest) => Promise<void>;
  onAddPlace?: (dayId: string, data: AddPlaceRequest) => Promise<unknown>;
  onDeletePlace?: (dayId: string, placeId: string) => Promise<void>;
  onReorderPlaces?: (dayId: string, reorderedPlaces: DayAttractionDto[]) => Promise<void>;
  tripDestination?: string;
}

const DayView: React.FC<DayViewProps> = ({
  day,
  selectedDate,
  onAddDay,
  onDayCreated,
  isLoading = false,
  onPlacesChange,
  onPlaceVisitedChange,
  onPlaceUpdate,
  onAddPlace,
  onDeletePlace,
  onReorderPlaces,
  tripDestination,
}) => {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [editingPlaceId, setEditingPlaceId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isLoading) {
    return (
      <Card className="h-full">
        <CardHeader>
          <div className="h-6 bg-gray-200 rounded animate-pulse" />
        </CardHeader>
        <CardContent>
          <div className="space-y-3">
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
            <div className="h-4 bg-gray-200 rounded animate-pulse" />
          </div>
        </CardContent>
      </Card>
    );
  }

  // Handle case when no date is selected
  if (!selectedDate) {
    return (
      <Card className="h-full">
        <CardContent className="flex items-center justify-center h-full">
          <p className="text-muted-foreground">Select a day to view details</p>
        </CardContent>
      </Card>
    );
  }

  const formattedDate = selectedDate.toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const handleAddPlace = async (request: AddPlaceRequest) => {
    setIsSubmitting(true);
    try {
      let dayIdToUse = day?.id;

      // If no day exists yet, create one first
      if (!day && selectedDate && onAddDay) {
        try {
          const newDay = await onAddDay(selectedDate);
          dayIdToUse = newDay.id;
          // Signal that a day was created for this date - useLayoutEffect will handle selection
          onDayCreated?.(selectedDate);
        } catch {
          toast.error("Failed to create day for the place");
          setIsSubmitting(false);
          setIsDialogOpen(false);
          return;
        }
      }

      if (!dayIdToUse) {
        toast.error("Unable to add place - no day available");
        setIsSubmitting(false);
        setIsDialogOpen(false);
        return;
      }

      if (onAddPlace) {
        await onAddPlace(dayIdToUse, request);
        if (day) {
          // Only show success message if day already existed
          toast.success("Place added successfully");
        }
      } else {
        // Fallback to direct API call if optimistic update is not available
        await api.addPlaceToDay(dayIdToUse, request);
        onPlacesChange?.();
        if (day) {
          // Only show success message if day already existed
          toast.success("Place added successfully");
        }
      }
    } catch {
      toast.error("Failed to add place");
    } finally {
      setIsSubmitting(false);
      setIsDialogOpen(false);
    }
  };

  const handleEditPlace = (placeId: string) => {
    setEditingPlaceId(placeId);
    setIsEditDialogOpen(true);
  };

  const handleUpdatePlace = async (request: UpdatePlaceRequest) => {
    if (!editingPlaceId) return;

    setIsSubmitting(true);
    try {
      if (onPlaceUpdate) {
        await onPlaceUpdate(editingPlaceId, request);
        toast.success("Place updated successfully");
      } else {
        // Fallback to direct API call if optimistic update is not available
        await api.updatePlace(editingPlaceId, request);
        onPlacesChange?.();
        toast.success("Place updated successfully");
      }
    } catch {
      toast.error("Failed to update place");
    } finally {
      setIsSubmitting(false);
      setIsEditDialogOpen(false);
      setEditingPlaceId(null);
    }
  };

  // Places are always added to the top (order 1), then reordered

  return (
    <>
      <Card className="h-full flex flex-col">
        <CardHeader className="pb-2">
          <div className="flex items-center justify-between">
            <CardTitle className="text-sm">{formattedDate}</CardTitle>
            {day?.places && day.places.length > 0 && (
              <div className="flex items-center gap-2">
                <span className="text-xs text-muted-foreground">Places ({day.places.length})</span>
                <Button onClick={() => setIsDialogOpen(true)} size="sm" className="flex items-center gap-1 h-7">
                  <Plus className="h-3 w-3" />
                  Add Place
                </Button>
              </div>
            )}
          </div>
          {day?.notes && <p className="text-xs text-muted-foreground">{day.notes}</p>}
        </CardHeader>
        <CardContent className="flex-1 overflow-y-auto">
          <PlacesList
            dayId={day?.id || "temp"} // Use temp ID when no day exists yet
            places={day?.places || []}
            onPlacesChange={
              onPlacesChange ||
              (() => {
                /* empty */
              })
            }
            onPlaceVisitedChange={onPlaceVisitedChange}
            onOpenAddDialog={() => setIsDialogOpen(true)}
            onEditPlace={handleEditPlace}
            onDeletePlace={onDeletePlace}
            onReorderPlaces={onReorderPlaces}
            selectedDate={selectedDate}
          />
        </CardContent>
      </Card>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="sm:max-w-4xl max-h-[99vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>Add New Place</DialogTitle>
          </DialogHeader>
          <PlaceForm
            onSubmit={handleAddPlace}
            onCancel={() => setIsDialogOpen(false)}
            isSubmitting={isSubmitting}
            defaultLocation={tripDestination}
            selectedDate={selectedDate}
          />
        </DialogContent>
      </Dialog>

      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent className="sm:max-w-4xl">
          <DialogHeader>
            <DialogTitle>Edit Place Details</DialogTitle>
          </DialogHeader>
          <PlaceForm
            defaultPlace={editingPlaceId ? day?.places?.find((p) => p.place.id === editingPlaceId)?.place : undefined}
            onSubmit={handleAddPlace}
            onUpdate={handleUpdatePlace}
            onCancel={() => setIsEditDialogOpen(false)}
            isSubmitting={isSubmitting}
            isEditing={true}
            selectedDate={selectedDate}
          />
        </DialogContent>
      </Dialog>
    </>
  );
};

export default DayView;
