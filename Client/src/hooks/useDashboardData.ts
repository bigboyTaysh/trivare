import { useState, useEffect } from "react";
import { getTrips } from "../services/api";
import type { DashboardViewModel, TripListDto } from "../types/trips";

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
        });
      } catch (error) {
        setState((prev) => ({
          ...prev,
          isLoading: false,
          error: error instanceof Error ? error : new Error("Failed to load trips"),
        }));
      }
    };

    fetchDashboardData();
  }, []);

  return state;
}
