import { useState, useEffect } from "react";
import {
  getTrips,
  getTrip,
  getTransports,
  getDays,
  getTripFiles,
  getAccommodationFiles,
  getTransportFiles,
} from "../services/api";
import type { DashboardViewModel, TripListDto, FileDto } from "../types/trips";
import type { TripStatistics } from "./useTripStatistics";

/**
 * Fetches statistics for a single trip
 */
async function fetchTripStatistics(tripId: string): Promise<TripStatistics> {
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
  const placeCount = days.reduce((total: number, day) => total + (day.places?.length || 0), 0);
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

  return stats;
}

/**
 * Custom hook to manage dashboard data fetching and categorization
 * Categorizes trips into Ongoing (current and future) and Past based on end date
 */
export function useDashboardData(): DashboardViewModel {
  const [state, setState] = useState<DashboardViewModel>({
    ongoingTrips: [],
    pastTrips: [],
    totalTripCount: 0,
    tripLimit: 10,
    isLoading: true,
    error: null,
    statisticsLoading: false,
  });

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        const response = await getTrips();
        const trips = response.data;

        // Categorize trips based on current date
        const today = new Date();
        today.setHours(0, 0, 0, 0); // Reset time to midnight for accurate date comparison

        const categorized = trips.reduce(
          (acc, trip) => {
            const endDate = new Date(trip.endDate);
            endDate.setHours(0, 0, 0, 0);

            // Ongoing: end date is today or in the future
            if (endDate >= today) {
              acc.ongoingTrips.push(trip);
            }
            // Past: end date is before today
            else {
              acc.pastTrips.push(trip);
            }

            return acc;
          },
          {
            ongoingTrips: [] as TripListDto[],
            pastTrips: [] as TripListDto[],
          }
        );

        setState({
          ...categorized,
          totalTripCount: trips.length,
          tripLimit: 10,
          isLoading: false,
          error: null,
          statisticsLoading: true,
        });

        // Fetch statistics for all trips (limit to first 10 for performance)
        const allTrips = [...categorized.ongoingTrips, ...categorized.pastTrips].slice(0, 10);
        if (allTrips.length > 0) {
          const statisticsPromises = allTrips.map((trip) => fetchTripStatistics(trip.id));
          const statisticsResults = await Promise.allSettled(statisticsPromises);

          // Update trips with their statistics
          const tripsWithStats = allTrips.map((trip, index) => {
            const result = statisticsResults[index];
            if (result.status === "fulfilled") {
              return { ...trip, statistics: result.value };
            }
            // If statistics fetch failed, return trip without statistics
            console.warn(`Failed to fetch statistics for trip ${trip.id}:`, result.reason);
            return trip;
          });

          // Split back into ongoing and past trips
          const ongoingTripsWithStats = categorized.ongoingTrips.map((trip) => {
            const tripWithStats = tripsWithStats.find((t) => t.id === trip.id);
            return tripWithStats || trip;
          });

          const pastTripsWithStats = categorized.pastTrips.map((trip) => {
            const tripWithStats = tripsWithStats.find((t) => t.id === trip.id);
            return tripWithStats || trip;
          });

          setState((prev) => ({
            ...prev,
            ongoingTrips: ongoingTripsWithStats,
            pastTrips: pastTripsWithStats,
            statisticsLoading: false,
          }));
        } else {
          setState((prev) => ({
            ...prev,
            statisticsLoading: false,
          }));
        }
      } catch (error) {
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: error instanceof Error ? error : new Error("Failed to load trips"),
          statisticsLoading: false,
        }));
      }
    };

    fetchDashboardData();
  }, []);

  return state;
}
