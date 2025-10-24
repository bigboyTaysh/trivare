import { useState, useEffect } from "react";
import type { TripDetailViewModel, UpdateTripRequest } from "@/types/trips";
import { api } from "@/services/api";
import { toast } from "sonner";

interface UseTripDetailsReturn {
  trip: TripDetailViewModel | null;
  isLoading: boolean;
  error: Error | null;
  updateTrip: (data: UpdateTripRequest) => Promise<void>;
  deleteTrip: () => Promise<void>;
}

export const useTripDetails = (tripId: string): UseTripDetailsReturn => {
  const [trip, setTrip] = useState<TripDetailViewModel | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchTrip = async () => {
      try {
        setIsLoading(true);
        const response = await api.getTrip(tripId);
        // Map TripDetailDto to TripDetailViewModel
        const tripData: TripDetailViewModel = {
          id: response.id,
          name: response.name,
          destination: response.destination || "",
          startDate: response.startDate,
          endDate: response.endDate,
          notes: response.notes,
          accommodation:
            response.accommodation && response.accommodation.name
              ? {
                  id: response.accommodation.id,
                  name: response.accommodation.name,
                  address: response.accommodation.address,
                  checkInDate: response.accommodation.checkInDate,
                  checkOutDate: response.accommodation.checkOutDate,
                  notes: response.accommodation.notes,
                }
              : undefined,
        };
        setTrip(tripData);
      } catch (err) {
        toast.error("Failed to load trip details");
        setError(err as Error);
      } finally {
        setIsLoading(false);
      }
    };

    if (tripId) {
      fetchTrip();
    }
  }, [tripId]);

  const updateTrip = async (data: UpdateTripRequest) => {
    try {
      const response = await api.updateTrip(tripId, data);
      // Map and update local state
      const updatedTrip: TripDetailViewModel = {
        id: response.id,
        name: response.name,
        destination: response.destination || "",
        startDate: response.startDate,
        endDate: response.endDate,
        notes: response.notes,
        accommodation:
          response.accommodation && response.accommodation.name
            ? {
                id: response.accommodation.id,
                name: response.accommodation.name,
                address: response.accommodation.address,
                checkInDate: response.accommodation.checkInDate,
                checkOutDate: response.accommodation.checkOutDate,
                notes: response.accommodation.notes,
              }
            : undefined,
      };
      setTrip(updatedTrip);
      toast.success("Trip updated successfully");
    } catch (err) {
      toast.error("Failed to update trip");
      throw err;
    }
  };

  const deleteTrip = async () => {
    try {
      await api.deleteTrip(tripId);
      toast.success("Trip deleted successfully");
      // Redirect to trips list
      if (typeof window !== "undefined") {
        window.location.href = "/trips";
      }
    } catch (err) {
      toast.error("Failed to delete trip");
      throw err;
    }
  };

  return {
    trip,
    isLoading,
    error,
    updateTrip,
    deleteTrip,
  };
};
