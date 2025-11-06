import { useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import type { AddAccommodationRequest, UpdateAccommodationRequest, AccommodationDto } from "@/types/trips";

interface AccommodationFormProps {
  accommodation?: AccommodationDto | null;
  onSubmit: (data: AddAccommodationRequest | UpdateAccommodationRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
}

export function AccommodationForm({ accommodation, onSubmit, onCancel, isSubmitting }: AccommodationFormProps) {
  const isEditing = !!accommodation;

  // Helper function to convert UTC date to local datetime-local format
  const toLocalDateTimeString = (dateString: string): string => {
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const [formData, setFormData] = useState({
    name: accommodation?.name || "",
    address: accommodation?.address || "",
    checkInDate: accommodation?.checkInDate ? toLocalDateTimeString(accommodation.checkInDate) : "",
    checkOutDate: accommodation?.checkOutDate ? toLocalDateTimeString(accommodation.checkOutDate) : "",
    notes: accommodation?.notes || "",
  });

  const [errors, setErrors] = useState<Partial<Record<string, string>>>({});

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<string, string>> = {};

    // Validate name (optional but has max length)
    if (formData.name && formData.name.length > 255) {
      newErrors.name = "Name cannot exceed 255 characters";
    }

    // Validate address (optional but has max length)
    if (formData.address && formData.address.length > 500) {
      newErrors.address = "Address cannot exceed 500 characters";
    }

    // Validate check-in date (optional)
    // Validate check-out date (optional)
    if (formData.checkInDate && formData.checkOutDate) {
      if (new Date(formData.checkOutDate) < new Date(formData.checkInDate)) {
        newErrors.checkOutDate = "Check-out date must be on or after check-in date";
      }
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

    try {
      // Prepare request data
      let requestData: AddAccommodationRequest | UpdateAccommodationRequest;

      if (isEditing) {
        // For updates, send all fields including empty ones to allow clearing
        requestData = {
          name: formData.name?.trim() || "",
          address: formData.address?.trim() || "",
          checkInDate: formData.checkInDate || null,
          checkOutDate: formData.checkOutDate || null,
          notes: formData.notes?.trim() || "",
        };
      } else {
        // For adds, only send non-empty fields
        requestData = {
          ...(formData.name?.trim() && { name: formData.name.trim() }),
          ...(formData.address?.trim() && { address: formData.address.trim() }),
          ...(formData.checkInDate && { checkInDate: formData.checkInDate }),
          ...(formData.checkOutDate && { checkOutDate: formData.checkOutDate }),
          ...(formData.notes?.trim() && { notes: formData.notes.trim() }),
        };
      }

      await onSubmit(requestData);
    } catch {
      // Error is handled by the parent component
    }
  };

  const handleInputChange = (field: string, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error for this field when user starts typing
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Accommodation Name */}
      <div className="space-y-2">
        <Label htmlFor="name">Accommodation Name</Label>
        <Input
          id="name"
          type="text"
          value={formData.name}
          onChange={(e) => handleInputChange("name", e.target.value)}
          placeholder="e.g., Hotel Eiffel"
          disabled={isSubmitting}
          className={errors.name ? "border-destructive" : ""}
          maxLength={255}
        />
        {errors.name && <p className="text-sm text-destructive">{errors.name}</p>}
      </div>

      {/* Address */}
      <div className="space-y-2">
        <Label htmlFor="address">Address</Label>
        <Input
          id="address"
          type="text"
          value={formData.address}
          onChange={(e) => handleInputChange("address", e.target.value)}
          placeholder="e.g., 123 Main St, Paris, France"
          disabled={isSubmitting}
          className={errors.address ? "border-destructive" : ""}
          maxLength={500}
        />
        {errors.address && <p className="text-sm text-destructive">{errors.address}</p>}
      </div>

      {/* Check-in/Check-out Date and Time */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="checkInDate">Check-in Date & Time</Label>
          <Input
            id="checkInDate"
            type="datetime-local"
            value={formData.checkInDate}
            onChange={(e) => handleInputChange("checkInDate", e.target.value)}
            disabled={isSubmitting}
            className={errors.checkInDate ? "border-destructive" : ""}
          />
          {errors.checkInDate && <p className="text-sm text-destructive">{errors.checkInDate}</p>}
        </div>

        <div className="space-y-2">
          <Label htmlFor="checkOutDate">Check-out Date & Time</Label>
          <Input
            id="checkOutDate"
            type="datetime-local"
            value={formData.checkOutDate}
            onChange={(e) => handleInputChange("checkOutDate", e.target.value)}
            disabled={isSubmitting}
            className={errors.checkOutDate ? "border-destructive" : ""}
            min={formData.checkInDate}
          />
          {errors.checkOutDate && <p className="text-sm text-destructive">{errors.checkOutDate}</p>}
        </div>
      </div>

      {/* Notes */}
      <div className="space-y-2">
        <Label htmlFor="notes">Notes</Label>
        <Textarea
          id="notes"
          value={formData.notes}
          onChange={(e) => handleInputChange("notes", e.target.value)}
          placeholder="Add any additional notes about your accommodation..."
          disabled={isSubmitting}
          className={errors.notes ? "border-destructive" : ""}
          rows={3}
          maxLength={2000}
        />
        <div className="flex justify-between items-center">
          {errors.notes && <p className="text-sm text-destructive">{errors.notes}</p>}
          <p className="text-xs text-muted-foreground ml-auto">{formData.notes?.length || 0}/2000</p>
        </div>
      </div>

      {/* Form Actions */}
      <div className="flex gap-4 justify-end pt-4">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting
            ? isEditing
              ? "Updating..."
              : "Adding..."
            : isEditing
              ? "Update Accommodation"
              : "Add Accommodation"}
        </Button>
      </div>
    </form>
  );
}
