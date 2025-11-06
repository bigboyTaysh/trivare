# API Endpoint Implementation Plan: Update Place on Day

## 1. Endpoint Overview
This endpoint allows authenticated users to update the order or visited status of a place associated with a specific day in their trip itinerary. It supports partial updates, allowing users to modify either the display order, the visited status, or both. The endpoint ensures data integrity by validating ownership and preventing unauthorized access through Row-Level Security (RLS).

## 2. Request Details
- HTTP Method: PATCH
- URL Structure: `/api/days/{dayId}/places/{placeId}`
- Parameters:
  - Path Parameters:
    - `dayId` (required): GUID of the day containing the place
    - `placeId` (required): GUID of the place to update
  - Request Body (application/json):
    - `order` (optional): Integer representing the new display order in the day's itinerary (must be positive)
    - `isVisited` (optional): Boolean indicating whether the place has been visited
- Authentication: Required (JWT Bearer token)

## 3. Used Types
- **Request DTO**: `UpdateDayAttractionRequest` (existing)
  - Properties: `Order` (int?), `IsVisited` (bool?)
- **Response DTO**: New `UpdateDayAttractionResponse` 
  - Properties: `DayId` (Guid), `PlaceId` (Guid), `Order` (int), `IsVisited` (bool)
- **Entity**: `DayAttraction` (composite key: DayId, PlaceId)
- **Service Interface**: Extend `IPlacesService` with `UpdatePlaceOnDayAsync`

## 4. Response Details
- **Success Response (200 OK)**:
```json
{
  "dayId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "placeId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "order": 3,
  "isVisited": true
}
```
- **Error Responses**:
  - `400 Bad Request`: Invalid input data (e.g., negative order, invalid GUID format, no fields provided)
  - `401 Unauthorized`: Missing or invalid JWT token
  - `403 Forbidden`: Day belongs to another user (enforced by RLS)
  - `404 Not Found`: Day not found or place not associated with the specified day
  - `500 Internal Server Error`: Unexpected server error

## 5. Data Flow
1. **Authentication**: JWT token validated, user ID extracted
2. **Authorization**: RLS ensures user can only access their own trip data
3. **Validation**: 
   - Path parameters validated as valid GUIDs
   - Request body validated (at least one field provided, order > 0 if specified)
   - Verify DayAttraction exists and belongs to user's trip
4. **Business Logic**: Update DayAttraction entity with provided fields
5. **Database**: Save changes to DayAttractions table
6. **Response**: Return updated DayAttraction data

## 6. Security Considerations
- **Authentication**: JWT Bearer token required for all requests
- **Authorization**: Row-Level Security (RLS) prevents access to other users' data
- **Input Validation**: 
  - GUID format validation for path parameters
  - Type validation for request body fields
  - Business rule validation (order must be positive)
- **SQL Injection Prevention**: Parameterized queries via Entity Framework
- **Data Exposure**: Response only includes necessary fields, no sensitive data leaked

## 7. Error Handling
- **Validation Errors (400)**: Invalid GUIDs, negative order values, empty request body
- **Authorization Errors (403)**: Attempted access to another user's day (handled by RLS)
- **Not Found Errors (404)**: Non-existent day or place-day association
- **Server Errors (500)**: Database connection issues, unexpected exceptions
- **Logging**: Errors logged via GlobalExceptionHandlerMiddleware, audit events not required for this endpoint

## 8. Performance Considerations
- **Database Queries**: Single query to verify and update DayAttraction
- **Indexing**: Foreign key indexes on DayId and PlaceId optimize lookups
- **Caching**: No caching implemented (low-frequency update operation)
- **Concurrency**: Potential race conditions if multiple updates occur simultaneously (handled by database transactions)
- **Response Size**: Minimal response payload for fast transmission

## 9. Implementation Steps
1. **Create Response DTO**:
   - Add `UpdateDayAttractionResponse.cs` in `Application/DTOs/Places/`
   - Include properties: DayId, PlaceId, Order, IsVisited

2. **Extend Service Interface**:
   - Add `UpdatePlaceOnDayAsync` method to `IPlacesService`
   - Define parameters: Guid dayId, Guid placeId, UpdateDayAttractionRequest request, Guid userId

3. **Implement Service Method**:
   - Add method to `PlacesService.cs`
   - Implement validation logic (ownership, existence, input validation)
   - Use repository pattern for data access
   - Handle partial updates correctly

4. **Create Controller Action**:
   - Add `UpdatePlaceOnDay` action to `PlacesController.cs`
   - Apply `[Authorize]` attribute
   - Use model binding for path parameters and request body
   - Call service method and return appropriate response

5. **Add Validation Attributes**:
   - Ensure `UpdateDayAttractionRequest` has appropriate validation attributes
   - Add custom validation for business rules if needed

6. **Update Swagger Documentation**:
   - Add endpoint to Swagger with proper schemas and examples
   - Include security requirements and response examples