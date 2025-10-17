# API Endpoint Implementation Plan: Add Transport

## 1. Endpoint Overview
This endpoint allows authenticated users to add transportation details to an existing trip. It creates a one-to-one relationship between a trip and its transport information. The endpoint validates user ownership of the trip, ensures no transport already exists, and enforces business rules on transport data. Successful creation returns the new transport object with a 201 status code.

## 2. Request Details
- **HTTP Method**: POST
- **URL Structure**: `/api/trips/{tripId}/transport`
- **Parameters**:
  - **Required**: `tripId` (path parameter, UUID of the trip)
  - **Optional**: None (path parameter is required)
- **Request Body**: JSON object with the following optional fields:
  ```json
  {
    "type": "string",
    "departureLocation": "string",
    "arrivalLocation": "string",
    "departureTime": "datetime",
    "arrivalTime": "datetime",
    "notes": "string"
  }
  ```
- **Authentication**: Required (JWT token in Authorization header)

## 3. Used Types
- **Request**: `CreateTransportRequest`
- **Response**: `CreateTransportResponse`
- **Domain Entity:** `Transport` (Domain/Entities/Transport.cs)

## 4. Response Details
- **Success Response (201 Created)**:
  ```json
  {
    "id": "uuid",
    "tripId": "uuid",
    "type": "string",
    "departureLocation": "string",
    "arrivalLocation": "string",
    "departureTime": "datetime",
    "arrivalTime": "datetime",
    "notes": "string"
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid input data
  - 401 Unauthorized: Missing or invalid JWT
  - 403 Forbidden: Trip belongs to another user
  - 404 Not Found: Trip does not exist
  - 409 Conflict: Transport already exists for this trip
  - 500 Internal Server Error: Server-side error

## 5. Data Flow
1. API controller receives POST request, validates JWT, extracts userId.
2. Controller maps request body to `CreateTransportRequest`.
3. Request is passed to `TripService.CreateTransportAsync()`.
4. Service validates trip existence and ownership via `ITripRepository`.
5. Service checks for existing transport via repository (to prevent 409).
6. If valid, creates `Transport` entity, saves via repository and UnitOfWork.
7. Maps entity to `TransportResponse` and returns.
8. Controller returns 201 with DTO.

## 6. Security Considerations
- JWT authentication required; validate token in middleware.
- Row-Level Security (RLS) in Azure SQL ensures users only access their own trips/transports.
- Session context sets UserId for RLS enforcement.
- Input validation prevents injection; use EF parameterized queries.
- Ownership check in service prevents unauthorized access to other users' trips.
- Consider rate limiting for abuse prevention (not implemented in MVP).

## 7. Error Handling
- **400 Bad Request**: Invalid JSON, missing required fields (if any), invalid date formats, or business rule violations (e.g., arrival before departure). Log as "TransportCreationFailed" with details.
- **401 Unauthorized**: Invalid/missing JWT. Handled by auth middleware.
- **403 Forbidden**: Trip not owned by user. Log access attempt.
- **404 Not Found**: Trip does not exist. Log as not found.
- **409 Conflict**: Transport already exists. Log conflict.
- **500 Internal Server Error**: Database errors or exceptions. Log full error details, return generic message.
- Use global exception handler middleware for consistent error responses. Audit all failures.

## 8. Performance Considerations
- Database queries are optimized with indexes on foreign keys (TripId).
- Use AsNoTracking for read queries in validation.
- Eager loading not needed for this endpoint.
- Consider caching trip ownership if performance issues arise (not in MVP).
- Monitor for N+1 queries; ensure single queries for existence checks.

## 9. Implementation Steps
1. Create `CreateTransportRequest` DTO in `Application/DTOs/Transport/` with properties: type, departureLocation, arrivalLocation, departureTime, arrivalTime, notes.
2. Create `TransportResponse` DTO in `Application/DTOs/Transport/` with properties: id, tripId, type, departureLocation, arrivalLocation, departureTime, arrivalTime, notes.
3. Add `CreateTransportAsync(Guid tripId, CreateTransportRequest request, Guid userId)` method to `ITripService` interface and implement in `TripService` class.
4. Update `ITripRepository` interface with methods: `GetTripByIdAsync(Guid tripId)`, `GetTransportByTripIdAsync(Guid tripId)`, and `AddTransportAsync(Transport transport)`.
5. Implement the new repository methods in `TripRepository` using Entity Framework, ensuring proper tracking and Unit of Work pattern.
6. Add `POST /api/trips/{tripId}/transport` endpoint to `TripsController` with proper route attribute and HTTP method.
7. Implement controller logic: validate JWT, extract userId, map request body to `CreateTransportRequest`, call service with tripId, request, and userId, handle exceptions, and return appropriate HTTP responses.
8. Add model validation attributes to `CreateTransportRequest` (e.g., date validations, string lengths matching DB schema).
9. Update Swagger/OpenAPI documentation for the endpoint with detailed schemas, examples, and security requirements.