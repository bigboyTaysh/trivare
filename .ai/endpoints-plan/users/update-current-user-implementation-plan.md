# API Endpoint Implementation Plan: Update Current User

## 1. Endpoint Overview
This endpoint allows authenticated users to update their own profile information, specifically the email address. It follows REST principles with PATCH method for partial updates, ensuring users can only modify their own data through JWT-based authentication. The implementation adheres to Clean Architecture, separating concerns across API, Application, and Domain layers.

## 2. Request Details
- **HTTP Method**: PATCH
- **URL Structure**: `/api/users/me`
- **Authentication**: Required (JWT Bearer token in Authorization header)
- **Parameters**:
  - Required: None (authentication via JWT)
  - Optional: None (body contains optional fields)
- **Request Body**:
  ```json
  {
    "email": "newemail@example.com"
  }
  ```
  - Content-Type: application/json
  - Only email field is supported for updates

## 3. Used Types
- **Request DTO**: `UpdateUserRequestDto` (Application/DTOs/Users/UpdateUserRequestDto.cs)
  - Properties: Email (string, optional)
  - Validation: Email format, uniqueness check
- **Response DTO**: `UserResponseDto` (Application/DTOs/Users/UserResponseDto.cs)
  - Properties: Id, Email, CreatedAt, Roles
- **Domain Entity**: `User` (Domain/Entities/User.cs)
- **Command Model**: Not used (following Clean Architecture guidelines to avoid CQRS unless necessary)

## 4. Response Details
- **Success Response (200 OK)**:
  ```json
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "newemail@example.com",
    "createdAt": "2025-10-12T10:30:00Z",
    "roles": ["User"]
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid email format or validation errors
  - 401 Unauthorized: Missing or invalid JWT token
  - 404 Not Found: User not found (edge case for /me endpoint)
  - 409 Conflict: Email already exists
  - 500 Internal Server Error: Database or server errors

## 5. Data Flow
1. **API Layer**: UsersController receives PATCH request, validates JWT, binds request to UpdateUserRequestDto
2. **Application Layer**: Controller calls IUserService.UpdateUser, passing user ID from JWT and update data
3. **Domain Layer**: UserService validates input, checks email uniqueness, updates User entity
4. **Infrastructure Layer**: Repository saves changes to Azure SQL Users table via Entity Framework
5. **Audit Logging**: Log successful updates or failures to AuditLog table
6. **Response**: Map updated User entity to UserResponseDto and return

## 6. Security Considerations
- **Authentication**: JWT token validation ensures only authenticated users can access
- **Authorization**: Extract user ID from JWT claims to ensure users can only update their own profile
- **Input Validation**: Sanitize and validate email format to prevent injection attacks
- **Data Protection**: Use parameterized queries in Entity Framework to prevent SQL injection
- **Row-Level Security**: RLS policies on Users table ensure users can't access others' data
- **Rate Limiting**: Consider implementing rate limiting to prevent abuse
- **Audit Trail**: All updates logged to AuditLog for compliance and monitoring

## 7. Error Handling
- **Validation Errors (400)**: Return detailed validation messages for email format issues
- **Authentication Errors (401)**: Generic message to avoid token enumeration
- **Conflict Errors (409)**: Indicate email already in use without revealing other users' data
- **Not Found (404)**: User not found (rare for /me, but handled)
- **Server Errors (500)**: Log detailed errors, return generic message to client
- **Exception Handling**: Use GlobalExceptionHandlerMiddleware for consistent error responses
- **Logging**: Log all errors to AuditLog with EventType "UserUpdateFailed" including user ID and error details

## 8. Performance Considerations
- **Database Queries**: Use indexed queries for email uniqueness check
- **Caching**: Consider caching user data if updates are frequent
- **Connection Pooling**: Leverage Entity Framework's connection pooling for Azure SQL
- **Async Operations**: Implement async/await throughout the pipeline
- **Validation Efficiency**: Client-side validation reduces server load
- **Monitoring**: Track endpoint performance metrics for optimization

## 9. Implementation Steps
1. **Create/Update DTOs**: Implement UpdateUserRequestDto and ensure UserResponseDto exists in Application/DTOs
2. **Add Validation**: Implement email format and uniqueness validation in service layer
3. **Update Controller**: Add PATCH method to UsersController with proper routing and authorization
4. **Implement Service Logic**: Extend IUserService.UpdateUser to handle email updates with validation
5. **Add Repository Methods**: Ensure IUserRepository supports user updates with email uniqueness checks
6. **Implement Audit Logging**: Add logging to AuditLog for update operations
7. **Add Tests**: Create unit tests for service, controller, and validation logic
8. **Update Swagger**: Document endpoint with examples and security schemes
9. **Test Integration**: Test with real database and JWT authentication
10. **Deploy and Monitor**: Deploy to staging, monitor performance and error rates