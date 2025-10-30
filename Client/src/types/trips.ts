import { z } from "zod";

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

/**
 * Corresponds to UpdateTripRequest.cs
 */
export interface UpdateTripRequest {
  name?: string;
  destination?: string;
  startDate?: string; // Format: "YYYY-MM-DD"
  endDate?: string; // Format: "YYYY-MM-DD"
  notes?: string;
}

/**
 * Corresponds to TripDetailDto.cs
 */
export interface TripDetailDto {
  id: string; // Guid
  name: string;
  destination?: string;
  startDate: string; // DateOnly as string "YYYY-MM-DD"
  endDate: string; // DateOnly as string "YYYY-MM-DD"
  notes?: string;
  createdAt: string; // ISO 8601 DateTime string
  transport?: unknown; // TransportDto - to be defined later
  transports?: TransportDto[]; // TransportDto[] - multiple transports per trip
  accommodation?: AccommodationDto;
  days?: unknown[]; // DayDto[] - to be defined later
  files?: FileDto[];
}

/**
 * Corresponds to FileDto.cs
 */
export interface FileDto {
  id: string; // Guid
  fileName: string;
  fileSize: number; // long as number
  fileType: string;
  tripId?: string;
  transportId?: string;
  accommodationId?: string;
  dayId?: string;
  createdAt: string; // ISO 8601 DateTime string
  downloadUrl: string;
  filePath: string;
  previewUrl: string;
}

/**
 * Corresponds to TransportDto.cs
 */
export interface TransportDto {
  id: string; // Guid
  tripId: string; // Guid
  type?: string;
  departureLocation?: string;
  arrivalLocation?: string;
  departureTime?: string; // ISO 8601 DateTime string
  arrivalTime?: string; // ISO 8601 DateTime string
  notes?: string;
}

/**
 * Corresponds to TransportResponse.cs
 */
export interface TransportResponse {
  id: string; // Guid
  tripId: string; // Guid
  type: string;
  departureLocation?: string;
  arrivalLocation?: string;
  departureTime?: string; // ISO 8601 DateTime string
  arrivalTime?: string; // ISO 8601 DateTime string
  notes?: string;
}

/**
 * Request to create a new transport
 */
export interface CreateTransportRequest {
  type: string; // Required, max 100 chars
  departureLocation?: string; // Optional, max 255 chars
  arrivalLocation?: string; // Optional, max 255 chars
  departureTime?: string; // Optional, ISO 8601 DateTime string
  arrivalTime?: string; // Optional, ISO 8601 DateTime string, must be after departureTime
  notes?: string; // Optional, max 2000 chars
}

/**
 * Request to update an existing transport (partial update)
 */
export interface UpdateTransportRequest {
  type?: string; // Optional, max 100 chars
  departureLocation?: string; // Optional, max 255 chars
  arrivalLocation?: string; // Optional, max 255 chars
  departureTime?: string; // Optional, ISO 8601 DateTime string
  arrivalTime?: string; // Optional, ISO 8601 DateTime string, must be after departureTime
  notes?: string; // Optional, max 2000 chars
}

/**
 * Corresponds to AccommodationDto.cs
 */

/**
 * Response from GET /api/trips/{tripId}/files
 * Backend returns array directly
 */
export type FileListResponse = FileDto[];

/**
 * Corresponds to FileUploadResponse.cs
 */
export interface FileUploadResponse {
  id: string;
  fileName: string;
  fileSize: number;
  fileType: string;
  tripId?: string;
  transportId?: string;
  accommodationId?: string;
  dayId?: string;
  createdAt: string;
  downloadUrl: string;
  filePath: string;
  previewUrl: string;
}

/**
 * Corresponds to AccommodationDto.cs
 */
export interface AccommodationDto {
  id: string; // Guid
  tripId: string; // Guid
  name?: string;
  address?: string;
  checkInDate?: string; // ISO 8601 DateTime string
  checkOutDate?: string; // ISO 8601 DateTime string
  notes?: string;
}

/**
 * Request to add accommodation to a trip
 */
export interface AddAccommodationRequest {
  name?: string;
  address?: string;
  checkInDate?: string | null; // ISO 8601 DateTime string or null
  checkOutDate?: string | null; // ISO 8601 DateTime string or null
  notes?: string;
}

/**
 * Request to update accommodation
 */
