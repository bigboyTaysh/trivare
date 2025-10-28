import React, { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import FilesSection from "@/components/common/FilesSection";
import type { TripDetailViewModel, UpdateTripRequest } from "@/types/trips";
import { UpdateTripViewModel, type UpdateTripViewModel as UpdateTripFormData } from "@/types/trips";
import { Edit, Trash2 } from "lucide-react";
import { formatDate } from "@/lib/dateUtils";

interface TripHeaderProps {
  trip: TripDetailViewModel;
  onUpdate: (data: UpdateTripRequest) => void;
  onDelete: () => void;
  totalFileCount: number;
  onFileChange: () => void;
}

const TripHeader: React.FC<TripHeaderProps> = ({ trip, onUpdate, onDelete, totalFileCount, onFileChange }) => {
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  const form = useForm<UpdateTripFormData>({
    resolver: zodResolver(UpdateTripViewModel),
    defaultValues: {
      name: trip.name,
      destination: trip.destination || "",
      startDate: trip.startDate,
      endDate: trip.endDate,
      notes: trip.notes || "",
    },
  });

  const onSubmit = async (data: UpdateTripFormData) => {
    try {
      await onUpdate(data);
      setIsEditMode(false);
    } catch {
      // Error handling is done in the parent component
    }
  };

  const handleEdit = () => {
    form.reset({
      name: trip.name,
      destination: trip.destination || "",
      startDate: trip.startDate,
      endDate: trip.endDate,
      notes: trip.notes || "",
    });
    setIsEditMode(true);
  };

  const handleCancel = () => {
    setIsEditMode(false);
    form.reset();
  };

  const handleDelete = () => {
    onDelete();
    setIsDeleteDialogOpen(false);
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex justify-between items-start">
          <div className="flex-1">
            {isEditMode ? (
              <Form {...form}>
                <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                  <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Trip Name</FormLabel>
                        <FormControl>
                          <Input {...field} className="text-2xl font-semibold" />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="destination"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Destination</FormLabel>
                          <FormControl>
                            <Input {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="grid grid-cols-2 gap-2">
                      <FormField
                        control={form.control}
                        name="startDate"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Start Date</FormLabel>
                            <FormControl>
                              <Input type="date" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />

                      <FormField
                        control={form.control}
                        name="endDate"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>End Date</FormLabel>
                            <FormControl>
                              <Input type="date" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  <FormField
                    control={form.control}
                    name="notes"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Notes</FormLabel>
                        <FormControl>
                          <Textarea {...field} rows={3} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <div className="flex gap-2">
                    <Button type="submit">Save Changes</Button>
                    <Button type="button" variant="outline" onClick={handleCancel}>
                      Cancel
                    </Button>
                  </div>
                </form>
              </Form>
            ) : (
              <>
                <CardTitle className="text-xl md:text-2xl">{trip.name}</CardTitle>
                <p className="text-sm text-muted-foreground mt-1">
                  {trip.destination && <>{trip.destination} • </>}
                  {formatDate(trip.startDate)} - {formatDate(trip.endDate)}
                </p>
              </>
            )}
          </div>
          <div className="flex gap-1">
            {!isEditMode && (
              <Button variant="outline" size="sm" onClick={handleEdit}>
                <Edit className="h-4 w-4 mr-1" />
                <span className="hidden sm:inline">Edit</span>
              </Button>
            )}
            <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
              <DialogTrigger asChild>
                <Button variant="ghost" size="sm">
                  <Trash2 className="h-4 w-4" />
                </Button>
              </DialogTrigger>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>Delete Trip</DialogTitle>
                  <DialogDescription>
                    Are you sure you want to delete &quot;{trip.name}&quot;? This action cannot be undone.
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
                    Cancel
                  </Button>
                  <Button variant="destructive" onClick={handleDelete}>
                    Delete
                  </Button>
                </DialogFooter>
              </DialogContent>
            </Dialog>
          </div>
        </div>
      </CardHeader>
      {!isEditMode && trip.notes && (
        <CardContent className="pt-0">
          <p className="text-sm text-muted-foreground">{trip.notes}</p>
        </CardContent>
      )}
      {!isEditMode && (
        <CardContent className="pt-0">
          <div className="pt-3 border-t">
            <FilesSection
              entityId={trip.id}
              entityType="trip"
              title="Trip Files"
              tripId={trip.id}
              totalFileCount={totalFileCount}
              onFileChange={onFileChange}
            />
          </div>
        </CardContent>
      )}
    </Card>
  );
};

export default TripHeader;
