import React, { useState } from "react";
import { z } from "zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { toast } from "sonner";
import { Search, Plus } from "lucide-react";
import { api } from "@/services/api";
import { PlaceSearchResults } from "../features/trip-details/PlaceSearchResults";
import type { AddPlaceRequest, UpdatePlaceRequest, PlaceDto } from "@/types/trips";

interface PlaceFormProps {
  defaultPlace?: Partial<AddPlaceRequest["place"]> & { id?: string };
  defaultLocation?: string; // Default location from trip destination
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
  defaultLocation,
  onSubmit,
  onUpdate,
  onCancel,
  isSubmitting,
  isEditing = false,
}: PlaceFormProps) {
  const [mode, setMode] = useState<"search" | "manual">(isEditing ? "manual" : "search");
  const [searchLocation, setSearchLocation] = useState(defaultLocation || "");
  const [searchKeyword, setSearchKeyword] = useState("");
  const [searchPreferences, setSearchPreferences] = useState("");
  const [searchResults, setSearchResults] = useState<PlaceDto[]>([]);
  const [isSearching, setIsSearching] = useState(false);

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

  const handleSearch = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!searchLocation.trim() || !searchKeyword.trim()) {
      toast.error("Location and keyword are required");
      return;
    }

    setIsSearching(true);
    try {
      const response = await api.searchPlaces({
        location: searchLocation,
        keyword: searchKeyword,
        preferences: searchPreferences || undefined,
      });
      setSearchResults(response.results);

      if (response.count === 0) {
        toast.info("No places found. Try different search terms.");
      }
    } catch (error) {
      console.error("Search error:", error);
      toast.error("Failed to search places");
    } finally {
      setIsSearching(false);
    }
  };

  const handleSelectPlace = async (place: PlaceDto) => {
    const request: AddPlaceRequest = {
      order: 1,
      place: {
        googlePlaceId: place.googlePlaceId,
        name: place.name,
        formattedAddress: place.formattedAddress,
        website: place.website,
        googleMapsLink: place.googleMapsLink,
        openingHoursText: place.openingHoursText,
        photoReference: place.photoReference,
      },
    };

    try {
      await onSubmit(request);
    } catch (error) {
      console.error("Error selecting place:", error);
      toast.error("Failed to add place");
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
          order: 1,
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

  // Show only manual form in edit mode
  if (isEditing) {
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
            {isSubmitting ? "Updating..." : "Update Place"}
          </Button>
        </div>
      </form>
    );
  }

  return (
    <div className="space-y-6">
      {/* Mode Selector */}
      <div className="flex gap-2 border-b">
        <button
          type="button"
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            mode === "search"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
          onClick={() => setMode("search")}
        >
          <Search className="inline h-4 w-4 mr-1" />
          Search Places
        </button>
        <button
          type="button"
          className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${
            mode === "manual"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
          onClick={() => setMode("manual")}
        >
          <Plus className="inline h-4 w-4 mr-1" />
          Add Manually
        </button>
      </div>

      {/* Search Mode */}
      {mode === "search" && (
        <div className="space-y-4">
          <form onSubmit={handleSearch} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="searchLocation">
                Location <span className="text-destructive">*</span>
              </Label>
              <Input
                id="searchLocation"
                type="text"
                value={searchLocation}
                onChange={(e) => setSearchLocation(e.target.value)}
                placeholder="e.g., Paris, France"
                disabled={isSearching}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="searchKeyword">
                What are you looking for? <span className="text-destructive">*</span>
              </Label>
              <Input
                id="searchKeyword"
                type="text"
                value={searchKeyword}
                onChange={(e) => setSearchKeyword(e.target.value)}
                placeholder="e.g., breakfast with coffee, museum, restaurant"
                disabled={isSearching}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="searchPreferences">Additional Preferences (optional)</Label>
              <Input
                id="searchPreferences"
                type="text"
                value={searchPreferences}
                onChange={(e) => setSearchPreferences(e.target.value)}
                placeholder="e.g., vegetarian-friendly, outdoor seating"
                disabled={isSearching}
              />
            </div>

            <Button type="submit" disabled={isSearching} className="w-full">
              <Search className="h-4 w-4 mr-2" />
              {isSearching ? "Searching..." : "Search"}
            </Button>
          </form>

          <PlaceSearchResults results={searchResults} isLoading={isSearching} onSelect={handleSelectPlace} />
        </div>
      )}

      {/* Manual Mode */}
      {mode === "manual" && (
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
              {isSubmitting ? "Adding..." : "Add Place"}
            </Button>
          </div>
        </form>
      )}
    </div>
  );
}
