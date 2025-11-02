import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { Skeleton } from "@/components/ui/skeleton";
import { Plus, Edit, Trash2, MapPin, Calendar, FileText } from "lucide-react";
import { AccommodationForm } from "@/components/forms/AccommodationForm";
import FilesSection from "@/components/common/FilesSection";
import type { AddAccommodationRequest, UpdateAccommodationRequest, AccommodationDto } from "@/types/trips";
import { formatDateTime } from "@/lib/dateUtils";
import { toast } from "sonner";

interface AccommodationSectionProps {
  tripId: string;
  totalFileCount: number;
  onFileChange: () => void;
  accommodation: AccommodationDto | null | undefined;
  onAddAccommodation: (data: AddAccommodationRequest) => Promise<AccommodationDto>;
  onUpdateAccommodation: (data: UpdateAccommodationRequest) => Promise<AccommodationDto>;
  onDeleteAccommodation: () => Promise<void>;
  isLoading: boolean;
}

const AccommodationSection: React.FC<AccommodationSectionProps> = ({
  tripId,
  totalFileCount,
  onFileChange,
  accommodation,
  onAddAccommodation,
  onUpdateAccommodation,
  onDeleteAccommodation,
  isLoading,
}) => {
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleAdd = async (data: AddAccommodationRequest) => {
    setIsSubmitting(true);
    try {
      await onAddAccommodation(data);
      toast.success("Accommodation added successfully");
      setIsDialogOpen(false);
    } catch (err) {
      toast.error("Failed to add accommodation");
      console.error("Failed to add accommodation:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (data: UpdateAccommodationRequest) => {
    setIsSubmitting(true);
    try {
      await onUpdateAccommodation(data);
      toast.success("Accommodation updated successfully");
      setIsDialogOpen(false);
    } catch (err) {
      toast.error("Failed to update accommodation");
      console.error("Failed to update accommodation:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    setIsSubmitting(true);
    try {
      await onDeleteAccommodation();
      toast.success("Accommodation deleted successfully");
      setIsDeleteDialogOpen(false);
    } catch (err) {
      toast.error("Failed to delete accommodation");
      console.error("Failed to delete accommodation:", err);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-32 w-full" />
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Add Accommodation Button */}
      <div className="flex justify-end">
        <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
          <DialogTrigger asChild>
            <Button size="sm">
              <Plus className="h-4 w-4 mr-2" />
              Add Accommodation
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>{accommodation ? "Edit Accommodation" : "Add Accommodation"}</DialogTitle>
            </DialogHeader>
            <AccommodationForm
              accommodation={accommodation || undefined}
              onSubmit={accommodation ? handleUpdate : handleAdd}
              onCancel={() => setIsDialogOpen(false)}
              isSubmitting={isSubmitting}
            />
          </DialogContent>
        </Dialog>
      </div>

      {/* Accommodation Card */}
      {accommodation ? (
        <Card className="py-[5px] flex flex-col">
          <CardContent className="py-[5px] flex-1 flex flex-col">
            <div>
              <div className="flex items-start justify-between mb-2">
                <div className="flex-1 min-w-0">
                  {accommodation.name && <h4 className="font-medium text-base truncate">{accommodation.name}</h4>}
                </div>
                <div className="flex gap-1 shrink-0 ml-2">
                  <Button variant="outline" size="sm" onClick={() => setIsDialogOpen(true)} className="h-7 px-2">
                    <Edit className="h-3 w-3 mr-1" />
                    <span className="text-xs">Edit</span>
                  </Button>
                  <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
                    <DialogTrigger asChild>
                      <Button variant="ghost" size="sm" className="h-7 w-7 p-0">
                        <Trash2 className="h-3 w-3" />
                      </Button>
                    </DialogTrigger>
                    <DialogContent>
                      <DialogHeader>
                        <DialogTitle>Delete Accommodation</DialogTitle>
                        <DialogDescription>
                          Are you sure you want to delete this accommodation? This action cannot be undone.
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

              <div className="space-y-2 mb-2">
                {accommodation.address && (
                  <div className="flex items-start gap-2">
                    <MapPin className="h-3.5 w-3.5 mt-0.5 text-muted-foreground shrink-0" />
                    <div className="min-w-0 flex-1">
                      <div className="text-xs truncate">{accommodation.address}</div>
                    </div>
                  </div>
                )}

                {accommodation.checkInDate && (
                  <div className="flex items-start gap-2">
                    <Calendar className="h-3.5 w-3.5 mt-0.5 text-muted-foreground shrink-0" />
                    <div className="min-w-0 flex-1">
                      <div className="text-xs font-medium">Check-in</div>
                      <div className="text-xs">{formatDateTime(accommodation.checkInDate)}</div>
                    </div>
                  </div>
                )}

                {accommodation.checkOutDate && (
                  <div className="flex items-start gap-2">
                    <Calendar className="h-3.5 w-3.5 mt-0.5 text-muted-foreground shrink-0" />
                    <div className="min-w-0 flex-1">
                      <div className="text-xs font-medium">Check-out</div>
                      <div className="text-xs">{formatDateTime(accommodation.checkOutDate)}</div>
                    </div>
                  </div>
                )}
              </div>

              {accommodation.notes && (
                <div className="pt-1.5 border-t mb-1.5">
                  <p className="text-xs text-muted-foreground line-clamp-2">{accommodation.notes}</p>
                </div>
              )}
            </div>

            {/* Files Section - pinned to bottom */}
            <div className="pt-1.5 border-t mt-auto">
              <FilesSection
                entityId={accommodation.id}
                entityType="accommodation"
                title="Accommodation Files"
                tripId={tripId}
                totalFileCount={totalFileCount}
                onFileChange={onFileChange}
              />
            </div>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent className="pt-6">
            <div className="text-center py-8">
              <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
              <h3 className="text-lg font-medium mb-2">No accommodation added</h3>
              <p className="text-muted-foreground">Add your accommodation details to keep track of your stay.</p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};

export default AccommodationSection;
