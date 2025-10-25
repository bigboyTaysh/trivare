import { API_BASE_URL } from "../config/api";
import type {
  TripListResponse,
  CreateTripRequest,
  CreateTripResponse,
  TripDetailDto,
  UpdateTripRequest,
  FileListResponse,
  FileUploadResponse,
  AccommodationDto,
  AddAccommodationRequest,
  UpdateAccommodationRequest,
} from "../types/trips";
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

/**
 * Fetch a specific trip by ID
 * @param tripId Trip identifier
 * @returns Promise with trip details
 */
export async function getTrip(tripId: string): Promise<TripDetailDto> {
  return fetchData<TripDetailDto>(`/trips/${tripId}`);
}

/**
 * Update a specific trip
 * @param tripId Trip identifier
 * @param data Trip update data
 * @returns Promise with updated trip details
 */
export async function updateTrip(tripId: string, data: UpdateTripRequest): Promise<TripDetailDto> {
  return fetchData<TripDetailDto>(`/trips/${tripId}`, {
    method: "PATCH",
    body: JSON.stringify(data),
  });
}

/**
 * Delete a specific trip
 * @param tripId Trip identifier
 * @returns Promise that resolves when trip is deleted
 */
export async function deleteTrip(tripId: string): Promise<void> {
  return fetchDataNoResponse(`/trips/${tripId}`, {
    method: "DELETE",
  });
}

/**
 * Add accommodation to a trip
 * @param tripId Trip identifier
 * @param data Accommodation data
 * @returns Promise with created accommodation details
 */
export async function addAccommodation(tripId: string, data: AddAccommodationRequest): Promise<AccommodationDto> {
  return fetchData<AccommodationDto>(`/trips/${tripId}/accommodation`, {
    method: "POST",
    body: JSON.stringify(data),
  });
}

/**
 * Update accommodation for a trip
 * @param tripId Trip identifier
 * @param data Accommodation update data
 * @returns Promise with updated accommodation details
 */
export async function updateAccommodation(tripId: string, data: UpdateAccommodationRequest): Promise<AccommodationDto> {
  return fetchData<AccommodationDto>(`/trips/${tripId}/accommodation`, {
    method: "PATCH",
    body: JSON.stringify(data),
  });
}

/**
 * Delete accommodation from a trip
 * @param tripId Trip identifier
 * @returns Promise that resolves when accommodation is deleted
 */
export async function deleteAccommodation(tripId: string): Promise<void> {
  return fetchDataNoResponse(`/trips/${tripId}/accommodation`, {
    method: "DELETE",
  });
}

/**
 * Fetch files for a specific trip
 * @param tripId Trip identifier
 * @returns Promise with array of files
 */
export async function getTripFiles(tripId: string): Promise<FileListResponse> {
  return fetchData<FileListResponse>(`/trips/${tripId}/files`);
}

/**
 * Upload a file to a trip with progress tracking
 * @param tripId Trip identifier
 * @param file File to upload
 * @param onProgress Optional progress callback (0-100)
 * @returns Promise with upload response
 */
export async function uploadTripFile(
  tripId: string,
  file: File,
  onProgress?: (progress: number) => void
): Promise<FileUploadResponse> {
  return new Promise((resolve, reject) => {
    const formData = new FormData();
    formData.append("file", file);

    const accessToken = typeof window !== "undefined" ? localStorage.getItem("accessToken") : null;

    const xhr = new XMLHttpRequest();

    xhr.upload.addEventListener("progress", (event) => {
      if (event.lengthComputable && onProgress) {
        const progress = Math.round((event.loaded / event.total) * 100);
        onProgress(progress);
      }
    });

    xhr.addEventListener("load", () => {
      if (xhr.status === 401) {
        if (typeof window !== "undefined") {
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
          localStorage.removeItem("user");
          window.location.href = "/login";
        }
        reject(new Error("Unauthorized"));
        return;
      }

      if (xhr.status >= 200 && xhr.status < 300) {
        try {
          const response = JSON.parse(xhr.responseText);
          resolve(response);
        } catch {
          reject(new Error("Invalid response format"));
        }
      } else {
        reject(new Error(`API error: ${xhr.statusText}`));
      }
    });

    xhr.addEventListener("error", () => {
      reject(new Error("Network error"));
    });

    xhr.open("POST", `${API_BASE_URL}/trips/${tripId}/files`);
    if (accessToken) {
      xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
    }
    xhr.send(formData);
  });
}

/**
 * Delete a specific file
 * @param fileId File identifier
 * @returns Promise that resolves when file is deleted
 */
export async function deleteFile(fileId: string): Promise<void> {
  return fetchDataNoResponse(`/files/${fileId}`, {
    method: "DELETE",
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
  getTrip,
  updateTrip,
  deleteTrip,
  addAccommodation,
  updateAccommodation,
  deleteAccommodation,
  getTripFiles,
  uploadTripFile,
  deleteFile,
  // User-related endpoints
  getMe,
  updateUser,
  deleteAccount,
};
