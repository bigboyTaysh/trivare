# API Endpoint Implementation Plan: List Days

## 1. Endpoint Overview
This endpoint retrieves all days associated with a specific trip. It is a read-only operation that requires authentication and ensures the requesting user owns the trip. The endpoint follows REST principles and integrates with the Clean Architecture layers, using Entity Framework for data access and RLS for security.

## 2. Request Details
- **HTTP Method:** GET
- **URL Structure:** `/api/trips/{tripId}/days`
- **Parameters:**
  - Required: `tripId` (path parameter, string, must be a valid GUID representing the trip ID)
  - Optional: None
- **Request Body:** None
- **Authentication:** Required (JWT token in Authorization header)
- **Headers:** `Authorization: Bearer {jwt-token}`

## 3. Used Types
- **Domain Entity:** `Day` (from `Server/Domain/Entities/Day.cs`) - represents the day entity with properties like Id, TripId, Date, Notes.
- **DTO:** `DayDto` (from `Server/Application/DTOs/Days/DayDto.cs`) - response DTO with Id, Date, Notes. If not existing, create it with these properties.

## 4. Response Details
- **Success Response (200 OK):**
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "date": "2025-07-01",
        "notes": "Arrival day"
      },
      // ... more days
    ]
  }
  ```
- **Error Responses:**
  - `400 Bad Request`: Invalid `tripId` format (not a valid GUID).
  - `401 Unauthorized`: Missing or invalid JWT token.
  - `403 Forbidden`: Trip belongs to another user.
  - `404 Not Found`: Trip does not exist.
  - `500 Internal Server Error`: Unexpected server error (e.g., database failure).

## 5. Data Flow
1. **Controller Layer (API):** `DaysController.GetDays(tripId)` receives the request, validates JWT, and calls `DayService`.
2. **Application Layer:** `DayService.GetDaysForTrip(tripId, userId)` validates ownership, queries repository.
3. **Infrastructure Layer:** Repository (e.g., `DayRepository`) executes EF query with RLS applied, fetching days for the trip.
4. **Database:** Azure SQL returns days data, filtered by RLS.
5. **Response Mapping:** Service maps entities to `DayDto`, controller returns JSON.

## 6. Security Considerations
- **Authentication:** JWT required; validate token in middleware.
- **Authorization:** Ensure user owns the trip via service check (RLS provides additional DB-level protection).
- **Input Validation:** Validate `tripId` as GUID to prevent injection.
- **Data Exposure:** Only return days for owned trips; no sensitive data in response.

## 7. Error Handling
- **Validation Errors (400):** Invalid GUID format - return immediately with message.
- **Authentication Errors (401):** Handled by middleware; log if needed.
- **Authorization Errors (403):** Trip not owned.
- **Not Found (404):** Trip doesn't exist - check via repository.
- **Server Errors (500):** Catch exceptions in middleware, log, return generic error.
- **Logging:** General errors via logging framework.

## 8. Performance Considerations
- **Query Optimization:** Use EF Include() if expanding related data; apply AsNoTracking() for read-only.
- **Indexing:** Rely on existing FK indexes on `Days.TripId`.
- **Caching:** No caching needed for dynamic trip data.
- **Pagination:** Not implemented (small dataset per trip), but monitor for future addition if trips grow.
- **RLS Overhead:** Minimal impact as it's DB-level filtering.

## 9. Implementation Steps
1. **Verify Existing Code:** Check `DaysController`, `DayService`, `DayDto`, and `DayRepository` for existing implementations.
2. **Create/Update DTO:** If `DayDto` doesn't exist, create it in `Server/Application/DTOs/Days/DayDto.cs` with Id, Date, Notes.
3. **Update Controller:** In `DaysController.cs`, add `GetDays` action method with route `/api/trips/{tripId}/days`, validate tripId, call service.
4. **Update Service:** In `DayService.cs`, add `GetDaysForTrip` method to query days by tripId, ensure ownership check.
5. **Update Repository:** Ensure `DayRepository` has a method to fetch days with trip ownership validation.
6. **Add Validation:** Implement GUID validation in controller; add ownership check in service.
7. **Update Swagger:** Add endpoint documentation with examples and security schemes.
8. **Code Review:** Ensure Clean Architecture separation, no outer-layer dependencies in inner layers.
