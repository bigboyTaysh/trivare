# API Endpoint Implementation Plan: Logout

## 1. Endpoint Overview
The Logout endpoint allows users to invalidate their refresh token, effectively logging them out of the application. This is a POST request to `/api/auth/logout` that takes a refresh token in the request body and ensures it can no longer be used for obtaining new access tokens. The access token will expire naturally on its own. This endpoint follows Clean Architecture principles, separating concerns across API, Application, Domain, and Infrastructure layers.

## 2. Request Details
- **HTTP Method**: POST
- **URL Structure**: `/api/auth/logout`
- **Parameters**:
  - Required: None (body-based)
  - Optional: None
- **Request Body**:
  ```json
  {
    "refreshToken": "string"  // JWT refresh token to invalidate
  }
  ```
- **Authentication**: None required (token provided in body)
- **Content-Type**: `application/json`

## 3. Used Types
- **Request DTO**: `LogoutRequestDto` (Application/DTOs/Auth/LogoutRequestDto.cs)
  - Property: `RefreshToken` (string, required)
- **Response DTO**: `LogoutResponseDto` (Application/DTOs/Auth/LogoutResponseDto.cs)
  - Property: `Message` (string, e.g., "Logged out successfully")
- **Domain Entities**: `User` (for validation), `AuditLog` (for error logging)

## 4. Response Details
- **Success Response (200 OK)**:
  ```json
  {
    "message": "Logged out successfully"
  }
  ```
- **Error Responses**:
  - 400 Bad Request: Invalid refresh token (e.g., malformed, expired, or not found)
    ```json
    {
      "message": "Invalid refresh token provided"
    }
    ```
  - 500 Internal Server Error: Unexpected server error

## 5. Data Flow
1. **API Layer**: `AuthController.Logout` receives the POST request, binds the body to `LogoutRequestDto`, and validates input.
2. **Application Layer**: Passes the request to `AuthService.LogoutAsync`.
3. **Service Logic**: Validates the refresh token (decode JWT, check expiry, verify against database), invalidates it (e.g., update user record or blacklist), and logs success/failure to `AuditLog`.
4. **Infrastructure Layer**: Uses `ApplicationDbContext` to interact with `Users` and `AuditLog` tables. If invalidation involves external storage (e.g., Redis for blacklisting), use appropriate adapters.
5. **Response**: Returns success message or error details.

## 6. Security Considerations
- **Token Invalidation**: Implement server-side invalidation to prevent token reuse. Options include updating a `RefreshTokenInvalidatedAt` field in the `Users` table or using a token blacklist in Redis/cache.
- **Input Validation**: Ensure the refresh token is a valid JWT and not expired. Use JWT libraries to decode and verify claims.
- **Rate Limiting**: Apply rate limiting to prevent brute-force attempts on invalid tokens, as no authentication is required.
- **HTTPS Enforcement**: Ensure the endpoint is only accessible over HTTPS to protect token transmission.
- **Audit Logging**: Log all logout attempts (successful or failed) to `AuditLog` for security monitoring, including user ID and failure reasons.
- **No Session Context**: Since no JWT auth is required, avoid setting SESSION_CONTEXT for RLS; handle user lookup directly.

## 7. Error Handling
- **Validation Errors (400)**: Invalid or missing refresh token. Log to `AuditLog` with event type "LogoutFailed" and details.
- **Token Expired/Not Found (400)**: If token is expired or doesn't match any user. Include descriptive message.
- **Database Errors (500)**: Log exceptions and return generic error. Ensure transactions for atomic operations.
- **Unexpected Errors (500)**: Catch all exceptions, log details, and return server error.
- Use ASP.NET middleware for consistent error responses. Implement `ExceptionFilter` for custom handling.

## 8. Performance Considerations
- **Database Queries**: Optimize lookups for user by refresh token (consider indexing on token-related fields if added).
- **Token Validation**: Use efficient JWT parsing; avoid heavy computations.
- **Caching**: If using a blacklist, leverage in-memory or Redis cache for fast checks.
- **Async Operations**: Ensure all I/O operations (database, logging) are asynchronous to avoid blocking.
- **Monitoring**: Track endpoint performance and error rates for optimization.

## 9. Implementation Steps
1. **Create DTOs**: Implement `LogoutRequestDto` and `LogoutResponseDto` in Application/DTOs/Auth/.
2. **Update AuthService**: Add `LogoutAsync` method in `AuthService` to handle validation and invalidation logic.
3. **Add Repository Methods**: Ensure `IAuditLogRepository` has methods for logging events.
4. **Implement Controller**: Add `Logout` action in `AuthController` with model validation and error handling.
5. **Add Validation**: Use FluentValidation for `LogoutRequestDto` to check token format.
6. **Database Changes**: If needed, add fields to `Users` table for token invalidation (via migration).
7. **Testing**: Write unit tests for service logic and integration tests for the endpoint.
8. **Security Review**: Ensure token invalidation prevents reuse and review for vulnerabilities.
9. **Documentation**: Update Swagger with endpoint details, examples, and security schemes.