# API Endpoint Implementation Plan: Update Transport

## 1. Endpoint Overview
This endpoint allows authenticated users to update details of a specific transport record associated with their trip. It supports partial updates using the PATCH method, enabling users to modify only the fields they want to change. The endpoint enforces ownership, ensuring users can only update transport records linked to their own trips.

## 2. Request Details
- HTTP Method: PATCH
- URL Structure: `/api/transport/{transportId}`
- Parameters:
  - Required: `transportId` (GUID in URL path, identifies the transport record to update)
  - Optional: None in query/path beyond transportId
- Request Body: JSON object with optional fields for partial update
  ```json
  {
    "type": "Flight",  // Optional, string
    "departureLocation": "Paris",  // Optional, string
    "arrivalLocation": "London",  // Optional, string
    "departureTime": "2025-07-01T10:00:00Z",  // Optional, ISO 8601 datetime
    "arrivalTime": "2025-07-01T12:00:00Z",  // Optional, ISO 8601 datetime
    "notes": "Updated flight details"  // Optional, string
  }
  ```

## 3. Used Types
- **Domain Entity**: `Transport` (Server/Domain/Entities/Transport.cs)
- **Request**: `UpdateTransportRequest` (Server/Application/DTOs/Transport/UpdateTransportRequest.cs) - Partial fields with validation attributes
- **Response**: `TransportResponse` (Server/Application/DTOs/Transport/TransportResponse.cs) - Full transport object

## 4. Response Details
- Success (200 OK): Returns the updated transport object
  ```json
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "type": "Flight",
    "departureLocation": "Paris",
    "arrivalLocation": "London",
    "departureTime": "2025-07-01T10:00:00Z",
    "arrivalTime": "2025-07-01T12:00:00Z",
    "notes": "Updated flight details"
  }
  ```
- Error Responses: As outlined in Error Handling section

## 5. Data Flow
1. API Controller receives PATCH request with transportId and partial update data
2. JWT middleware validates authentication and extracts user ID
5. Business logic validates input (e.g., arrival after departure)
5. Repository updates Transport entity in Azure SQL database
6. Response is returned

## 6. Security Considerations
- **Authentication**: JWT token required; validated by middleware
- **Authorization**: Row-Level Security (RLS) in Azure SQL ensures users only access their own data; application-layer check via trip ownership
- **Input Validation**: Model validation on DTOs prevents injection; date/time validation prevents logical inconsistencies
- **IDOR Prevention**: Explicit check that transport's TripId belongs to authenticated user
- **Rate Limiting**: Consider implementing to prevent abuse of update operations

## 7. Error Handling
- **400 Bad Request**: Invalid input (e.g., malformed GUID, invalid dates where arrival before departure, or validation failures on fields)
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: Transport record belongs to another user (ownership violation)
- **404 Not Found**: TransportId does not exist in database
- **500 Internal Server Error**: Database connection issues, unexpected exceptions; log to AuditLog as "TransportUpdateFailed"
- Global exception handler provides consistent error responses; audit logging captures failures for monitoring

## 8. Performance Considerations
- Use AsNoTracking() for read operations during ownership validation
- Eager loading not needed for this update operation
- Database indexes on Transport.Id and Transport.TripId optimize lookups
- Consider caching user session context to reduce repeated ownership checks
- Monitor query performance; use compiled queries if update pattern becomes frequent

## 9. Implementation Steps
1. Create UpdateTransportRequest in Server/Application/DTOs/Transport/ with validation attributes (e.g., [Required] if needed, date comparisons)
2. Create TransportResponse in Server/Application/DTOs/Transport/ mapping full entity
3. Add UpdateTransport method to ITripService or new ITransportService interface
4. Implement service method in TripService or TransportService with ownership check and update logic
5. Create TransportController method with PATCH route, model binding, and command dispatching
6. Update Swagger documentation with endpoint details and examples