# API Endpoint Implementation Plan: Update Trip

## 1. Endpoint Overview
The Update Trip endpoint allows authenticated users to partially update their existing trip information. This is a PATCH operation that supports updating any combination of trip fields (name, destination, start/end dates, notes) while maintaining data integrity and user ownership validation. The endpoint follows REST principles with proper HTTP status codes and integrates with the Clean Architecture pattern.

## 2. Request Details
- **HTTP Method:** PATCH
- **URL Structure:** `/api/trips/{tripId}`
- **Authentication:** Required (JWT Bearer token)
- **Parameters:**
  - **Required:** 
    - `tripId` (path parameter, GUID) - The unique identifier of the trip to update
  - **Optional:** None (all in body)
- **Request Body:** JSON object with optional fields
  ```json
  {
    "name": "string?",        // 1-255 chars after trim
    "destination": "string?", // max 255 chars
    "startDate": "date?",     // ISO date format
    "endDate": "date?",       // ISO date format, >= startDate
    "notes": "string?"        // max 2000 chars
  }
  ```

## 3. Used Types
- **Request DTO:** `UpdateTripRequest` (existing in `Server/Application/DTOs/Trips/`)
- **Response DTO:** `TripDetailDto` (existing in `Server/Application/DTOs/Trips/`)
- **Entity:** `Trip` (existing in `Server/Domain/Entities/`)
- **Result Wrapper:** `Result<TripDetailDto>` using the common Result pattern

## 4. Response Details
- **Success (200 OK):**
  ```json
  {
    "id": "guid",
    "name": "string",
    "destination": "string?",
    "startDate": "date",
    "endDate": "date", 
    "notes": "string?",
    "createdAt": "datetime"
  }
  ```
- **Error Responses:**
  - `400 Bad Request` - Invalid input data (validation errors)
  - `401 Unauthorized` - Missing or invalid JWT token
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip with specified ID not found
  - `500 Internal Server Error` - Unexpected server error

## 5. Data Flow
1. **Authentication:** JWT token validated by middleware, user ID extracted
2. **Controller:** Receives request, validates model binding, calls service
3. **Service Layer:** 
   - Validates business rules (ownership, data constraints)
   - Calls repository to fetch and update trip
4. **Repository Layer:** Uses Entity Framework with RLS for data access
5. **Database:** Azure SQL with Row-Level Security ensures user data isolation
6. **Response:** Mapped back through DTOs and returned to client

## 6. Security Considerations
- **Authentication:** JWT Bearer token required, validated by ASP.NET Core middleware
- **Authorization:** User ownership verified in service layer (trip.UserId == authenticatedUserId)
- **Data Validation:** Input sanitized and validated against business rules
- **SQL Injection Protection:** Parameterized queries via Entity Framework
- **Row-Level Security:** Database-level RLS policies prevent unauthorized data access
- **Input Limits:** String lengths enforced to prevent buffer overflow attacks

## 7. Error Handling
- **Validation Errors (400):** Invalid dates, empty name, string length violations
- **Authorization Errors (403):** Attempt to update another user's trip
- **Not Found (404):** Trip ID doesn't exist or belongs to another user
- **Server Errors (500):** Database connection issues, unexpected exceptions
- **Logging:** Warnings for validation failures, errors for server issues
- **Exception Handling:** Global exception handler middleware catches unhandled exceptions

## 8. Performance Considerations
- **Database Queries:** Single query to fetch trip, update operation with minimal locking
- **Indexing:** Foreign key indexes on UserId optimize ownership checks
- **Caching:** No caching needed for update operations
- **Async Operations:** Full async/await pattern to prevent thread blocking
- **Connection Pooling:** Azure SQL connection pooling handles concurrent requests
- **RLS Overhead:** Minimal performance impact from security policies

## 9. Implementation Steps
1. **Add UpdateTripAsync to ITripService interface**
   - Define method signature with UpdateTripRequest, Guid userId, CancellationToken
   - Return Task<Result<TripDetailDto>>

2. **Implement UpdateTripAsync in TripService**
   - Add input validation (name not empty, dates valid, string lengths)
   - Fetch trip with ownership check using repository
   - Apply partial updates only for provided fields
   - Save changes and log audit event
   - Map to TripDetailDto for response

3. **Add UpdateTrip method to TripsController**
   - PATCH route with tripId parameter
   - Extract userId from JWT claims
   - Call service method and handle result
   - Add proper Swagger documentation and response types

4. **Update Repository Interface (if needed)**
   - Ensure ITripRepository has UpdateAsync method
   - Implement in TripRepository using EF Core

5. **Add Unit Tests**
   - Test validation scenarios (invalid dates, ownership)
   - Test successful updates with various field combinations
   - Test error cases (404, 403)
   - Mock repository and audit log dependencies

6. **Integration Testing**
   - Test full request flow with test database
   - Verify audit logging and RLS behavior
   - Load testing for concurrent updates

7. **Documentation Updates**
   - Update API documentation with new endpoint
   - Add examples for partial updates
   - Document validation rules and error responses