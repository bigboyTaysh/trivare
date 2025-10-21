import { API_BASE_URL } from "../config/api";
import type { TripListResponse, CreateTripRequest, CreateTripResponse } from "../types/trips";

/**
 * Generic fetch helper with JWT authentication
 */
export async function fetchData<T>(endpoint: string, options?: RequestInit): Promise<T> {
  // Get JWT token from localStorage
  const accessToken = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    headers: {
      "Content-Type": "application/json",
      ...(accessToken && { Authorization: `Bearer ${accessToken}` }),
      ...options?.headers,
    },
    ...options,
  });

  // Handle 401 Unauthorized - redirect to login
  if (response.status === 401) {
    if (typeof window !== "undefined") {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("user");
      window.location.href = "/login";
    }
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    throw new Error(`API error: ${response.statusText}`);
  }

  return response.json();
}

/**
 * Fetch all trips for the current user
 * @returns Promise with trip list response including pagination metadata
 */
export async function getTrips(): Promise<TripListResponse> {
  return fetchData<TripListResponse>("/trips?pageSize=50&sortBy=startDate&sortOrder=asc");
}

/**
 * Create a new trip
 * @param data Trip creation data
 * @returns Promise with created trip details
 */
export async function createTrip(data: CreateTripRequest): Promise<CreateTripResponse> {
  return fetchData<CreateTripResponse>("/trips", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

export const api = {
  getData: () => fetchData("/data"),
  getDataById: (id: number) => fetchData(`/data/${id}`),
  postData: (data: unknown) =>
    fetchData("/data", {
      method: "POST",
      body: JSON.stringify(data),
    }),
  // Trip-related endpoints
  getTrips,
};
