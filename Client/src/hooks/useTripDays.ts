import { useState, useEffect } from "react";
import type { DayWithPlacesDto, CreateDayRequest, UpdateDayRequest } from "@/types/trips";
import { api } from "@/services/api";
import { toast } from "sonner";

interface UseTripDaysReturn {
  days: DayWithPlacesDto[];
  isLoading: boolean;
  error: Error | null;
  createDay: (data: CreateDayRequest) => Promise<void>;
  updateDay: (dayId: string, data: UpdateDayRequest) => Promise<void>;
  refetch: () => Promise<void>;
}

export const useTripDays = (tripId: string): UseTripDaysReturn => {
  const [days, setDays] = useState<DayWithPlacesDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const fetchDays = async () => {
    try {
      setIsLoading(true);
      const response = await api.getDays(tripId);
      setDays(response);
    } catch (err) {
      toast.error("Failed to load trip days");
      setError(err as Error);
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (tripId) {
      fetchDays();
    }
  }, [tripId]);

  const createDay = async (data: CreateDayRequest) => {
    try {
      const newDay = await api.createDay(tripId, data);
      setDays((prev) => [...prev, newDay]);
      toast.success("Day added successfully");
    } catch (err) {
      toast.error("Failed to add day");
      throw err;
    }
  };

  const updateDay = async (dayId: string, data: UpdateDayRequest) => {
    try {
      const updatedDay = await api.updateDay(dayId, data);
      setDays((prev) => prev.map((day) => (day.id === dayId ? updatedDay : day)));
      toast.success("Day updated successfully");
    } catch (err) {
      toast.error("Failed to update day");
      throw err;
    }
  };

  const refetch = async () => {
    await fetchDays();
  };

  return {
    days,
    isLoading,
    error,
    createDay,
    updateDay,
    refetch,
  };
};
