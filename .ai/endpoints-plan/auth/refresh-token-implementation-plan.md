# API Endpoint Implementation Plan: Refresh Token

## 1. Endpoint Overview
The Refresh Token endpoint allows authenticated users to obtain a new access token by providing a valid refresh token. This endpoint is crucial for maintaining user sessions without requiring frequent re-authentication, enhancing user experience in the Trivare trip planning application. It follows JWT-based authentication patterns and ensures token rotation for security.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/auth/refresh`
- **Parameters:**
  - Required: None (body-based)
  - Optional: None
- **Request Body:**
  ```json
  {
    "refreshToken": "string"
  }
  ```
  - `refreshToken`: A valid JWT refresh token string (required, non-empty).

## 3. Used Types
- **Request DTO:** `RefreshTokenRequest` (Application/DTOs/Auth/RefreshTokenRequest.cs) - Contains `RefreshToken` property with validation attributes.
- **Response DTO:** `RefreshTokenResponse` (Application/DTOs/Auth/RefreshTokenResponse.cs) - Contains `AccessToken`, `RefreshToken`, and `ExpiresIn` properties.
- **Command Model:** `RefreshTokenCommand` (Application/RefreshTokenCommand.cs) - Encapsulates request data for service layer processing.
- **Domain Entities:** `User` (Domain/Entities/User.cs) - For retrieving and updating user refresh token data.

## 4. Response Details
- **Success Response (200 OK):**
  ```json
  {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": 900
  }
  ```
- **Error Responses:**
  - 400 Bad Request: Invalid request body or missing refresh token.
  - 401 Unauthorized: Invalid, expired, or non-matching refresh token.
  - 500 Internal Server Error: Unexpected server error.

## 5. Data Flow
1. Client sends POST request to `/api/auth/refresh` with refresh token in body.
2. API controller validates request model and maps to `RefreshTokenCommand`.
3. `AuthService.RefreshTokenAsync()` validates the refresh token against the database (Users table).
4. If valid, generates new access and refresh tokens, updates user's refresh token in database.
5. Logs success/failure to AuditLog table.
6. Returns new tokens or appropriate error response.
7. Database interactions use Entity Framework with repository pattern for data access.

## 6. Security Considerations
- **Authentication:** No bearer token required; refresh token validated in request body.
- **Authorization:** Endpoint accessible to any valid refresh token holder.
- **Token Security:** Implement token rotation (new refresh token on each use) to prevent replay attacks.
- **Input Validation:** Strict validation of refresh token format and presence.
- **Rate Limiting:** Implement rate limiting to prevent brute force attacks (e.g., via middleware).
- **HTTPS Enforcement:** Ensure all requests use HTTPS (handled by Azure hosting).
- **Audit Logging:** Log failed attempts to AuditLog for security monitoring.
- **Token Storage:** Refresh tokens stored securely in database with appropriate hashing if needed.

## 7. Error Handling
- **Validation Errors (400):** Invalid request format, missing refresh token - return detailed validation messages.
- **Authentication Errors (401):** Invalid/expired refresh token - log to AuditLog with minimal details.
- **Server Errors (500):** Database failures, token generation errors - log full exception details internally, return generic message.
- **Custom Exceptions:** Use `InvalidRefreshTokenException` for business logic validation failures.
- **Logging:** Failed attempts logged to AuditLog with EventType "RefreshTokenFailed".

## 8. Performance Considerations
- **Database Queries:** Single query to Users table for token validation; optimize with index on refresh token column.
- **Token Generation:** Use efficient JWT libraries in .NET; cache token signing keys if applicable.
- **Caching:** No heavy caching needed, but consider in-memory caching for user data if multiple requests.
- **Concurrency:** Handle concurrent refresh requests safely (e.g., via database transactions).
- **Scalability:** Stateless design supports horizontal scaling; Azure SQL handles load.

## 9. Implementation Steps
1. Create `RefreshTokenRequest` and `RefreshTokenResponse` DTOs in Application/DTOs/Auth/.
2. Add `RefreshTokenCommand` class in Application/ for command pattern.
3. Extend `IAuthService` interface with `RefreshTokenAsync(RefreshTokenCommand command)` method.
4. Implement the method in `AuthService` to validate refresh token, generate new tokens, and update database.
5. Add controller action in `AuthController` (Api/Controllers/AuthController.cs) with proper routing and model validation.
6. Implement input validation using model attributes and custom validators.
7. Add audit logging for failed attempts in the service layer.
8. Update dependency injection in Application/DependencyInjection.cs if new services are added.
9. Test the endpoint with unit tests (happy path, invalid token, expired token) and integration tests.
10. Document the endpoint in Swagger with detailed schemas and examples.