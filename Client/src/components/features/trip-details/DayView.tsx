import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus, Calendar } from "lucide-react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { PlaceForm } from "../../forms/PlaceForm";
import { api } from "@/services/api";
import { toast } from "sonner";
import type { DayWithPlacesDto, AddPlaceRequest } from "@/types/trips";
import { PlacesList } from "./PlacesList";

interface DayViewProps {
  day: DayWithPlacesDto | null;
  selectedDate?: Date;
  onAddDay?: (date: Date) => void;
  isLoading?: boolean;
  onPlacesChange?: () => void;
}

const DayView: React.FC<DayViewProps> = ({ day, selectedDate, onAddDay, isLoading = false, onPlacesChange }) => {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
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

  if (!day) {
    // If no day exists but a date is selected, show option to add the day
    if (selectedDate) {
      const formattedDate = selectedDate.toLocaleDateString("en-US", {
        weekday: "long",
        year: "numeric",
        month: "long",
        day: "numeric",
      });

      return (
        <Card className="h-full flex flex-col">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm">{formattedDate}</CardTitle>
          </CardHeader>
          <CardContent className="flex-1 flex items-center justify-center">
            <div className="flex flex-col items-center justify-center">
              <Calendar className="h-8 w-8 text-gray-400 mb-2" />
              <h3 className="text-sm font-medium text-gray-900 mb-1.5">No trip day yet</h3>
              <p className="text-gray-500 text-center text-xs mb-2">
                Add this day to your trip itinerary to start planning activities and places to visit.
              </p>
              <Button onClick={() => onAddDay?.(selectedDate)} className="flex items-center gap-1 h-7">
                <Plus className="h-3 w-3" />
                Add Day
              </Button>
            </div>
          </CardContent>
        </Card>
      );
    }

    return (
      <Card className="h-full">
        <CardContent className="flex items-center justify-center h-full">
          <p className="text-gray-500">Select a day to view details</p>
        </CardContent>
      </Card>
    );
  }

  const formattedDate = new Date(day.date).toLocaleDateString("en-US", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  const handleAddPlace = async (request: AddPlaceRequest) => {
    setIsSubmitting(true);
    try {
      await api.addPlaceToDay(day.id, request);
      onPlacesChange?.();
      toast.success("Place added successfully");
    } catch (error) {
      console.error("Failed to add place:", error);
      toast.error("Failed to add place");
    } finally {
      setIsSubmitting(false);
      setIsDialogOpen(false);
    }
  };

  // Places are always added to the top (order 1), then reordered

  return (
    <>
      <Card className="h-full flex flex-col">
        <CardHeader className="pb-2">
          <CardTitle className="text-sm">{formattedDate}</CardTitle>
          {day.notes && <p className="text-xs text-gray-600">{day.notes}</p>}
        </CardHeader>
        <CardContent className="flex-1 overflow-y-auto">
          <PlacesList
            dayId={day.id}
            places={day.places}
            onPlacesChange={
              onPlacesChange ||
              (() => {
                /* empty */
              })
            }
            onOpenAddDialog={() => setIsDialogOpen(true)}
          />
        </CardContent>
      </Card>

      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Add New Place</DialogTitle>
          </DialogHeader>
          <PlaceForm onSubmit={handleAddPlace} onCancel={() => setIsDialogOpen(false)} isSubmitting={isSubmitting} />
        </DialogContent>
      </Dialog>
    </>
  );
};

export default DayView;
