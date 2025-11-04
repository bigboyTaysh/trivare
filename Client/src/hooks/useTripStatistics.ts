import { useState, useEffect } from "react";
import {
  getTrip,
  getTransports,
  getDays,
  getTripFiles,
  getAccommodationFiles,
  getTransportFiles,
} from "@/services/api";
import type { TransportResponse, DayWithPlacesDto, FileDto } from "@/types/trips";

export interface TripStatistics {
  // Basic counts
  dayCount: number;
  placeCount: number;
  transportCount: number;
  fileCount: number;

  // Duration
  durationInDays: number;
  daysCompleted?: number;
  daysRemaining?: number;
  progressPercentage?: number;

  // Status flags
  hasAccommodation: boolean;
  hasTransports: boolean;
  hasPlaces: boolean;
  hasFiles: boolean;

  // Detailed data for advanced stats
  transports: TransportResponse[];
  days: DayWithPlacesDto[];
  files: FileDto[];
}

/**
 * Hook to fetch and compute statistics for a specific trip
 * Used to enhance trip cards with additional details
 */
export function useTripStatistics(tripId: string) {
  const [statistics, setStatistics] = useState<TripStatistics | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    const fetchStatistics = async () => {
      try {
        setIsLoading(true);
        setError(null);

        // Fetch all trip-related data in parallel
        const [tripDetail, transports, days, tripFiles] = await Promise.all([
          getTrip(tripId),
          getTransports(tripId).catch(() => []), // Handle cases where no transports exist
          getDays(tripId).catch(() => []), // Handle cases where no days exist
          getTripFiles(tripId).catch(() => []), // Handle cases where no files exist
        ]);

        // Fetch accommodation files if accommodation exists
        let accommodationFiles: FileDto[] = [];
        if (tripDetail.accommodation?.id) {
          accommodationFiles = await getAccommodationFiles(tripDetail.accommodation.id).catch(() => []);
        }

        // Fetch transport files for all transports
        const transportFilesPromises = transports.map((transport) => getTransportFiles(transport.id).catch(() => []));
        const transportFilesArrays = await Promise.all(transportFilesPromises);
        const allTransportFiles = transportFilesArrays.flat();

        // Combine all files
        const allFiles = [...tripFiles, ...accommodationFiles, ...allTransportFiles];

        // Calculate basic statistics
        const dayCount = days.filter((day) => day.places && day.places.length > 0).length; // Only count days with places
        const placeCount = days.reduce((total, day) => total + (day.places?.length || 0), 0);
        const transportCount = transports.length;
        const fileCount = allFiles.length;

        // Calculate duration
        const startDate = new Date(tripDetail.startDate);
        const endDate = new Date(tripDetail.endDate);
        const durationInDays = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)) + 1;

        // Calculate progress for ongoing trips
        let daysCompleted: number | undefined;
        let daysRemaining: number | undefined;
        let progressPercentage: number | undefined;

        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (startDate <= today && endDate >= today) {
          // Trip is ongoing
          const startOfTrip = new Date(startDate);
          startOfTrip.setHours(0, 0, 0, 0);

          daysCompleted = Math.ceil((today.getTime() - startOfTrip.getTime()) / (1000 * 60 * 60 * 24));
          daysRemaining = durationInDays - daysCompleted;
          progressPercentage = Math.min(100, Math.max(0, (daysCompleted / durationInDays) * 100));
        } else if (endDate < today) {
          // Trip is past
          daysCompleted = durationInDays;
          daysRemaining = 0;
          progressPercentage = 100;
        }
        // Future trips don't have progress

        const stats: TripStatistics = {
          dayCount,
          placeCount,
          transportCount,
          fileCount,
          durationInDays,
          daysCompleted,
          daysRemaining,
          progressPercentage,
          hasAccommodation: !!tripDetail.accommodation,
          hasTransports: transportCount > 0,
          hasPlaces: placeCount > 0,
          hasFiles: fileCount > 0,
          transports,
          days,
          files: allFiles,
        };

        setStatistics(stats);
      } catch (err) {
        setError(err instanceof Error ? err : new Error("Failed to load trip statistics"));
      } finally {
        setIsLoading(false);
      }
    };

    if (tripId) {
      fetchStatistics();
    }
  }, [tripId]);

  return { statistics, isLoading, error };
}
