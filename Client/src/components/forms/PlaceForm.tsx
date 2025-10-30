import React, { useState } from "react";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import type { AddPlaceRequest, UpdatePlaceRequest } from "@/types/trips";

interface PlaceFormProps {
  defaultPlace?: Partial<AddPlaceRequest["place"]>;
  onSubmit: (data: AddPlaceRequest) => Promise<void>;
  onUpdate?: (data: UpdatePlaceRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting: boolean;
  isEditing?: boolean;
}

const PlaceFormSchema = z.object({
  name: z.string().min(1, "Name is required").max(500, "Name cannot exceed 500 characters"),
  formattedAddress: z.string().max(500, "Address cannot exceed 500 characters").optional(),
  website: z.string().url("Must be a valid URL").optional().or(z.literal("")),
  googleMapsLink: z.string().url("Must be a valid URL").optional().or(z.literal("")),
  openingHoursText: z.string().max(1000, "Opening hours cannot exceed 1000 characters").optional(),
});

type PlaceFormData = z.infer<typeof PlaceFormSchema>;

export function PlaceForm({
  defaultPlace,
  onSubmit,
  onUpdate,
  onCancel,
  isSubmitting,
  isEditing = false,
}: PlaceFormProps) {
  const [formData, setFormData] = useState<PlaceFormData>(() => ({
    name: defaultPlace?.name || "",
    formattedAddress: defaultPlace?.formattedAddress || "",
    website: defaultPlace?.website || "",
    googleMapsLink: defaultPlace?.googleMapsLink || "",
    openingHoursText: defaultPlace?.openingHoursText || "",
  }));
  const [errors, setErrors] = useState<Partial<Record<keyof PlaceFormData, string>>>({});

  const handleInputChange = (field: keyof PlaceFormData, value: string | number) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error when user types
    if (errors[field]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const validatedData = PlaceFormSchema.parse(formData);

      if (isEditing && onUpdate) {
        const updateRequest: UpdatePlaceRequest = {
          name: validatedData.name,
          formattedAddress: validatedData.formattedAddress,
          website: validatedData.website || undefined,
          googleMapsLink: validatedData.googleMapsLink || undefined,
          openingHoursText: validatedData.openingHoursText || undefined,
        };
        await onUpdate(updateRequest);
      } else {
        const request: AddPlaceRequest = {
          order: 1, // Always add to the top of the list
          place: {
            name: validatedData.name,
            formattedAddress: validatedData.formattedAddress,
            website: validatedData.website || undefined,
            googleMapsLink: validatedData.googleMapsLink || undefined,
            openingHoursText: validatedData.openingHoursText || undefined,
          },
        };
        await onSubmit(request);
      }
      onCancel();
    } catch (error) {
      if (error instanceof z.ZodError) {
        const fieldErrors: Partial<Record<keyof PlaceFormData, string>> = {};
        error.errors.forEach((err) => {
          if (err.path.length > 0) {
            fieldErrors[err.path[0] as keyof PlaceFormData] = err.message;
          }
        });
        setErrors(fieldErrors);
        toast.error("Please fix the errors below");
      } else {
        toast.error("An unexpected error occurred");
      }
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Place Name */}
      <div className="space-y-2">
        <Label htmlFor="name">
          Place Name <span className="text-destructive">*</span>
        </Label>
        <Input
          id="name"
          type="text"
          value={formData.name}
          onChange={(e) => handleInputChange("name", e.target.value)}
          placeholder="e.g., Eiffel Tower"
          disabled={isSubmitting}
          className={errors.name ? "border-destructive" : ""}
          maxLength={500}
        />
        {errors.name && <p className="text-sm text-destructive">{errors.name}</p>}
      </div>

      {/* Formatted Address */}
      <div className="space-y-2">
        <Label htmlFor="formattedAddress">Address</Label>
        <Input
          id="formattedAddress"
          type="text"
          value={formData.formattedAddress}
          onChange={(e) => handleInputChange("formattedAddress", e.target.value)}
          placeholder="e.g., Champ de Mars, 5 Av. Anatole France, 75007 Paris, France"
          disabled={isSubmitting}
          className={errors.formattedAddress ? "border-destructive" : ""}
          maxLength={500}
        />
        {errors.formattedAddress && <p className="text-sm text-destructive">{errors.formattedAddress}</p>}
      </div>

      {/* Website */}
      <div className="space-y-2">
        <Label htmlFor="website">Website (optional)</Label>
        <Input
          id="website"
          type="url"
          value={formData.website}
          onChange={(e) => handleInputChange("website", e.target.value)}
          placeholder="https://example.com"
          disabled={isSubmitting}
          className={errors.website ? "border-destructive" : ""}
        />
        {errors.website && <p className="text-sm text-destructive">{errors.website}</p>}
      </div>

      {/* Google Maps Link */}
      <div className="space-y-2">
        <Label htmlFor="googleMapsLink">Google Maps Link (optional)</Label>
        <Input
          id="googleMapsLink"
          type="url"
          value={formData.googleMapsLink}
          onChange={(e) => handleInputChange("googleMapsLink", e.target.value)}
          placeholder="https://maps.google.com/..."
          disabled={isSubmitting}
          className={errors.googleMapsLink ? "border-destructive" : ""}
        />
        {errors.googleMapsLink && <p className="text-sm text-destructive">{errors.googleMapsLink}</p>}
      </div>

      {/* Opening Hours */}
      <div className="space-y-2">
        <Label htmlFor="openingHoursText">Opening Hours (optional)</Label>
        <Input
          id="openingHoursText"
          type="text"
          value={formData.openingHoursText}
          onChange={(e) => handleInputChange("openingHoursText", e.target.value)}
          placeholder="e.g., Mon-Fri 9AM-5PM, Sat-Sun 10AM-4PM"
          disabled={isSubmitting}
          className={errors.openingHoursText ? "border-destructive" : ""}
          maxLength={1000}
        />
        {errors.openingHoursText && <p className="text-sm text-destructive">{errors.openingHoursText}</p>}
      </div>

      {/* Actions */}
      <div className="flex justify-end space-x-2 pt-4">
        <Button type="button" variant="outline" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? (isEditing ? "Updating..." : "Adding...") : isEditing ? "Update Place" : "Add Place"}
        </Button>
      </div>
    </form>
  );
}
