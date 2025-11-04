import { useState, useEffect, useCallback } from "react";
import type { AccommodationDto, AddAccommodationRequest, UpdateAccommodationRequest } from "@/types/trips";
import { api } from "@/services/api";
import { toast } from "sonner";

interface UseAccommodationReturn {
  accommodation: AccommodationDto | null;
  isLoading: boolean;
  error: Error | null;
  addAccommodation: (data: AddAccommodationRequest) => Promise<void>;
  updateAccommodation: (data: UpdateAccommodationRequest) => Promise<void>;
  deleteAccommodation: () => Promise<void>;
  refreshAccommodation: () => Promise<void>;
}

export const useAccommodation = (tripId: string): UseAccommodationReturn => {
  const [accommodation, setAccommodation] = useState<AccommodationDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const refreshAccommodation = useCallback(async () => {
    if (!tripId) return;

    try {
      setIsLoading(true);
      setError(null);
      // Get the full trip details and extract accommodation
      const tripDetails = await api.getTrip(tripId);
      setAccommodation(tripDetails.accommodation || null);
    } catch (err) {
      setError(err as Error);
      toast.error("Failed to load accommodation");
    } finally {
      setIsLoading(false);
    }
  }, [tripId]);

  useEffect(() => {
    refreshAccommodation();
  }, [refreshAccommodation]);

  const addAccommodationHandler = async (data: AddAccommodationRequest) => {
    try {
      setIsLoading(true);
      const newAccommodation = await api.addAccommodation(tripId, data);
      setAccommodation(newAccommodation);
      toast.success("Accommodation added successfully");
    } catch (err) {
      toast.error("Failed to add accommodation");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateAccommodationHandler = async (data: UpdateAccommodationRequest) => {
    try {
      setIsLoading(true);
      const updatedAccommodation = await api.updateAccommodation(tripId, data);
      setAccommodation(updatedAccommodation);
      toast.success("Accommodation updated successfully");
    } catch (err) {
      toast.error("Failed to update accommodation");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteAccommodationHandler = async () => {
    try {
      setIsLoading(true);
      await api.deleteAccommodation(tripId);
      setAccommodation(null);
      toast.success("Accommodation deleted successfully");
    } catch (err) {
      toast.error("Failed to delete accommodation");
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    accommodation,
    isLoading,
    error,
    addAccommodation: addAccommodationHandler,
    updateAccommodation: updateAccommodationHandler,
    deleteAccommodation: deleteAccommodationHandler,
    refreshAccommodation,
  };
};
