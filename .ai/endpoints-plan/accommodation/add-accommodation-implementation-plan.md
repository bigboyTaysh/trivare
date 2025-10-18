# API Endpoint Implementation Plan: Add Accommodation

## 1. Endpoint Overview
This endpoint allows authenticated users to add accommodation details to a specific trip. It establishes a one-to-one relationship between a trip and accommodation, ensuring that only one accommodation can be associated per trip. The endpoint validates user ownership of the trip and prevents duplicate accommodations.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/trips/{tripId}/accommodation`
- **Parameters:**
  - **Required:** tripId (path parameter, Guid - the unique identifier of the trip)
  - **Optional:** None
- **Request Body:** JSON object with the following structure:
  ```json
  {
    "name": "string",  // Optional: Name of the accommodation
    "address": "string",  // Optional: Address of the accommodation
    "checkInDate": "2025-07-01T15:00:00Z",  // Optional: ISO 8601 datetime string
    "checkOutDate": "2025-07-10T11:00:00Z",  // Optional: ISO 8601 datetime string
    "notes": "string"  // Optional: Additional notes
  }
  ```

## 3. Used Types
- **Request:** `AddAccommodationRequest` (in Application/DTOs/Accommodation/) with properties: Name (string?), Address (string?), CheckInDate (DateTime?), CheckOutDate (DateTime?), Notes (string?).
- **Response:** `AccommodationResponse` (in Application/DTOs/Accommodation/) with properties: Id (Guid), TripId (Guid), Name (string?), Address (string?), CheckInDate (DateTime?), CheckOutDate (DateTime?), Notes (string?).
- **Domain Entity:** `Accommodation` (in Domain/Entities/) with properties matching the database table.

## 4. Response Details
- **Success Response (201 Created):**
  ```json
  {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Hotel Eiffel",
    "address": "123 Rue de Paris, 75001 Paris",
    "checkInDate": "2025-07-01T15:00:00Z",
    "checkOutDate": "2025-07-10T11:00:00Z",
    "notes": "Booking confirmation #123456"
  }
  ```
- **Error Responses:**
  - 400 Bad Request: Invalid input data (e.g., checkOutDate before checkInDate).
  - 401 Unauthorized: Missing or invalid JWT token.
  - 403 Forbidden: Trip belongs to another user.
  - 404 Not Found: Specified trip does not exist.
  - 409 Conflict: Accommodation already exists for this trip.
  - 500 Internal Server Error: Unexpected server error.

## 5. Data Flow
1. **API Layer:** Controller receives the request, validates JWT, binds request body to `AddAccommodationRequestDto`, and passes to Application layer.
2. **Application Layer:** Validates business rules (e.g., trip ownership, no existing accommodation), calls `IAccommodationService.AddAccommodationAsync()`.
3. **Infrastructure Layer:** Repository interacts with Azure SQL to insert the new `Accommodation` entity, leveraging Entity Framework with RLS for security.
4. **Response:** Map entity to `AccommodationResponse` and return 201.

## 6. Security Considerations
- **Authentication:** JWT required; validate token in middleware.
- **Authorization:** Ensure trip belongs to authenticated user via RLS in database queries.
- **Input Validation:** Sanitize and validate all inputs to prevent injection; use parameterized queries via EF.
- **Data Privacy:** Only return user's own data; no exposure of other users' trips/accommodations.
- **Rate Limiting:** Consider implementing to prevent abuse, though not specified in MVP.

## 7. Error Handling
- **Validation Errors (400):** Return detailed error messages for invalid dates or missing required business fields.
- **Authorization Errors (403/401):** Log unauthorized attempts; return generic messages to avoid information leakage.
- **Not Found (404):** Confirm trip exists before checking ownership.
- **Conflict (409):** Check for existing accommodation before insertion.
- **Server Errors (500):** Log full stack traces; return generic error message. Use GlobalExceptionHandlerMiddleware for consistent handling.

## 8. Performance Considerations
- **Database Queries:** Use EF with Include() if needed, but keep simple for insertion. Leverage indexes on TripId.
- **Validation:** Perform lightweight checks in Application layer; avoid heavy computations.
- **Caching:** Not applicable for creation endpoint.
- **Scalability:** Azure SQL handles concurrency; monitor for bottlenecks in high-traffic scenarios.

## 9. Implementation Steps
1. **Create DTOs:** Implement `AddAccommodationRequest` and `AccommodationResponse` in Application/DTOs/Accommodation/.
3. **Extend Service:** Create new `IAccommodationService`.
5. **Implement Repository:** Ensure `IAccommodationRepository` has `AddAsync` method; use EF for insertion with RLS.
5. **Add Controller Endpoint:** In `TripsController`, add POST method for `/api/trips/{tripId}/accommodation`, inject service, handle mapping and responses.
6. **Add Validation:** Implement business validation in service (e.g., date checks, uniqueness).
7. **Update Swagger:** Ensure endpoint is documented with examples and schemas.