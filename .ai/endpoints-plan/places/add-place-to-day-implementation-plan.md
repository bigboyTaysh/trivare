# API Endpoint Implementation Plan: Add Place to Day

## 1. Endpoint Overview
This endpoint allows authenticated users to add a place (attraction) to a specific day in their trip itinerary. It supports two modes: adding an existing place from the database or creating a new manual place. The endpoint ensures data integrity by validating ownership, preventing duplicates, and maintaining proper relationships between days, places, and day-attractions.

## 2. Request Details
- **HTTP Method:** `POST`
- **URL Structure:** `/api/days/{dayId}/places`
- **Authentication:** Required (JWT Bearer token)
- **Parameters:**
  - **Path Parameters:**
    - `dayId` (required): GUID identifier of the day to add the place to
  - **Request Body:** JSON object with one of the following mutually exclusive structures:
    - **For existing place:**
      ```json
      {
        "placeId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
        "order": 1
      }
      ```
      - `placeId` (required): GUID of an existing place in the database
      - `order` (required): Integer representing display order in the day's itinerary (must be positive)
    - **For new manual place:**
      ```json
      {
        "place": {
          "name": "Local Bakery",
          "formattedAddress": "10 Rue du Pain, Paris",
          "website": "https://bakery.example.com",
          "googleMapsLink": "https://maps.google.com/...",
          "openingHoursText": "7:00 AM - 7:00 PM"
        },
        "order": 2
      }
      ```
      - `place` (required): Object containing place details
        - `name` (required): String, place name (minimum 1 character after trim)
        - `formattedAddress` (optional): String, max 500 characters
        - `website` (optional): String, must be valid URL format
        - `googleMapsLink` (optional): String, must be valid URL format
        - `openingHoursText` (optional): String, max 1000 characters
      - `order` (required): Integer representing display order (must be positive)

## 3. Used Types
- **Request DTOs:**
  - `AddPlaceRequest`: Main request model supporting both existing and new place scenarios
  - `CreatePlaceRequest`: Nested DTO for new manual place creation
- **Response DTOs:**
  - `DayAttractionDto`: Response structure containing day-place association with full place details
- **Domain Entities:**
  - `Day`: Represents a day in a trip
  - `Place`: Represents a place/attraction
  - `DayAttraction`: Junction entity linking days to places with order and visit status
- **Error Codes:**
  - `DayNotFound`: When the specified day doesn't exist
  - `DayNotOwned`: When the day belongs to another user
  - `PlaceNotFound`: When specified placeId doesn't exist
  - `InvalidPlaceData`: When new place data is malformed
  - `PlaceAlreadyAdded`: When the place is already associated with the day
  - `InvalidOrder`: When order is not a positive integer

## 4. Response Details
- **Success Response (201 Created):**
  ```json
  {
    "dayId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
    "place": {
      "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
      "name": "Local Bakery",
      "formattedAddress": "10 Rue du Pain, Paris",
      "website": "https://bakery.example.com",
      "openingHoursText": "7:00 AM - 7:00 PM",
      "isManuallyAdded": true
    },
    "order": 2,
    "isVisited": false
  }
  ```
- **Error Responses:**
  - `400 Bad Request`: Invalid place data, invalid order, or providing both placeId and place object
  - `403 Forbidden`: Day belongs to another user's trip
  - `404 Not Found`: Day not found, or placeId not found when adding existing place
  - `409 Conflict`: Place already added to this day

## 5. Data Flow
1. **Authentication & Authorization:** JWT token validated, user ID extracted
2. **Input Validation:** Request body validated for structure and required fields
3. **Ownership Check:** Verify day exists and belongs to authenticated user via repository query
4. **Place Resolution:** 
   - If `placeId` provided: Fetch existing place from database
   - If `place` object provided: Create new place entity with `IsManuallyAdded = true`
5. **Conflict Check:** Verify place is not already associated with the day
6. **Association Creation:** Create `DayAttraction` entity linking day and place
7. **Persistence:** Save changes to database with transaction support
8. **Response Mapping:** Transform entities to DTO and return success response

## 6. Security Considerations
- **Row-Level Security (RLS):** Database-level security ensures users can only access their own trip data
- **Authorization Checks:** Application-layer validation confirms day ownership before any operations
- **Input Sanitization:** All string inputs validated for length and format; URLs validated for proper structure
- **SQL Injection Prevention:** All database queries use parameterized statements via Entity Framework
- **Data Exposure Prevention:** Response only includes public place information; no sensitive user data returned
- **Mutual Exclusivity:** Validation ensures only one of `placeId` or `place` is provided to prevent ambiguous requests

## 7. Error Handling
- **Validation Errors (400):** Invalid input data, missing required fields, malformed URLs, negative order values
- **Authorization Errors (403):** Attempting to modify days belonging to other users
- **Not Found Errors (404):** Non-existent day or place IDs
- **Conflict Errors (409):** Attempting to add a place that's already associated with the day
- **Server Errors (500):** Database connection issues, unexpected exceptions during processing
- **Logging:** All errors logged with structured information including user ID, day ID, and error details for debugging

## 8. Performance Considerations
- **Database Queries:** Efficient queries with proper indexing on foreign keys (DayId, PlaceId)
- **Eager Loading:** No N+1 query issues as operations involve single entities
- **Transaction Scope:** Database operations wrapped in transactions to ensure data consistency
- **Caching:** No caching needed for this write operation
- **Concurrent Access:** Row-level locking prevents race conditions when multiple users modify the same day

## 9. Implementation Steps
1. **Add Service Method:** Extend `PlacesService` with `AddPlaceToDayAsync` method
2. **Repository Dependencies:** Ensure `IDayRepository`, `IPlaceRepository`, and `IDayAttractionRepository` interfaces exist
3. **Input Validation:** Implement validation logic for request DTOs using FluentValidation
4. **Controller Implementation:** Add `POST /api/days/{dayId}/places` endpoint in `PlacesController`
5. **Authorization:** Apply `[Authorize]` attribute and extract user ID from JWT claims
6. **Business Logic:** Implement ownership validation, place resolution, and conflict detection
7. **Database Operations:** Create transaction scope for atomic place creation and association
8. **Response Mapping:** Implement AutoMapper profiles for entity-to-DTO transformation
9. **Error Handling:** Add comprehensive exception handling with appropriate HTTP status codes
10. **Documentation:** Update Swagger documentation with detailed request/response schemas