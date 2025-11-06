# API Endpoint Implementation Plan: Update Day

## 1. Endpoint Overview
This endpoint allows authenticated users to update the details of a specific day within their trip. It supports partial updates using the PATCH method, enabling changes to the date and/or notes fields. The endpoint ensures that only the owner of the trip can modify the day, adhering to Clean Architecture principles with separation of concerns across API, Application, and Infrastructure layers.

## 2. Request Details
- **HTTP Method:** PATCH
- **URL Structure:** `/api/days/{dayId}`
- **Parameters:**
  - Required: `dayId` (path parameter, string representing a valid GUID)
  - Optional: None (query parameters not used)
- **Request Body:** JSON object with optional fields
  ```json
  {
    "date": "2025-07-01",  // Optional, string in YYYY-MM-DD format
    "notes": "Updated notes for the day"  // Optional, string up to 2000 characters
  }
  ```
- **Authentication:** Required (JWT token in Authorization header)
- **Content-Type:** application/json

## 3. Used Types
- **Request DTO:** `UpdateDayRequest` (Application/DTOs/Days/)
  - Properties: `Date` (DateOnly?, optional), `Notes` (string?, optional)
- **Response:** `DayDto` (Application/DTOs/Days/)
  - Properties: `Id` (Guid), `TripId` (Guid), `Date` (DateOnly), `Notes` (string), `CreatedAt` (DateTime)
- **Domain Entity:** `Day` (Domain/Entities)
  - Properties: `Id` (Guid), `TripId` (Guid), `Date` (DateOnly), `Notes` (string)

## 4. Response Details
- **Success Response (200 OK):**
  ```json
  {
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
    "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2025-07-01",
    "notes": "Updated notes for the day"
  }
  ```
- **Error Responses:**
  - `400 Bad Request` - Invalid input data (e.g., malformed date, notes too long)
  - `401 Unauthorized` - Missing or invalid JWT token
  - `403 Forbidden` - Day belongs to another user
  - `404 Not Found` - Day with specified dayId does not exist
  - `409 Conflict` - Date already used for another day in the same trip
  - `500 Internal Server Error` - Unexpected server error

## 5. Data Flow
1. **API Layer:** Controller receives PATCH request, validates JWT, binds request to UpdateDayRequest.
2. **Application Layer:** Calls IDayService.UpdateDayAsync.
3. **Service Logic:** Validates ownership (queries Days -> Trips -> Users), checks date uniqueness within trip (no other day has same date), updates Day entity, saves via repository.
4. **Infrastructure Layer:** Repository uses Entity Framework to update Azure SQL Days table, with RLS ensuring user isolation.
5. **Response:** Maps updated entity to DayResponseDto, returns 200 OK.

## 6. Security Considerations
- **Authentication:** JWT token required; validated via middleware.
- **Authorization:** Service checks user ownership by joining Days.TripId to Trips.UserId.
- **Input Validation:** Sanitize and validate date format, notes length to prevent injection.
- **RLS:** Azure SQL Row-Level Security restricts DB access to user's data.
- **Data Protection:** No sensitive data exposed; use HTTPS for transport security.

## 7. Error Handling
- **Validation Errors (400):** Invalid GUID, date format, or notes length; log to AuditLog with "DayUpdateValidationFailed".
- **Conflict (409):** Date already used for another day in the same trip.
- **Not Found (404):** DayId not exists; log "DayNotFound".
- **Forbidden (403):** Ownership mismatch; log "UnauthorizedDayUpdate".
- **Server Errors (500):** DB exceptions; log "DayUpdateServerError" and return generic message.
- **Global Handling:** Use middleware for unhandled exceptions..

## 8. Performance Considerations
- **Query Optimization:** Use Include() for related Trip data if needed; index on Days.Id and foreign keys.
- **Caching:** No caching needed for updates; consider read caching for GET endpoints.
- **Concurrency:** Handle potential race conditions with optimistic locking if high traffic.

## 9. Implementation Steps
1. Create UpdateDayRequest and DayDto in Application/DTOs/Days/.
2. Add UpdateDayAsync method to IDayService and implement in DayService, including date uniqueness check within the trip.
3. Update DaysController with PATCH endpoint, injecting service and mapper.
4. Implement validation.
5. Add repository method in IDayRepository if needed (e.g., for checking existing dates).
6. Test ownership validation, date uniqueness, and error scenarios.
7. Update Swagger documentation with schemas and examples.