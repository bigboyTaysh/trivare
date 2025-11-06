import { API_BASE_URL } from "@/config/api";

/**
 * Helper function to generate Google Places Photo URL through backend proxy
 * This keeps the API key secure by proxying the image through the backend
 *
 * @param photoReference - Photo reference from Google Places API (e.g., "places/{id}/photos/{id}")
 * @param maxWidth - Maximum width of the photo in pixels (default: 400)
 * @returns The backend proxy URL that streams the image without exposing the API key
 */
export function getGooglePlacesPhotoUrl(photoReference: string, maxWidth = 400): string {
  // Use the backend proxy endpoint to fetch images securely
  // The API key is kept on the backend and never exposed to the frontend
  return `${API_BASE_URL}/places/photo?photoReference=${encodeURIComponent(photoReference)}&maxWidth=${maxWidth}`;
}
