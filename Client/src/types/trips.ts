// API DTOs (Expected from Backend)

/**
 * Corresponds to TripListDto.cs
 */
export interface TripListDto {
  id: string; // Guid
  name: string;
  destination?: string;
  startDate: string; // Format: "YYYY-MM-DD"
  endDate: string; // Format: "YYYY-MM-DD"
  notes?: string;
  createdAt: string; // ISO 8601 DateTime string
}

/**
 * Corresponds to TripListResponse.cs
 */
export interface TripListResponse {
  data: TripListDto[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}

/**
 * Corresponds to CreateTripRequest.cs
 */
export interface CreateTripRequest {
  name: string; // Required, max 255 chars
  destination?: string; // Optional, max 255 chars
  startDate: string; // Required, format: "YYYY-MM-DD"
  endDate: string; // Required, format: "YYYY-MM-DD", must be >= startDate
  notes?: string; // Optional, max 2000 chars
}

/**
 * Corresponds to CreateTripResponse.cs
 */
export interface CreateTripResponse {
  id: string; // Guid
  userId: string; // Guid
  name: string;
  destination?: string;
  startDate: string; // Format: "YYYY-MM-DD"
  endDate: string; // Format: "YYYY-MM-DD"
  notes?: string;
  createdAt: string; // ISO 8601 DateTime string
}

// ViewModels (Frontend-specific state)

/**
 * Frontend view model for the Dashboard view
 */
export interface DashboardViewModel {
  // Categorized data
  ongoingTrips: TripListDto[]; // Current and future trips combined
  pastTrips: TripListDto[];

  // Metadata for UI logic
  totalTripCount: number;
  readonly tripLimit: 10;

  // UI state flags
  isLoading: boolean;
  error: Error | null;
}
