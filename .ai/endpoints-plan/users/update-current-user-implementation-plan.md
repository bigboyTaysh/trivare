# API Endpoint Implementation Plan: Update Current User

## 1. Endpoint Overview
This endpoint allows authenticated users to update their own profile information, specifically the username and/or password. It follows REST principles with PATCH method for partial updates, ensuring users can only modify their own data through JWT-based authentication. The implementation adheres to Clean Architecture, separating concerns across API, Application, and Domain layers. Email cannot be changed through this endpoint.

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
    "userName": "newusername",
    "currentPassword": "CurrentPassword123!",
    "newPassword": "NewSecurePassword123!"
  }
  ```
  - Content-Type: application/json
  - All fields are optional
  - To change password, both currentPassword and newPassword must be provided
  - Username is for display purposes only (not unique, not used for login)

## 3. Used Types
- **Request DTO**: `UpdateUserRequest` (Application/DTOs/Users/UpdateUserRequest.cs)
  - Properties: 
    - UserName (string, optional, 3-50 chars, alphanumeric + underscore/hyphen)
    - CurrentPassword (string, optional, min 8 chars)
    - NewPassword (string, optional, min 8 chars, must contain uppercase, lowercase, digit, special char)
  - Validation: Format validation, password strength, current password verification
- **Response DTO**: `UserDto` (Application/DTOs/Users/UserDto.cs)
  - Properties: Id, UserName, Email, CreatedAt, Roles
- **Domain Entity**: `User` (Domain/Entities/User.cs)
  - Properties: Id, UserName, Email, PasswordHash, PasswordSalt, etc.
- **Command Model**: Not used (following Clean Architecture guidelines to avoid CQRS unless necessary)

## 4. Response Details
- **Success Response (200 OK)**:
  ```json
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "userName": "newusername",
    "email": "user@example.com",
    "createdAt": "2025-10-12T10:30:00Z",
    "roles": ["User"]
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid username format, weak password, or current password mismatch
  - 401 Unauthorized: Missing or invalid JWT token
  - 404 Not Found: User not found (edge case for /me endpoint)
  - 500 Internal Server Error: Database or server errors

## 5. Data Flow
1. **API Layer**: UsersController receives PATCH request, validates JWT, binds request to UpdateUserRequest
2. **Application Layer**: Controller calls IUserService.UpdateUserAsync, passing user ID from JWT and update data
3. **Domain Layer**: UserService validates input:
   - If username provided: updates User.UserName (no uniqueness check - display name only)
   - If passwords provided: verifies current password, hashes and updates User.PasswordHash and User.PasswordSalt
4. **Infrastructure Layer**: Repository saves changes to Azure SQL Users table via Entity Framework
5. **Audit Logging**: Log successful updates or failures to AuditLog table
6. **Response**: Map updated User entity to UserDto and return

## 6. Security Considerations
- **Authentication**: JWT token validation ensures only authenticated users can access
- **Authorization**: Extract user ID from JWT claims to ensure users can only update their own profile
- **Input Validation**: 
  - Validate username format (3-50 chars, alphanumeric + underscore/hyphen)
  - Validate password strength (min 8 chars, uppercase, lowercase, digit, special char)
  - Verify current password before allowing password change
- **Password Security**: 
  - Hash passwords using secure algorithm with salt (PasswordHashingService)
  - Never log or return password values
  - Require current password verification for password changes
- **Data Protection**: Use parameterized queries in Entity Framework to prevent SQL injection
- **Row-Level Security**: RLS policies on Users table ensure users can't access others' data
- **Rate Limiting**: Consider implementing rate limiting to prevent abuse
- **Audit Trail**: All updates logged to AuditLog for compliance and monitoring

## 7. Error Handling
- **Validation Errors (400)**: 
  - Invalid username format
  - Weak password (doesn't meet requirements)
  - Current password mismatch (AuthErrorCodes.CurrentPasswordMismatch)
  - Missing current password when trying to change password
- **Authentication Errors (401)**: Generic message to avoid token enumeration
- **Not Found (404)**: User not found (rare for /me, but handled with UserErrorCodes.UserNotFound)
- **Server Errors (500)**: Log detailed errors, return generic message to client
- **Exception Handling**: Use GlobalExceptionHandlerMiddleware for consistent error responses
- **Logging**: Log all errors with user ID and error details for debugging and monitoring

## 8. Performance Considerations
- **Database Queries**: Minimal queries - single user fetch by ID and single update
- **Password Hashing**: Use efficient hashing algorithm (configurable work factor for balance)
- **Caching**: Consider caching user data if reads are frequent (invalidate on update)
- **Connection Pooling**: Leverage Entity Framework's connection pooling for Azure SQL
- **Async Operations**: Implement async/await throughout the pipeline
- **Validation Efficiency**: Client-side validation reduces server load
- **Monitoring**: Track endpoint performance metrics for optimization

## 9. Implementation Steps
1. ✅ **Create/Update DTOs**: UpdateUserRequest with UserName, CurrentPassword, NewPassword fields
2. ✅ **Add Validation**: Data annotations in DTO for format validation
3. ✅ **Update Controller**: PATCH /api/users/me endpoint in UsersController with JWT auth
4. ✅ **Implement Service Interface**: IUserService.UpdateUserAsync method signature
5. ✅ **Implement Service Logic**: UserService.UpdateUserAsync with:
   - Username update (trim whitespace)
   - Password verification and hashing (using IPasswordHashingService)
6. ✅ **Update Repository**: IUserRepository.UpdateAsync for persisting changes
7. ✅ **Update Domain Entity**: User entity includes UserName property
8. ✅ **Error Codes**: AuthErrorCodes.CurrentPasswordMismatch, UserErrorCodes.UserNotFound
9. **Add Tests**: Create unit tests for service, controller, and validation logic
10. **Update Swagger**: Document endpoint with examples and security schemes
11. **Test Integration**: Test with real database and JWT authentication
12. **Deploy and Monitor**: Deploy to staging, monitor performance and error rates

## 10. Key Implementation Notes
- **Username is NOT unique**: Multiple users can have the same username (display name only)
- **Email cannot be changed**: Email is the unique identifier for login and cannot be modified
- **Optional fields**: Both username and password updates are optional - user can update one, both, or neither
- **Password change requires verification**: Current password must be provided and verified before allowing password change
- **No uniqueness checks for username**: Service does not check if username already exists