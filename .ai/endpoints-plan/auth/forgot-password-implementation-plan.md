# API Endpoint Implementation Plan: Forgot Password

## 1. Endpoint Overview
This endpoint initiates the password reset process for users who have forgotten their password. It accepts an email address, generates a secure reset token, updates the user's record in the database, and sends a reset email. The endpoint returns a generic success message regardless of whether the email exists to prevent email enumeration attacks. It follows Clean Architecture principles, separating concerns across API, Application, Domain, and Infrastructure layers.

## 2. Request Details
- **HTTP Method:** POST
- **URL Structure:** `/api/auth/forgot-password`
- **Authentication:** None required
- **Parameters:**
  - Required: None (body-based)
  - Optional: None
- **Request Body:**
  ```json
  {
    "email": "user@example.com"
  }
  ```
  - `email`: String, required, must be a valid email format.

## 3. Used Types
- **Request DTO:** `ForgotPasswordRequestDto` (in Application/DTOs/Auth/)
  - Properties: `string Email` with validation attributes (e.g., `[Required]`, `[EmailAddress]`).
- **Response:** Simple JSON object with message (no dedicated DTO needed).
- **Domain Entities:** `User` (for updating PasswordResetToken and PasswordResetTokenExpiry).
- **Infrastructure Services:** `IEmailService` for sending emails.

## 4. Response Details
- **Success Response (200 OK):**
  ```json
  {
    "message": "Password reset link sent to your email"
  }
  ```
- **Error Responses:**
  - `400 Bad Request`: Invalid email format.
  - `404 Not Found`: Email not found (though spec notes may return 200 for security).
  - `429 Too Many Requests`: Rate limiting exceeded.
  - `500 Internal Server Error`: Unexpected server error.

## 5. Data Flow
1. Request reaches the API controller (`AuthController`).
2. Controller validates the request DTO using model binding and validation.
3. If valid, controller calls `IAuthService.ForgotPasswordAsync(requestDto)`.
4. Service checks if user exists in `Users` table via repository.
5. If user exists, generates a secure reset token and expiry (e.g., 24 hours), updates `PasswordResetToken` and `PasswordResetTokenExpiry` in the database.
6. Logs the event to `AuditLog` table (EventType: "PasswordResetRequested").
7. Calls `IEmailService` to send the reset email with the token.
8. Returns success message (200 OK) regardless of user existence.
9. If email service fails, logs error and returns 500.

## 6. Security Considerations
- **Authentication/Authorization:** No authentication required, but implement rate limiting (e.g., 5 requests per hour per IP/email) using ASP.NET middleware to prevent abuse.
- **Data Validation:** Strict email format validation; sanitize inputs to prevent injection.
- **Token Security:** Use cryptographically secure random generation for reset tokens; store hashed versions if needed, but since tokens are sent via email, ensure secure transmission.
- **Email Enumeration Prevention:** Always return 200 OK with the same message to avoid revealing user existence.
- **HTTPS Enforcement:** Ensure endpoint is served over HTTPS to protect email data in transit.
- **Rate Limiting:** Implement throttling to mitigate brute-force attacks.

## 7. Error Handling
- **Validation Errors (400):** Invalid email format; return detailed error messages for client correction.
- **Not Found (404):** If email doesn't exist, but return 200 OK per security best practices.
- **Rate Limiting (429):** Exceeded request limits; include retry-after header.
- **Server Errors (500):** Email service failure or database issues; log details internally without exposing to client.
- **Exception Handling:** Wrap service calls in try-catch; use custom exceptions (e.g., `EmailServiceException`) for specific failures.

## 8. Performance Considerations
- **Database Queries:** Single query to check user existence and update token; use indexed `Users.Email` for fast lookups.
- **Email Sending:** Asynchronous email sending to avoid blocking the response; use a background job if volume is high.
- **Caching:** No caching needed, as this is a write operation.
- **Scalability:** Stateless operation; ensure email service can handle load (e.g., queue emails).
- **Monitoring:** Track response times and failure rates for optimization.

## 9. Implementation Steps
1. Create `ForgotPasswordRequestDto` in `Application/DTOs/Auth/` with validation.
2. Extend `IAuthService` with `ForgotPasswordAsync(ForgotPasswordRequestDto requestDto)`.
3. Implement the method in `AuthService`: validate, generate token, update user, log to audit, send email.
4. Add `IEmailService` interface and implementation in Infrastructure (e.g., using SMTP or Azure services).
5. Update `AuthController` with `ForgotPassword` action: validate model, call service, return response.
6. Add rate limiting middleware or attributes to the endpoint.
7. Test with unit tests (mock email service) and integration tests (check database updates).
8. Update Swagger documentation with examples and error responses.
9. Deploy and monitor for security and performance.