# API Endpoint Implementation Plan: Create Day

## 1. Endpoint Overview
This endpoint allows authenticated users to add a new day to an existing trip. It validates that the day falls within the trip's date range and ensures no duplicate dates exist for the trip. The endpoint follows Clean Architecture principles, separating concerns across API, Application, Domain, and Infrastructure layers.

## 2. Request Details
- HTTP Method: POST
- URL Structure: `/api/trips/{tripId}/days`
- Parameters:
  - Required: `tripId` (GUID, path parameter - identifies the trip)
  - Optional: None in path/query
- Request Body:
  ```json
  {
    "date": "2025-07-01",
    "notes": "Arrival day - explore nearby area"
  }
  ```
  - `date`: Required, string in YYYY-MM-DD format, must be within trip's start and end dates.
  - `notes`: Optional, string up to 2000 characters.

## 3. Used Types
- **Request**: `CreateDayRequest` (in Application/DTOs/Days/)
  - Properties: `Date` (DateOnly, required), `Notes` (string, optional)
- **Response**: `CreateDayResponse` (in Application/DTOs/Days/)
  - Properties: `Id` (Guid), `TripId` (Guid), `Date` (DateOnly), `Notes` (string), `CreatedAt` (DateTime)
- **Entities**: `Trip`, `Day` (in Domain/Entities/)

## 4. Response Details
- **Success (201 Created)**:
  ```json
  {
    "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
    "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "date": "2025-07-01",
    "notes": "Arrival day - explore nearby area",
    "createdAt": "2025-07-01T00:00:00Z"
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid date format, date outside trip range, or validation failure.
  - 401 Unauthorized: Missing or invalid JWT token.
  - 403 Forbidden: Trip belongs to another user.
  - 404 Not Found: Trip not found.
  - 409 Conflict: Day with the same date already exists for this trip.
  - 500 Internal Server Error: Unexpected server error.

## 5. Data Flow
1. API Controller (`DaysController`) receives request, binds to `CreateDayRequest`.
2. Controller validates JWT, extracts user ID, and calls Application service.
3. Application service (`IDayService`) validates trip ownership (via repository), checks date range against `Trips` table, ensures no duplicate in `Days` table.
4. If valid, creates new `Day` entity, saves via repository (Unit of Work pattern).
5. Maps to `CreateDayResponse` and returns response.

## 6. Security Considerations
- **Authentication**: JWT required; validate token in middleware.
- **Authorization**: RLS ensures users only access their trips/days; check trip ownership in service.
- **Input Validation**: Sanitize inputs; use parameterized queries to prevent SQL injection.
- **Rate Limiting**: Implement per-user limits to prevent abuse.
- **Data Exposure**: Ensure no sensitive data in responses; use HTTPS.

## 7. Error Handling
- Use `GlobalExceptionHandlerMiddleware` for consistent error responses.
- Specific validations:
  - Trip not found: 404.
  - Date invalid/out of range: 400 with message.
  - Duplicate date: 409.
  - Ownership violation: 403.
- Rollback transactions on failure.

## 8. Performance Considerations
- Use eager loading if needed, but minimal here.
- Index on `Days(TripId, Date)` for quick duplicate checks.
- Cache trip date ranges if frequently accessed.
- Optimize with compiled queries for validation.

## 9. Implementation Steps
1. Define `CreateDayRequest` and `CreateDayResponse` in Application/DTOs/Days/.
2. Implement validation logic in `IDayService` (new or extend existing).
3. Add repository methods for day creation and validation in Infrastructure.
4. Update `DaysController` with POST endpoint, using dependency injection.
5. Add model validation attributes and Swagger documentation.
6. Update API documentation and ensure compliance with Clean Architecture.