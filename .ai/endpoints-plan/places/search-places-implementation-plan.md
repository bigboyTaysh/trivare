# API Endpoint Implementation Plan: Search Places (AI-Powered)

## 1. Endpoint Overview
This endpoint allows authenticated users to search for places using the Google Places API, enhanced with AI-powered filtering and ranking via OpenRouter.ai. It returns up to 5 relevant places based on location, keyword, and optional user preferences. The search event is logged to the AuditLog table for metrics tracking.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/places/search`
- **Parameters:** None (body-based)
- **Request Body:**
  ```json
  {
    "location": "Paris, France",
    "keyword": "restaurants with local cuisine",
    "preferences": "vegetarian-friendly, outdoor seating"
  }
  ```
  - Required fields: `location`, `keyword`
  - Optional fields: `preferences`

## 3. Used Types
- **Request DTO:** `PlaceSearchRequest` (existing)
- **Response DTO:** `PlaceSearchResponse` (existing)
- **Place DTO:** `PlaceDto` (existing)
- **Entity:** `Place` (for potential storage), `AuditLog` (for logging)

## 4. Response Details
- **Success (200 OK):**
  ```json
  {
    "results": [
      {
        "googlePlaceId": "ChIJD7fiBh9u5kcRYJSMaMOCCwQ",
        "name": "Le Potager du Marais",
        "formattedAddress": "22 Rue Rambuteau, 75003 Paris",
        "website": "https://www.lepotagerdumarais.fr",
        "googleMapsLink": "https://maps.google.com/?cid=123456",
        "openingHoursText": "12:00 PM - 11:00 PM"
      }
    ],
    "count": 5
  }
  ```
- **Error Responses:**
  - `400 Bad Request` - Invalid or missing search parameters
  - `401 Unauthorized` - Missing or invalid JWT token
  - `500 Internal Server Error` - External API failures (Google Places or OpenRouter.ai)

## 5. Data Flow
1. User sends authenticated POST request with search criteria.
2. Controller validates JWT and deserializes `PlaceSearchRequest`.
3. Controller calls `IPlacesService.SearchPlacesAsync(request, userId)`.
4. Service calls Google Places API to fetch initial results based on location and keyword.
5. Service sends results to OpenRouter.ai for AI filtering/ranking based on preferences.
6. Service maps filtered results to `PlaceDto` objects.
7. Service logs search event to AuditLog via repository.
8. Service returns `PlaceSearchResponse` with up to 5 results.
9. Controller returns 200 OK with response data.

## 6. Security Considerations
- **Authentication:** JWT token required in Authorization header.
- **Authorization:** Any authenticated user can search (no role restrictions).
- **Input Validation:** Sanitize location, keyword, and preferences to prevent injection.
- **Rate Limiting:** Implement to prevent abuse of external APIs.
- **API Keys:** Store Google Places API and OpenRouter.ai keys securely (e.g., Azure Key Vault).
- **Data Privacy:** Ensure no user data is sent to external APIs beyond necessary search parameters.

## 7. Error Handling
- **400 Bad Request:** Invalid input (missing required fields, malformed data).
- **401 Unauthorized:** Invalid or missing JWT.
- **500 Internal Server Error:** External API timeouts, network failures, or unexpected errors.
- Use `GlobalExceptionHandlerMiddleware` for consistent error responses.
- Log errors internally for debugging, but do not expose sensitive details to clients.

## 8. Performance Considerations
- **Caching:** Cache Google Places API results for popular locations to reduce external calls.
- **Async Operations:** Use async/await for all external API calls.
- **Result Limiting:** Always limit to 5 results to control response size.
- **Timeout Handling:** Set reasonable timeouts for external APIs (e.g., 10 seconds).
- **Database Efficiency:** Use efficient queries for AuditLog insertion.

## 9. Implementation Steps
1. Create `IPlacesService` interface in `Application/Interfaces/`.
2. Implement `PlacesService` in `Application/Services/` with Google Places API and OpenRouter.ai integration.
3. Create `PlacesController` in `Api/Controllers/` with the search endpoint.
4. Add necessary dependencies (Google Places API client, OpenRouter.ai client) via DI.
5. Implement input validation in `PlaceSearchRequest` DTO.
6. Add audit logging in the service for successful searches.
7. Update Swagger documentation for the new endpoint.
8. Add unit tests for the service and controller.
9. Test integration with external APIs in development environment.
10. Deploy and monitor for performance and error rates.