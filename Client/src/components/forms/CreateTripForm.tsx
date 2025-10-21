import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { createTrip } from "@/services/api";
import type { CreateTripRequest } from "@/types/trips";
import { toast } from "sonner";

/**
 * Form for creating a new trip
 * Handles validation and API submission
 */
export function CreateTripForm() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formData, setFormData] = useState<CreateTripRequest>({
    name: "",
    destination: "",
    startDate: "",
    endDate: "",
    notes: "",
  });
  const [errors, setErrors] = useState<Partial<Record<keyof CreateTripRequest, string>>>({});

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof CreateTripRequest, string>> = {};

    // Validate name
    if (!formData.name.trim()) {
      newErrors.name = "Trip name is required";
    } else if (formData.name.length > 255) {
      newErrors.name = "Trip name cannot exceed 255 characters";
    }

    // Validate destination (optional but has max length)
    if (formData.destination && formData.destination.length > 255) {
      newErrors.destination = "Destination cannot exceed 255 characters";
    }

    // Validate start date
    if (!formData.startDate) {
      newErrors.startDate = "Start date is required";
    }

    // Validate end date
    if (!formData.endDate) {
      newErrors.endDate = "End date is required";
    } else if (formData.startDate && formData.endDate < formData.startDate) {
      newErrors.endDate = "End date must be on or after start date";
    }

    // Validate notes (optional but has max length)
    if (formData.notes && formData.notes.length > 2000) {
      newErrors.notes = "Notes cannot exceed 2000 characters";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);

    try {
      // Prepare request data (remove empty optional fields)
      const requestData: CreateTripRequest = {
        name: formData.name.trim(),
        startDate: formData.startDate,
        endDate: formData.endDate,
        ...(formData.destination?.trim() && { destination: formData.destination.trim() }),
        ...(formData.notes?.trim() && { notes: formData.notes.trim() }),
      };

      const response = await createTrip(requestData);

      toast.success("Trip created successfully!");

      // Redirect to the new trip's detail page
      window.location.href = `/trips/${response.id}`;
    } catch (error) {
      console.error("Failed to create trip:", error);
      toast.error(error instanceof Error ? error.message : "Failed to create trip. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleInputChange = (field: keyof CreateTripRequest, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleCancel = () => {
    window.location.href = "/";
  };

  return (
    <Card className="max-w-2xl mx-auto">
      <CardHeader>
        <CardTitle>Create New Trip</CardTitle>
        <CardDescription>Plan your next adventure by adding trip details below</CardDescription>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Trip Name */}
          <div className="space-y-2">
            <Label htmlFor="name">
              Trip Name <span className="text-destructive">*</span>
            </Label>
            <Input
              id="name"
              type="text"
              value={formData.name}
              onChange={(e) => handleInputChange("name", e.target.value)}
              placeholder="e.g., Summer Vacation 2025"
              disabled={isSubmitting}
              className={errors.name ? "border-destructive" : ""}
              maxLength={255}
            />
            {errors.name && <p className="text-sm text-destructive">{errors.name}</p>}
          </div>

          {/* Destination */}
          <div className="space-y-2">
            <Label htmlFor="destination">Destination</Label>
            <Input
              id="destination"
              type="text"
              value={formData.destination}
              onChange={(e) => handleInputChange("destination", e.target.value)}
              placeholder="e.g., Paris, France"
              disabled={isSubmitting}
              className={errors.destination ? "border-destructive" : ""}
              maxLength={255}
            />
            {errors.destination && <p className="text-sm text-destructive">{errors.destination}</p>}
          </div>

          {/* Date Range */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="startDate">
                Start Date <span className="text-destructive">*</span>
              </Label>
              <Input
                id="startDate"
                type="date"
                value={formData.startDate}
                onChange={(e) => handleInputChange("startDate", e.target.value)}
                disabled={isSubmitting}
                className={errors.startDate ? "border-destructive" : ""}
              />
              {errors.startDate && <p className="text-sm text-destructive">{errors.startDate}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="endDate">
                End Date <span className="text-destructive">*</span>
              </Label>
              <Input
                id="endDate"
                type="date"
                value={formData.endDate}
                onChange={(e) => handleInputChange("endDate", e.target.value)}
                disabled={isSubmitting}
                className={errors.endDate ? "border-destructive" : ""}
                min={formData.startDate}
              />
              {errors.endDate && <p className="text-sm text-destructive">{errors.endDate}</p>}
            </div>
          </div>

          {/* Notes */}
          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Textarea
              id="notes"
              value={formData.notes}
              onChange={(e) => handleInputChange("notes", e.target.value)}
              placeholder="Add any additional notes about your trip..."
              disabled={isSubmitting}
              className={errors.notes ? "border-destructive" : ""}
              rows={4}
              maxLength={2000}
            />
            <div className="flex justify-between items-center">
              {errors.notes && <p className="text-sm text-destructive">{errors.notes}</p>}
              <p className="text-xs text-muted-foreground ml-auto">{formData.notes?.length || 0}/2000</p>
            </div>
          </div>

          {/* Form Actions */}
          <div className="flex gap-4 justify-end pt-4">
            <Button type="button" variant="outline" onClick={handleCancel} disabled={isSubmitting}>
              Cancel
            </Button>
            <Button type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Creating..." : "Create Trip"}
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}