export interface UpdateAccommodationRequest {
  name?: string;
  address?: string;
  checkInDate?: string | null; // ISO 8601 DateTime string or null to clear
  checkOutDate?: string | null; // ISO 8601 DateTime string or null to clear
  notes?: string;
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

/**
 * Frontend view model for Trip Details view
 */
export interface TripDetailViewModel {
  id: string;
  name: string;
  destination: string;
  startDate: string;
  endDate: string;
  notes?: string;
  transports?: TransportViewModel[];
  accommodation?: {
    id: string;
    name: string;
    address?: string;
    checkInDate?: string;
    checkOutDate?: string;
    notes?: string;
  };
}

/**
 * Frontend view model for transport data
 */
export interface TransportViewModel {
  id: string;
  tripId: string;
  type: string;
  departureLocation?: string;
  arrivalLocation?: string;
  departureTime?: string;
  arrivalTime?: string;
  notes?: string;
}

export interface FileViewModel {
  id: string;
  fileName: string;
  fileSize: number; // in bytes
  fileType: string;
  uploadedAt: string;
  previewUrl: string;
  downloadUrl: string;
}

/**
 * Corresponds to UpdateTripRequest.cs - Validation Schema
 */
// ... existing code ...

export const UpdateTripViewModel = z
  .object({
    name: z
      .string()
      .min(1, { message: "Trip name is required" })
      .max(255, { message: "Trip name cannot exceed 255 characters" }),
    destination: z.string().max(255, { message: "Destination cannot exceed 255 characters" }).optional(),
    startDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, { message: "Start date must be in YYYY-MM-DD format" }),
    endDate: z.string().regex(/^\d{4}-\d{2}-\d{2}$/, { message: "End date must be in YYYY-MM-DD format" }),
    notes: z.string().max(2000, { message: "Notes cannot exceed 2000 characters" }).optional(),
  })
  .refine((data) => new Date(data.endDate) >= new Date(data.startDate), {
    message: "End date must be on or after start date",
    path: ["endDate"],
  });

export type UpdateTripViewModel = z.infer<typeof UpdateTripViewModel>;

/**
 * Validation schema for adding accommodation
 */
export const AddAccommodationViewModel = z
  .object({
    name: z.string().max(255, { message: "Name cannot exceed 255 characters" }).optional(),
    address: z.string().max(500, { message: "Address cannot exceed 500 characters" }).optional(),
    checkInDate: z.string().optional(),
    checkOutDate: z.string().optional(),
    notes: z.string().max(2000, { message: "Notes cannot exceed 2000 characters" }).optional(),
  })
  .refine(
    (data) => {
      if (data.checkInDate && data.checkOutDate) {
        return new Date(data.checkOutDate) >= new Date(data.checkInDate);
      }
      return true;
    },
    {
      message: "Check-out date must be on or after check-in date",
      path: ["checkOutDate"],
    }
  );

export type AddAccommodationViewModel = z.infer<typeof AddAccommodationViewModel>;

/**
 * Validation schema for updating accommodation
 */
export const UpdateAccommodationViewModel = z
  .object({
    name: z.string().max(255, { message: "Name cannot exceed 255 characters" }).optional(),
    address: z.string().max(500, { message: "Address cannot exceed 500 characters" }).optional(),
    checkInDate: z.string().optional(),
    checkOutDate: z.string().optional(),
    notes: z.string().max(2000, { message: "Notes cannot exceed 2000 characters" }).optional(),
  })
  .refine(
    (data) => {
      if (data.checkInDate && data.checkOutDate) {
        return new Date(data.checkOutDate) >= new Date(data.checkInDate);
      }
      return true;
    },
    {
      message: "Check-out date must be on or after check-in date",
      path: ["checkOutDate"],
    }
  );

export type UpdateAccommodationViewModel = z.infer<typeof UpdateAccommodationViewModel>;

/**
 * Corresponds to DayDto.cs
 */
export interface DayDto {
  id: string; // Guid
  tripId: string; // Guid
  date: string; // DateOnly as string "YYYY-MM-DD"
  notes?: string;
}

/**
 * Corresponds to DayWithPlacesDto.cs
 */
export interface DayWithPlacesDto {
  id: string; // Guid
  tripId: string; // Guid
  date: string; // DateOnly as string "YYYY-MM-DD"
  notes?: string;
  places: DayAttractionDto[] | null;
}

/**
 * Corresponds to DayAttractionDto.cs
 */
export interface DayAttractionDto {
  dayId: string; // Guid
  placeId: string; // Guid
  place: PlaceDto;
  order: number;
  isVisited: boolean;
}

/**
 * Corresponds to PlaceDto.cs
 */
export interface PlaceDto {
  id: string; // Guid
  googlePlaceId?: string;
  name: string;
  formattedAddress?: string;
  website?: string;
  googleMapsLink?: string;
  openingHoursText?: string;
  isManuallyAdded: boolean;
}

/**
 * Corresponds to CreateDayRequest.cs
 */
export interface CreateDayRequest {
  date: string; // Required, format: "YYYY-MM-DD"
  notes?: string; // Optional, max 2000 chars
}

/**
 * Corresponds to UpdateDayRequest.cs - to be defined later
 */
export interface UpdateDayRequest {
  date?: string; // Format: "YYYY-MM-DD"
  notes?: string; // Optional, max 2000 chars
}

export interface AddPlaceRequest {
  order: number;
  placeId?: string;
  place?: {
    name: string;
    formattedAddress?: string;
    website?: string;
    googleMapsLink?: string;
    openingHoursText?: string;
  };
}

export interface UpdatePlaceRequest {
  name?: string;
  formattedAddress?: string;
  website?: string;
  googleMapsLink?: string;
  openingHoursText?: string;
}
