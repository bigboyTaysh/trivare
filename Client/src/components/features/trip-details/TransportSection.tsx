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
import { Plus, Edit, Trash2, MapPin, Calendar, Plane } from "lucide-react";
import { useTripDetails } from "@/hooks/useTripDetails";
import FilesSection from "@/components/common/FilesSection";
import type { CreateTransportRequest, UpdateTransportRequest, TransportViewModel } from "@/types/trips";
import { api } from "@/services/api";
import { toast } from "sonner";

interface TransportSectionProps {
  tripId: string;
}

const TransportSection: React.FC<TransportSectionProps> = ({ tripId }) => {
  const { trip, isLoading } = useTripDetails(tripId);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [editingTransport, setEditingTransport] = useState<TransportViewModel | null>(null);
  const [deletingTransportId, setDeletingTransportId] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const transports = trip?.transports || [];

  const handleAdd = async (data: CreateTransportRequest | UpdateTransportRequest) => {
    const createData = data as CreateTransportRequest;
    setIsSubmitting(true);
    try {
      await api.addTransport(tripId, createData);
      // Refresh trip details to get updated transports
      window.location.reload(); // Simple refresh for now
      setIsDialogOpen(false);
      toast.success("Transport added successfully");
    } catch {
      toast.error("Failed to add transport");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleUpdate = async (data: CreateTransportRequest | UpdateTransportRequest) => {
    if (!editingTransport) return;

    const updateData = data as UpdateTransportRequest;
    setIsSubmitting(true);
    try {
      await api.updateTransport(editingTransport.id, updateData);
      // Refresh trip details to get updated transports
      window.location.reload(); // Simple refresh for now
      setIsDialogOpen(false);
      setEditingTransport(null);
      toast.success("Transport updated successfully");
    } catch {
      toast.error("Failed to update transport");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!deletingTransportId) return;

    setIsSubmitting(true);
    try {
      await api.deleteTransport(deletingTransportId);
      // Refresh trip details to get updated transports
      window.location.reload(); // Simple refresh for now
      setIsDeleteDialogOpen(false);
      setDeletingTransportId(null);
      toast.success("Transport deleted successfully");
    } catch {
      toast.error("Failed to delete transport");
    } finally {
      setIsSubmitting(false);
    }
  };

  const formatDateTime = (dateString?: string) => {
    if (!dateString) return null;
    return new Date(dateString).toLocaleString();
  };

  const openEditDialog = (transport: TransportViewModel) => {
    setEditingTransport(transport);
    setIsDialogOpen(true);
  };

  const openDeleteDialog = (transportId: string) => {
    setDeletingTransportId(transportId);
    setIsDeleteDialogOpen(true);
  };

  const closeDialog = () => {
    setIsDialogOpen(false);
    setEditingTransport(null);
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Plane className="h-5 w-5" />
            Transport
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
            <Plane className="h-5 w-5" />
            Transport
          </CardTitle>
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button size="sm">
                <Plus className="h-4 w-4 mr-2" />
                Add Transport
              </Button>
            </DialogTrigger>
            <DialogContent className="max-w-2xl">
              <DialogHeader>
                <DialogTitle>{editingTransport ? "Edit Transport" : "Add Transport"}</DialogTitle>
              </DialogHeader>
              <TransportForm
                transport={editingTransport}
                onSubmit={editingTransport ? handleUpdate : handleAdd}
                onCancel={closeDialog}
                isSubmitting={isSubmitting}
              />
            </DialogContent>
          </Dialog>
        </div>
      </CardHeader>
      <CardContent>
        {transports.length > 0 ? (
          <div className="space-y-6">
            {transports.map((transport) => (
              <div key={transport.id} className="border rounded-lg p-4">
                <div className="flex items-start justify-between mb-4">
                  <div className="flex-1">
                    <h4 className="font-medium text-lg">{transport.type}</h4>
                  </div>
                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" onClick={() => openEditDialog(transport)}>
                      <Edit className="h-4 w-4 mr-2" />
                      Edit
                    </Button>
                    <Button variant="ghost" size="sm" onClick={() => openDeleteDialog(transport.id)}>
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-4">
                  {transport.departureLocation && (
                    <div className="flex items-start gap-2">
                      <MapPin className="h-4 w-4 mt-0.5 text-muted-foreground" />
                      <div>
                        <div className="text-sm font-medium">From</div>
                        <div className="text-sm">{transport.departureLocation}</div>
                      </div>
                    </div>
                  )}

                  {transport.arrivalLocation && (
                    <div className="flex items-start gap-2">
                      <MapPin className="h-4 w-4 mt-0.5 text-muted-foreground" />
                      <div>
                        <div className="text-sm font-medium">To</div>
                        <div className="text-sm">{transport.arrivalLocation}</div>
                      </div>
                    </div>
                  )}

                  {transport.departureTime && (
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <div>
                        <div className="text-sm font-medium">Departure</div>
                        <div className="text-sm">{formatDateTime(transport.departureTime)}</div>
                      </div>
                    </div>
                  )}

                  {transport.arrivalTime && (
                    <div className="flex items-center gap-2">
                      <Calendar className="h-4 w-4 text-muted-foreground" />
                      <div>
                        <div className="text-sm font-medium">Arrival</div>
                        <div className="text-sm">{formatDateTime(transport.arrivalTime)}</div>
                      </div>
                    </div>
                  )}
                </div>

                {transport.notes && (
                  <div className="pt-2 border-t">
                    <p className="text-sm text-muted-foreground">{transport.notes}</p>
                  </div>
                )}

                {/* Files Section */}
                <div className="pt-4 border-t">
                  <FilesSection entityId={transport.id} entityType="transport" title="Transport Files" />
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8">
            <Plane className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
            <h3 className="text-lg font-medium mb-2">No transports added</h3>
            <p className="text-muted-foreground mb-4">
              Add your transportation details to keep track of your journeys.
            </p>
          </div>
        )}
      </CardContent>

      {/* Delete Confirmation Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Delete Transport</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete this transport? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDeleteDialogOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleDelete} disabled={isSubmitting}>
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
};

// Simple inline form component - in a real app, this would be in a separate file
interface TransportFormProps {
  transport?: TransportViewModel | null;
  onSubmit: (data: CreateTransportRequest | UpdateTransportRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

const TransportForm: React.FC<TransportFormProps> = ({ transport, onSubmit, onCancel, isSubmitting }) => {
  const [formData, setFormData] = useState({
    type: transport?.type || "",
    departureLocation: transport?.departureLocation || "",
    arrivalLocation: transport?.arrivalLocation || "",
    departureTime: transport?.departureTime ? transport.departureTime.slice(0, 16) : "",
    arrivalTime: transport?.arrivalTime ? transport.arrivalTime.slice(0, 16) : "",
    notes: transport?.notes || "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const data = {
      ...formData,
      departureTime: formData.departureTime || undefined,
      arrivalTime: formData.arrivalTime || undefined,
      notes: formData.notes || undefined,
    };
    onSubmit(data);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="transport-type" className="block text-sm font-medium mb-1">
          Type *
        </label>
        <input
          id="transport-type"
          type="text"
          value={formData.type}
          onChange={(e) => setFormData({ ...formData, type: e.target.value })}
          className="w-full p-2 border rounded-md"
          required
          maxLength={100}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="departure-location" className="block text-sm font-medium mb-1">
            Departure Location
          </label>
          <input
            id="departure-location"
            type="text"
            value={formData.departureLocation}
            onChange={(e) => setFormData({ ...formData, departureLocation: e.target.value })}
            className="w-full p-2 border rounded-md"
            maxLength={255}
          />
        </div>
        <div>
          <label htmlFor="arrival-location" className="block text-sm font-medium mb-1">
            Arrival Location
          </label>
          <input
            id="arrival-location"
            type="text"
            value={formData.arrivalLocation}
            onChange={(e) => setFormData({ ...formData, arrivalLocation: e.target.value })}
            className="w-full p-2 border rounded-md"
            maxLength={255}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="departure-time" className="block text-sm font-medium mb-1">
            Departure Time
          </label>
          <input
            id="departure-time"
            type="datetime-local"
            value={formData.departureTime}
            onChange={(e) => setFormData({ ...formData, departureTime: e.target.value })}
            className="w-full p-2 border rounded-md"
          />
        </div>
        <div>
          <label htmlFor="arrival-time" className="block text-sm font-medium mb-1">
            Arrival Time
          </label>
          <input
            id="arrival-time"
            type="datetime-local"
            value={formData.arrivalTime}
            onChange={(e) => setFormData({ ...formData, arrivalTime: e.target.value })}
            className="w-full p-2 border rounded-md"
          />
        </div>
      </div>

      <div>
        <label htmlFor="transport-notes" className="block text-sm font-medium mb-1">
          Notes
        </label>
        <textarea
          id="transport-notes"
          value={formData.notes}
          onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          className="w-full p-2 border rounded-md"
          rows={3}
          maxLength={2000}
        />
      </div>

      <div className="flex justify-end gap-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Saving..." : transport ? "Update" : "Add"} Transport
        </Button>
      </div>
    </form>
  );
};

export default TransportSection;
