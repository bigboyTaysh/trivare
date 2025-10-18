# API Endpoint Implementation Plan: List Trips

## 1. Endpoint Overview
The "List Trips" endpoint allows authenticated users to retrieve a paginated list of their own trips. It supports optional parameters for pagination, sorting, and searching to enable efficient data retrieval. This read-only endpoint adheres to Clean Architecture principles, using the Application layer for business logic and the Infrastructure layer for data access. Row-Level Security (RLS) ensures users can only access their own trips.

## 2. Request Details
- **HTTP Method**: GET
- **URL Structure**: `/api/trips`
- **Parameters**:
  - **Required**: None
  - **Optional**:
    - `page` (integer, default: 1) - Page number for pagination
    - `pageSize` (integer, default: 10, max: 50) - Number of items per page
    - `sortBy` (string, default: "createdAt") - Sort field (allowed: createdAt, name, startDate)
    - `sortOrder` (string, default: "desc") - Sort order (allowed: asc, desc)
    - `search` (string, optional) - Search term for filtering by name or destination
- **Request Body**: None
- **Authentication**: Required (JWT token in Authorization header)

## 3. Used Types
- **TripListDto** (Application/DTOs/Trips/): Contains trip summary fields (id, name, destination, startDate, endDate, notes, createdAt)
- **PaginationResponse** (Application/DTOs/Common/): Contains pagination metadata (page, pageSize, totalItems, totalPages)
- **TripListResponse** (Application/DTOs/Trips/): Wrapper with `data` (array of TripListDto) and `pagination` (PaginationResponse)
- **TripListRequest** (Application/DTOs/Trips/): Optional request model for parameters (page, pageSize, sortBy, sortOrder, search)

## 4. Response Details
- **Success Response (200 OK)**:
  ```json
  {
    "data": [
      {
        "id": "uuid",
        "name": "Trip Name",
        "destination": "Destination",
        "startDate": "2025-07-01",
        "endDate": "2025-07-10",
        "notes": "Notes",
        "createdAt": "2025-06-01T10:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalItems": 5,
      "totalPages": 1
    }
  }
  ```
- **Error Responses**:
  - 401 Unauthorized: Invalid or missing JWT token
  - 400 Bad Request: Invalid query parameters
  - 500 Internal Server Error: Server-side error

## 5. Data Flow
1. Request reaches TripsController.ListTrips action.
2. JWT middleware validates token and sets user context.
3. Controller binds query parameters and validates them.
4. Controller calls ITripService.GetTripsAsync with user ID and parameters.
5. Service queries Trips table using Entity Framework, applying RLS (user ID from session context), filtering (search), sorting, and pagination.
6. Repository returns IQueryable<Trip>, service maps to DTOs and calculates pagination.
7. Response is serialized and returned.

## 6. Security Considerations
- **Authentication**: Enforced via JWT middleware; invalid tokens return 401.
- **Authorization**: RLS in Azure SQL ensures only user's trips are accessible; session context sets UserId.
- **Input Validation**: Query parameters validated for type, range (e.g., pageSize 1-50), and allowed values (sortBy, sortOrder).
- **Data Exposure**: Only non-sensitive trip data returned; no cascading includes by default.
- **Threats Mitigated**: SQL injection via EF parameterized queries; DoS via pagination limits; unauthorized access via RLS.

## 7. Error Handling
- **401 Unauthorized**: log to application logs.
- **400 Bad Request**: Return validation errors; log to application logs.
- **500 Internal Server Error**: Log full exception to application logs; return generic message.
- Use GlobalExceptionHandlerMiddleware for consistent error responses.

## 8. Performance Considerations
- **Database Queries**: Use EF with AsNoTracking for read-only; apply pagination early to limit results.
- **Indexing**: Rely on existing indexes on Trips.UserId and Trips.CreatedAt/Name/StartDate.
- **Caching**: Consider response caching for frequent requests, but evaluate based on data freshness needs.
- **Pagination Limits**: Max pageSize of 50 prevents large result sets.
- **Search Optimization**: Use EF Contains for search; monitor for full table scans.

## 9. Implementation Steps
1. Update ITripService interface with GetTripsAsync method signature.
2. Implement GetTripsAsync in TripService: Build query with filtering, sorting, pagination; map to DTOs.
3. Create/update DTOs: TripListDto, PaginationResponse, TripListResponse.
4. In TripsController, add ListTrips action: Bind parameters, validate, call service, return response.
5. Add parameter validation attributes or FluentValidation rules.
6. Update Swagger documentation for the endpoint.