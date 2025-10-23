import { API_BASE_URL } from "../config/api";
import type { TripListResponse, CreateTripRequest, CreateTripResponse } from "../types/trips";
import type { UserDto, UpdateUserRequest, DeleteAccountRequest } from "../types/user";
import type { ForgotPasswordRequest, ResetPasswordRequest } from "../types/auth";

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
 * Generic fetch helper for requests that don't return JSON (e.g., 204 No Content)
 */
export async function fetchDataNoResponse(endpoint: string, options?: RequestInit): Promise<void> {
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

/**
 * Fetch the current user's profile data
 * @returns Promise with user profile data
 */
export async function getMe(): Promise<UserDto> {
  return fetchData<UserDto>("/users/me");
}

/**
 * Update the current user's profile (username or password)
 * @param data User update data
 * @returns Promise with updated user profile data
 */
export async function updateUser(data: UpdateUserRequest): Promise<UserDto> {
  return fetchData<UserDto>("/users/me", {
    method: "PATCH",
    body: JSON.stringify(data),
  });
}

/**
 * Delete the current user's account
 * @param data Account deletion data (password confirmation)
 * @returns Promise that resolves when account is deleted
 */
export async function deleteAccount(data: DeleteAccountRequest): Promise<void> {
  return fetchDataNoResponse("/users/me", {
    method: "DELETE",
    body: JSON.stringify(data),
  });
}

/**
 * Send a forgot password request
 * @param data Forgot password request data
 * @returns Promise that resolves when request is sent
 */
export async function forgotPassword(data: ForgotPasswordRequest): Promise<void> {
  return fetchDataNoResponse("/auth/forgot-password", {
    method: "POST",
    body: JSON.stringify(data),
  });
}

/**
 * Reset user password
 * @param data Reset password request data
 * @returns Promise with success status and optional error message
 */
export async function resetPassword(data: ResetPasswordRequest): Promise<{ success: boolean; error?: string }> {
  try {
    await fetchDataNoResponse("/auth/reset-password", {
      method: "POST",
      body: JSON.stringify(data),
    });
    return { success: true };
  } catch (error) {
    return { success: false, error: error instanceof Error ? error.message : "An unexpected error occurred" };
  }
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
  // User-related endpoints
  getMe,
  updateUser,
  deleteAccount,
};
