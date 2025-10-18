# API Endpoint Implementation Plan: Update Accommodation

## 1. Endpoint Overview
This endpoint allows authenticated users to update accommodation details for a specific trip using a PATCH request. It supports partial updates, meaning only provided fields will be modified. The accommodation is linked one-to-one with a trip, ensuring updates are scoped to the user's own trips. This follows REST principles and Clean Architecture, with business logic handled in the Application layer.

## 2. Request Details
- **HTTP Method:** PATCH
- **URL Structure:** `/api/trips/{tripId}/accommodation`
- **Parameters:**
  - Required: `tripId` (path parameter, GUID) - The unique identifier of the trip.
  - Optional: None (body fields are optional for partial updates).
- **Request Body:** JSON object with optional fields from the Accommodation entity.
  ```json
  {
    "name": "string",  // Optional, max 255 chars
    "address": "string",  // Optional, max 500 chars
    "checkInDate": "2025-07-01T15:00:00Z",  // Optional, ISO 8601 DateTime
    "checkOutDate": "2025-07-12T11:00:00Z",  // Optional, ISO 8601 DateTime
    "notes": "string"  // Optional, max 2000 chars
  }
  ```
- **Authentication:** Required (JWT token in Authorization header).

## 3. Used Types
- **Request:** `UpdateAccommodationRequest` (Application/DTOs/Accommodation/) - Contains optional nullable properties for partial updates.
- **Response:** `AccommodationResponse` (Application/DTOs/Accommodation/) - Full accommodation object with id, tripId, name, address, checkInDate, checkOutDate, notes.
- **Domain Entities:** `Accommodation`, `Trip` (Domain/Entities/).

## 4. Response Details
- **Success (200 OK):** Returns the full updated accommodation object.
  ```json
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Hotel Eiffel",
    "address": "123 Rue de Paris, 75001 Paris",
    "checkInDate": "2025-07-01T15:00:00Z",
    "checkOutDate": "2025-07-12T11:00:00Z",
    "notes": "Booking confirmation #123456"
  }
  ```
- **Error Responses:**
  - 400 Bad Request: Invalid input data (e.g., invalid dates, field lengths exceeded).
  - 401 Unauthorized: Missing or invalid JWT.
  - 403 Forbidden: Trip belongs to another user.
  - 404 Not Found: Trip or accommodation not found.
  - 500 Internal Server Error: Unexpected server error.

## 5. Data Flow
1. **API Layer:** Controller receives PATCH request, validates JWT, extracts UserId, binds request to `UpdateAccommodationRequest`.
2. **Application Layer:** Validates command (business rules), calls `IAccommodationService.UpdateAccommodationAsync`.
3. **Service Logic:** Retrieves Accommodation via repository (ensuring trip ownership via RLS), applies partial updates, saves changes.
4. **Infrastructure Layer:** Entity Framework updates Azure SQL database with changes.
5. **Response:** Maps updated entity to `AccommodationResponse`, returns 200 OK.
- External Dependencies: None (pure DB operation).

## 6. Security Considerations
- **Authentication:** JWT required; validate token and extract UserId.
- **Authorization:** Ensure trip belongs to authenticated user (checked in service via DB query or RLS).
- **Input Validation:** Sanitize and validate all inputs to prevent injection; use parameterized queries in EF.
- **Data Protection:** No sensitive data exposed; follow GDPR for user data.
- **Threats:** Prevent unauthorized updates by enforcing ownership; log suspicious activity to AuditLog.

## 7. Error Handling
- **Validation Errors (400):** Return detailed error messages for invalid fields (e.g., "checkOutDate must be after checkInDate").
- **Authorization Errors (403/401):** Log failed attempts; return generic messages to avoid info leakage.
- **Not Found (404):** Check existence before update; log for debugging.
- **Server Errors (500):** Log stack traces; use GlobalExceptionHandlerMiddleware for consistent responses.

## 8. Performance Considerations
- **Database Queries:** Use EF Include() for related Trip if needed; ensure indexed FKs (TripId).
- **Optimization:** Partial updates minimize data transfer; cache not applicable here.
- **Bottlenecks:** Avoid N+1 queries; use AsNoTracking() for reads if not updating.
- **Scalability:** Azure SQL handles concurrency; monitor query performance.

## 9. Implementation Steps
1. **Create/Update DTOs:** Define `UpdateAccommodationRequest` and `AccommodationResponse` in Application/DTOs/Accommodation/.
2. **Extend Service:** Add `UpdateAccommodationAsync` method to `IAccommodationService` and `AccommodationService`, handling partial updates and ownership checks.
3. **Implement Controller:** Add PATCH endpoint in `AccommodationController` (or extend existing), map to service call.
4. **Add Validation:** Use FluentValidation for command validation (e.g., date logic).
5. **Handle Errors:** Integrate with GlobalExceptionHandlerMiddleware.
6. **Documentation:** Update Swagger with schemas and examples.