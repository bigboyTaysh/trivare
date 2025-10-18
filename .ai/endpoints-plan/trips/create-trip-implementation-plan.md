# API Endpoint Implementation Plan: Create Trip

## 1. Endpoint Overview
This endpoint allows authenticated users to create a new trip. It validates the input, enforces business rules (e.g., maximum 10 trips per user), and stores the trip in the database. The endpoint follows Clean Architecture principles, separating concerns across API, Application, Domain, and Infrastructure layers.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/trips`
- **Authentication:** Required (JWT Bearer token)
- **Parameters:**
  - None (path or query)
- **Request Body:**
  ```json
  {
    "name": "string", // Required, max 255 chars
    "destination": "string", // Optional, max 255 chars
    "startDate": "date", // Required, ISO date format
    "endDate": "date", // Required, ISO date format
    "notes": "string" // Optional, max 2000 chars
  }
  ```

## 3. Used Types
- **Request:** `CreateTripRequest` (Application/DTOs/Trips/CreateTripRequest.cs)
- **Response:** `CreateTripResponse` (Application/DTOs/Trips/CreateTripResponse.cs)
- **Domain Entity:** `Trip` (Domain/Entities/Trip.cs)

## 4. Response Details
- **Success (201 Created):**
  ```json
  {
    "id": "uuid",
    "userId": "uuid",
    "name": "string",
    "destination": "string",
    "startDate": "date",
    "endDate": "date",
    "notes": "string",
    "createdAt": "datetime"
  }
  ```
- **Error Responses:**
  - 400 Bad Request: Invalid input data
  - 401 Unauthorized: Missing or invalid JWT
  - 409 Conflict: User has reached maximum trips (10)
  - 500 Internal Server Error: Server-side error

## 5. Data Flow
1. Controller receives request, validates JWT, extracts UserId.
2. Passes request to ITripService in Application layer.
3. Service validates business rules (trip count, dates).
4. Creates Trip entity, saves via ITripRepository.
5. Logs audit event via IAuditLogRepository.
6. Maps entity to response DTO, returns 201.

## 6. Security Considerations
- JWT authentication required; validate token in middleware.
- Input validation prevents injection; use parameterized queries in EF.
- RLS in Azure SQL ensures user isolation.
- Rate limiting recommended to prevent abuse.
- Audit logging for compliance.

## 7. Error Handling
- Model validation errors: Return 400 with details.
- Business rule violations (e.g., trip limit): Return 409.
- DB errors: Log and return 500.
- Unauthorized: 401.
- Use GlobalExceptionHandlerMiddleware for consistent responses.

## 8. Performance Considerations
- EF tracking enabled for creation; use AsNoTracking for reads if needed later.
- Index on Trips.UserId for quick count queries.
- Avoid N+1 queries; eager load if expanding.
- Cache user trip count if high frequency.

## 9. Implementation Steps
1. Create CreateTripRequest and TripResponse in Application/DTOs/Trips/.
2. Implement validation in service (ITripService).
3. Update ITripRepository for save operation.
4. Add controller method in TripsController.
5. Configure Swagger for endpoint documentation.
6. Add audit logging in service.
7. Test endpoint with unit and integration tests.
8. Update migrations if schema changes (none expected).