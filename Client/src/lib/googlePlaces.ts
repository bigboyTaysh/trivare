// Helper function to generate Google Places Photo URL
// Note: In production, you should fetch this from your backend to keep the API key secure
export function getGooglePlacesPhotoUrl(photoReference: string, maxWidth = 400): string {
  // The backend should provide a proxy endpoint for Google Places photos
  // to avoid exposing the API key on the frontend
  return `/api/places/photo?photoReference=${encodeURIComponent(photoReference)}&maxWidth=${maxWidth}`;
}
