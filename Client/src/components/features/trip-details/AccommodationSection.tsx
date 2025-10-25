import React, { useState } from "react";
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
import { Skeleton } from "@/components/ui/skeleton";
import { Plus, Edit, Trash2, MapPin, Calendar, FileText } from "lucide-react";
import { useAccommodation } from "@/hooks/useAccommodation";
import { AccommodationForm } from "@/components/forms/AccommodationForm";
import type { AddAccommodationRequest, UpdateAccommodationRequest } from "@/types/trips";

interface AccommodationSectionProps {
  tripId: string;
}

const AccommodationSection: React.FC<AccommodationSectionProps> = ({ tripId }) => {
  const { accommodation, isLoading, addAccommodation, updateAccommodation, deleteAccommodation } =
    useAccommodation(tripId);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleAdd = async (data: AddAccommodationRequest) => {
    setIsSubmitting(true);
    try {
      await addAccommodation(data);
      setIsDialogOpen(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (data: UpdateAccommodationRequest) => {
    setIsSubmitting(true);
    try {
      await updateAccommodation(data);
      setIsDialogOpen(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    setIsSubmitting(true);
    try {
      await deleteAccommodation();
      setIsDeleteDialogOpen(false);
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return null;
    return new Date(dateString).toLocaleDateString();
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Accommodation
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="h-4 w-1/2" />
            <Skeleton className="h-4 w-2/3" />
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Accommodation
          </CardTitle>
          {accommodation ? (
            <div className="flex gap-2">
              <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
                <DialogTrigger asChild>
                  <Button variant="outline" size="sm">
                    <Edit className="h-4 w-4 mr-2" />
                    Edit
                  </Button>
                </DialogTrigger>
                <DialogContent className="max-w-2xl">
                  <DialogHeader>
                    <DialogTitle>Edit Accommodation</DialogTitle>
                  </DialogHeader>
                  <AccommodationForm
                    accommodation={accommodation}
                    onSubmit={handleUpdate}
                    onCancel={() => setIsDialogOpen(false)}
                    isSubmitting={isSubmitting}
                  />
                </DialogContent>
              </Dialog>
              <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
                <DialogTrigger asChild>
                  <Button variant="ghost" size="sm">
                    <Trash2 className="h-4 w-4" />
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
          ) : (
            <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
              <DialogTrigger asChild>
                <Button size="sm">
                  <Plus className="h-4 w-4 mr-2" />
                  Add Accommodation
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-2xl">
                <DialogHeader>
                  <DialogTitle>Add Accommodation</DialogTitle>
                </DialogHeader>
                <AccommodationForm
                  onSubmit={handleAdd}
                  onCancel={() => setIsDialogOpen(false)}
                  isSubmitting={isSubmitting}
                />
              </DialogContent>
            </Dialog>
          )}
        </div>
      </CardHeader>
      <CardContent>
        {accommodation ? (
          <div className="space-y-4">
            {accommodation.name && (
              <div>
                <h4 className="font-medium text-lg">{accommodation.name}</h4>
              </div>
            )}

            {accommodation.address && (
              <div className="flex items-start gap-2">
                <MapPin className="h-4 w-4 mt-0.5 text-muted-foreground" />
                <span className="text-sm">{accommodation.address}</span>
              </div>
            )}

            {(accommodation.checkInDate || accommodation.checkOutDate) && (
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <div className="flex gap-2 text-sm">
                  {accommodation.checkInDate && (
                    <span className="px-2 py-1 bg-secondary rounded-md">
                      Check-in: {formatDate(accommodation.checkInDate)}
                    </span>
                  )}
                  {accommodation.checkOutDate && (
                    <span className="px-2 py-1 bg-secondary rounded-md">
                      Check-out: {formatDate(accommodation.checkOutDate)}
                    </span>
                  )}
                </div>
              </div>
            )}

            {accommodation.notes && (
              <div className="pt-2 border-t">
                <p className="text-sm text-muted-foreground">{accommodation.notes}</p>
              </div>
            )}
          </div>
        ) : (
          <div className="text-center py-8">
            <FileText className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-medium mb-2">No accommodation added</h3>
            <p className="text-muted-foreground mb-4">Add your accommodation details to keep track of your stay.</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
};

export default AccommodationSection;
