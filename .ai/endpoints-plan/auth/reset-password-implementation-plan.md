# API Endpoint Implementation Plan: Reset Password

## 1. Endpoint Overview
This endpoint allows users to reset their password using a reset token received via email from the forgot password process. It validates the token, checks password strength, updates the user's password hash and salt, invalidates the reset token, and logs the event. No authentication is required as the token serves as the authorization mechanism.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/auth/reset-password`
- **Parameters:**
  - Required: None (body-based)
  - Optional: None
- **Request Body:**
  ```json
  {
    "token": "string",  // Reset token from email
    "newPassword": "string"  // New password meeting strength requirements
  }
  ```

## 3. Used Types
- **Request DTO:** `ResetPasswordRequestDto` (Application/DTOs/Auth/)
  - Properties: `Token` (string), `NewPassword` (string)
  - Validation: Token required, not empty; NewPassword required, min length 8, contains uppercase, lowercase, digit, special char.
- **Response DTO:** Simple object with `Message` property (or use IActionResult with message).

## 4. Response Details
- **Success (200 OK):**
  ```json
  {
    "message": "Password reset successful"
  }
  ```
- **Error Responses:**
  - 400 Bad Request: Invalid token, expired token, or weak password
  - 404 Not Found: Token not found
  - 500 Internal Server Error: Server-side error

## 5. Data Flow
1. Controller receives request, binds to `ResetPasswordRequestDto`.
2. Validate DTO (FluentValidation).
3. Call `IAuthService.ResetPasswordAsync(requestDto)`.
4. Service: Query `Users` table by `PasswordResetToken`, check expiry.
5. If valid, hash new password, update user (clear token/expiry), save via `IUserRepository`.
6. Log success/failure to `AuditLog` via repository.
7. Return response.

## 6. Security Considerations
- **Authentication:** None; token-based authorization.
- **Authorization:** Token must match and not be expired.
- **Data Validation:** Strong password enforcement; token format validation.
- **Threats:** Implement rate limiting (e.g., 5 attempts per IP/hour); use HTTPS; invalidate token post-reset; log failed attempts for monitoring.
- **Encryption:** Passwords hashed with salt using existing `IPasswordHashingService`.

## 7. Error Handling
- **Invalid Token:** 400 with message "Invalid or expired reset token".
- **Token Not Found:** 404 with message "Reset token not found".
- **Weak Password:** 400 with validation errors.
- **DB Errors:** 500 with generic message; log details.
- **Rate Limiting:** 429 if implemented.

## 8. Performance Considerations
- DB query: Indexed lookup on `PasswordResetToken`.
- Hashing: Use efficient algorithm (e.g., PBKDF2 via `PasswordHashingService`).
- Rate limiting: In-memory cache or middleware to prevent abuse.
- No heavy computations; keep response time <1s.

## 9. Implementation Steps
1. Create `ResetPasswordRequestDto` in Application/DTOs/Auth/ with validation.
2. Update `IAuthService` to include `ResetPasswordAsync(ResetPasswordRequestDto requestDto)`.
3. Implement method in `AuthService`: validate token, hash password, update user, log audit.
4. Add endpoint in `AuthController`: POST /reset-password, call service.
5. Update `IUserRepository` if needed for token-based lookup.
6. Add audit logging in service for success/failure.
7. Test: Valid reset, invalid token, expired token, weak password, DB errors.
8. Add rate limiting middleware if not present.
9. Update Swagger documentation.