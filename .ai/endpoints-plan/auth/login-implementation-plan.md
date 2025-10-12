# API Endpoint Implementation Plan: Login

## 1. Endpoint Overview

The Login endpoint authenticates users with their email and password credentials, returning JWT access and refresh tokens upon successful authentication. This is a public endpoint that requires no prior authentication. It implements security best practices including constant-time password verification, audit logging, and protection against user enumeration attacks.

**Key Characteristics:**
- Public endpoint (no authentication required)
- Returns JWT tokens for subsequent authenticated requests
- Access token lifetime: 15 minutes (900 seconds)
- Refresh token lifetime: 7 days
- Logs all authentication attempts for security monitoring
- Prevents user enumeration through consistent error messaging

## 2. Request Details

- **HTTP Method:** `POST`
- **URL Structure:** `/api/auth/login`
- **Content-Type:** `application/json`

### Parameters

#### Required Parameters (Request Body):
| Parameter | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| `email` | string | Required, valid email format | User's email address (case-insensitive) |
| `password` | string | Required, non-empty | User's plaintext password |

#### Optional Parameters:
None

### Request Body Example:
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

### Validation Rules:
- **Email:**
  - Required (400 error if missing)
  - Must be valid email format (400 error if invalid)
  - Will be normalized to lowercase and trimmed before lookup
  - Maximum length: 255 characters (database constraint)
  
- **Password:**
  - Required (400 error if missing)
  - No minimum length enforced on login (validation is at registration)
  - Plaintext password - will be hashed for comparison

## 3. Used Types

### Request DTOs

**LoginRequest** (Existing - needs validation attributes added)
```csharp
// Location: Server/Application/DTOs/Auth/LoginRequest.cs
public record LoginRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}
```

### Response DTOs

**LoginResponse** (Existing)
```csharp
// Location: Server/Application/DTOs/Auth/LoginResponse.cs
public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserDto User { get; init; }
}
```

**UserDto** (Existing)
```csharp
// Location: Server/Application/DTOs/Users/UserDto.cs
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}
```

**ErrorResponse** (Existing)
```csharp
// Location: Server/Application/DTOs/Common/ErrorResponse.cs
public record ErrorResponse
{
    public required string Error { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
```

### Domain Entities

**User** (Existing)
```csharp
// Location: Server/Domain/Entities/User.cs
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }
}
```

**AuditLog** (Existing)
```csharp
// Location: Server/Domain/Entities/AuditLog.cs
public class AuditLog
{
    public long Id { get; set; }
    public Guid? UserId { get; set; }
    public string EventType { get; set; }
    public DateTime EventTimestamp { get; set; }
    public string? Details { get; set; }
}
```

### Service Interfaces (To Be Created/Modified)

**IAuthService** (Needs new method)
```csharp
// Location: Server/Application/Interfaces/IAuthService.cs
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    // NEW METHOD TO ADD:
    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with JWT tokens and user information</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
```

**IJwtTokenService** (New service interface needed)
```csharp
// Location: Server/Application/Interfaces/IJwtTokenService.cs
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for authenticated user
    /// </summary>
    /// <param name="user">User entity with roles</param>
    /// <returns>JWT access token string</returns>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Generates a JWT refresh token for token renewal
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>JWT refresh token string</returns>
    string GenerateRefreshToken(Guid userId);
    
    /// <summary>
    /// Gets the access token expiration time in seconds
    /// </summary>
    int AccessTokenExpiresIn { get; }
}
```

## 4. Response Details

### Success Response (200 OK)

**Status Code:** `200 OK`

**Response Body:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6InVzZXJAZXhhbXBsZS5jb20iLCJyb2xlIjoiVXNlciIsImV4cCI6MTcyODc0NjQwMH0.signature",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJ0eXBlIjoicmVmcmVzaCIsImV4cCI6MTcyOTM1MDQwMH0.signature",
  "expiresIn": 900,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "createdAt": "2025-10-01T10:30:00Z",
    "roles": ["User"]
  }
}
```

**Response Headers:**
```
Content-Type: application/json
```

### Error Responses

#### 400 Bad Request - Validation Errors

**Scenario:** Missing or invalid input fields

```json
{
  "error": "ValidationError",
  "message": "One or more validation errors occurred",
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password is required"]
  }
}
```

#### 401 Unauthorized - Invalid Credentials

**Scenario:** User not found OR incorrect password (same message for security)

```json
{
  "error": "InvalidCredentials",
  "message": "Invalid email or password"
}
```

**Important:** This same response is returned whether:
- The email doesn't exist in the database
- The password is incorrect for an existing user

This prevents user enumeration attacks by not revealing which emails are registered.

#### 500 Internal Server Error

**Scenario:** Database connection error, token generation failure, or other system errors

```json
{
  "error": "InternalError",
  "message": "An error occurred during login. Please try again later."
}
```

## 5. Data Flow

### Sequence Diagram

```
Client → AuthController → AuthService → UserRepository → Database
                       ↓
                  PasswordHashingService
                       ↓
                  JwtTokenService
                       ↓
                  AuditLogRepository → Database
                       ↓
Client ← LoginResponse
```

### Detailed Flow

1. **Client Request** → Controller receives POST request to `/api/auth/login`

2. **Controller Layer** (`AuthController.Login`)
   - Validate `ModelState` for required fields and email format
   - Return 400 with validation errors if invalid
   - Pass `LoginRequest` to `AuthService.LoginAsync()`
   - Handle exceptions and map to appropriate HTTP status codes
   - Return 200 with `LoginResponse` on success

3. **Service Layer** (`AuthService.LoginAsync`)
   - Normalize email: trim whitespace and convert to lowercase
   - Query database via `UserRepository.GetByEmailAsync()` (includes roles)
   - If user not found:
     - Log failed attempt to AuditLog (`LoginFailed` event)
     - Throw `UnauthorizedAccessException` with generic message
   - Verify password using `PasswordHashingService.VerifyPassword()`
   - If password incorrect:
     - Log failed attempt to AuditLog (`LoginFailed` event)
     - Throw `UnauthorizedAccessException` with same generic message
   - Generate access token via `JwtTokenService.GenerateAccessToken()`
   - Generate refresh token via `JwtTokenService.GenerateRefreshToken()`
   - Log successful login to AuditLog (`LoginSuccessful` event)
   - Map `User` entity to `UserDto` (extract role names from UserRoles)
   - Return `LoginResponse` with tokens and user data

4. **Repository Layer** (`UserRepository.GetByEmailAsync`)
   - Execute EF Core query with `.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)`
   - Use indexed lookup on `Users(Email)` column
   - Return `User` entity or null

5. **Password Verification** (`PasswordHashingService.VerifyPassword`)
   - Hash provided password with stored salt using PBKDF2-HMAC-SHA256
   - Compare computed hash with stored hash using constant-time comparison
   - Return true/false

6. **Token Generation** (`JwtTokenService`)
   - Create JWT claims: sub (userId), email, roles, exp (expiration)
   - Sign token with symmetric key (from configuration)
   - Access token expiry: 15 minutes (900 seconds)
   - Refresh token expiry: 7 days (604800 seconds)

7. **Audit Logging** (`AuditLogRepository.AddAsync`)
   - Create `AuditLog` entry with event type, userId, timestamp, and details
   - Serialize details as JSON
   - Failures in audit logging are logged but don't break the main flow

### Database Queries

**Query 1: Fetch User with Roles**
```csharp
await _dbContext.Users
    .Include(u => u.UserRoles)
    .ThenInclude(ur => ur.Role)
    .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
```

**Query 2: Insert Audit Log**
```csharp
await _dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
await _dbContext.SaveChangesAsync(cancellationToken);
```

### External Dependencies

- **Database**: Azure SQL for user authentication data
- **Configuration**: JWT secret key, token expiration times
- **Logging**: ILogger for application logging

## 6. Security Considerations

### Authentication & Authorization

- **Endpoint Access:** Public (no authentication required)
- **Rate Limiting:** Consider implementing rate limiting to prevent brute force attacks
  - Recommendation: 5 failed attempts per IP per 15 minutes
  - Can be implemented using ASP.NET Core middleware or reverse proxy

### Password Security

- **Hashing Algorithm:** PBKDF2-HMAC-SHA256 (already implemented)
- **Iterations:** 100,000 (OWASP 2024 recommendation)
- **Salt:** 128 bytes, cryptographically random
- **Hash:** 256 bytes
- **Verification:** Constant-time comparison to prevent timing attacks

### JWT Token Security

**Access Token:**
- Lifetime: 15 minutes (short-lived to minimize exposure)
- Claims: userId (sub), email, roles
- Algorithm: HS256 (HMAC SHA-256)
- Storage: Client-side (localStorage or sessionStorage)

**Refresh Token:**
- Lifetime: 7 days
- Claims: userId (sub), type (refresh)
- Algorithm: HS256
- Purpose: Obtain new access tokens without re-authentication
- Consider: Storing refresh tokens in database for revocation capability

**Token Best Practices:**
- Use HTTPS-only for token transmission
- Consider HttpOnly cookies for web clients
- Implement token refresh mechanism (separate endpoint)
- Consider token revocation/blacklist for logout
- Rotate refresh tokens on each use

### Input Validation & Sanitization

- Email validated using DataAnnotations `[EmailAddress]` attribute
- Maximum length enforced (255 characters)
- Email normalized (lowercase, trimmed) before database lookup
- Password accepted as-is (no modification before hashing)
- Model binding handles basic injection prevention

### Protection Against Attacks

**1. User Enumeration Prevention:**
- Return identical error message for "user not found" and "wrong password"
- Use same response time for both scenarios (constant-time password verification)
- Don't reveal which emails are registered in error messages

**2. Brute Force Attack Prevention:**
- Audit logging of all login attempts (successful and failed)
- Monitor failed login patterns in AuditLog table
- Consider: Account lockout after N failed attempts (future enhancement)
- Consider: CAPTCHA after multiple failed attempts
- Recommendation: Implement rate limiting at API gateway or middleware level

**3. Timing Attack Prevention:**
- Password verification uses `CryptographicOperations.FixedTimeEquals()`
- Constant-time comparison prevents timing-based password guessing

**4. SQL Injection Prevention:**
- Entity Framework Core uses parameterized queries automatically
- No raw SQL in authentication flow

**5. Credential Stuffing Prevention:**
- Audit logging enables detection of suspicious patterns
- Strong password requirements at registration (separate endpoint)
- Consider: Integration with breach databases (HaveIBeenPwned API)

**6. Session Hijacking Prevention:**
- Short-lived access tokens (15 minutes)
- Refresh token rotation on each use (implement in refresh endpoint)
- HTTPS-only transmission
- Consider: Token binding to client IP or user agent

### Audit Logging

All authentication events are logged to the `AuditLog` table:

**Successful Login:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "eventType": "LoginSuccessful",
  "eventTimestamp": "2025-10-12T10:30:00Z",
  "details": "{\"Email\":\"user@example.com\"}"
}
```

**Failed Login:**
```json
{
  "userId": null,
  "eventType": "LoginFailed",
  "eventTimestamp": "2025-10-12T10:30:00Z",
  "details": "{\"Email\":\"user@example.com\",\"Reason\":\"InvalidCredentials\"}"
}
```

**Login Error:**
```json
{
  "userId": null,
  "eventType": "LoginError",
  "eventTimestamp": "2025-10-12T10:30:00Z",
  "details": "{\"Email\":\"user@example.com\",\"ErrorMessage\":\"Database connection timeout\"}"
}
```

### Security Headers

Ensure the following headers are configured at the API level:
- `Strict-Transport-Security: max-age=31536000; includeSubDomains`
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Content-Security-Policy: default-src 'self'`

## 7. Error Handling

### Error Scenarios

| Scenario | HTTP Status | Error Code | User Message | Logged Details | Action |
|----------|-------------|------------|--------------|----------------|--------|
| Missing email | 400 | `ValidationError` | "Email is required" | N/A | Return validation errors |
| Missing password | 400 | `ValidationError` | "Password is required" | N/A | Return validation errors |
| Invalid email format | 400 | `ValidationError` | "Invalid email format" | N/A | Return validation errors |
| Email too long (>255) | 400 | `ValidationError` | "Email cannot exceed 255 characters" | N/A | Return validation errors |
| User not found | 401 | `InvalidCredentials` | "Invalid email or password" | Email, Reason: "UserNotFound" | Log to AuditLog |
| Wrong password | 401 | `InvalidCredentials` | "Invalid email or password" | Email, UserId, Reason: "InvalidPassword" | Log to AuditLog |
| Database connection error | 500 | `InternalError` | "An error occurred during login. Please try again later." | Exception message, email | Log error + AuditLog |
| Token generation failure | 500 | `InternalError` | "An error occurred during login. Please try again later." | Exception message, userId | Log error + AuditLog |
| Audit log write failure | N/A | N/A | N/A | Exception message | Log error only (don't break flow) |

### Exception Handling Strategy

**Controller Layer:**
```csharp
try
{
    var response = await _authService.LoginAsync(request, cancellationToken);
    return Ok(response);
}
catch (UnauthorizedAccessException ex)
{
    // User not found or invalid password
    return Unauthorized(new ErrorResponse
    {
        Error = "InvalidCredentials",
        Message = "Invalid email or password"
    });
}
catch (Exception ex)
{
    // Log unexpected errors
    _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
    
    return StatusCode(500, new ErrorResponse
    {
        Error = "InternalError",
        Message = "An error occurred during login. Please try again later."
    });
}
```

**Service Layer:**
```csharp
try
{
    var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
    
    if (user == null)
    {
        await LogAuditAsync(null, "LoginFailed", new { Email = email, Reason = "UserNotFound" }, cancellationToken);
        throw new UnauthorizedAccessException("Invalid credentials");
    }
    
    if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
    {
        await LogAuditAsync(user.Id, "LoginFailed", new { Email = email, Reason = "InvalidPassword" }, cancellationToken);
        throw new UnauthorizedAccessException("Invalid credentials");
    }
    
    // Continue with token generation...
}
catch (UnauthorizedAccessException)
{
    throw; // Re-throw authentication errors
}
catch (Exception ex)
{
    _logger.LogError(ex, "Database error during login for email: {Email}", email);
    await LogAuditAsync(null, "LoginError", new { Email = email, ErrorMessage = ex.Message }, cancellationToken);
    throw; // Re-throw as internal error
}
```

### Model Validation

ASP.NET Core automatic model validation handles:
- Required field validation
- Email format validation
- String length validation
- Custom validation attributes (if added)

Example validation response:
```json
{
  "error": "ValidationError",
  "message": "One or more validation errors occurred",
  "errors": {
    "Email": ["Invalid email format", "Email cannot exceed 255 characters"],
    "Password": ["Password is required"]
  }
}
```

### Logging Strategy

**Application Logging (ILogger):**
- Information: Successful login attempts
- Warning: Failed login attempts (after threshold)
- Error: Database errors, token generation errors
- Critical: Missing roles, configuration errors

**Audit Logging (AuditLog table):**
- All successful logins
- All failed login attempts (for security monitoring)
- Errors during login process

**Log Levels by Scenario:**
```csharp
// Successful login
_logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}", user.Email, user.Id);

// Failed login attempt
_logger.LogWarning("Login failed for email: {Email}, Reason: {Reason}", email, reason);

// Database error
_logger.LogError(ex, "Database error during login for email: {Email}", email);

// Token generation error
_logger.LogError(ex, "Failed to generate JWT token for user: {UserId}", userId);

// Audit logging failure (non-critical)
_logger.LogError(ex, "Failed to write audit log for event: {EventType}", eventType);
```

## 8. Performance Considerations

### Potential Bottlenecks

1. **Database Query Performance:**
   - **Issue:** User lookup with role joins could be slow for large user bases
   - **Mitigation:** 
     - Indexed lookup on `Users(Email)` (non-clustered index already planned)
     - EF Core query optimization with explicit `.Include()` for eager loading
     - Consider caching user data for frequently logged-in users (future)

2. **Password Hashing:**
   - **Issue:** PBKDF2 with 100,000 iterations is computationally expensive (by design)
   - **Impact:** ~50-100ms per login attempt
   - **Mitigation:** 
     - This is intentional for security
     - Protects against brute force attacks
     - Cannot be significantly optimized without compromising security

3. **Token Generation:**
   - **Issue:** JWT signing with HMAC-SHA256
   - **Impact:** Minimal (<5ms)
   - **Mitigation:** Not a concern, already very fast

4. **Audit Logging:**
   - **Issue:** Database insert on every login attempt
   - **Mitigation:**
     - Async/await prevents blocking
     - Failures don't break login flow
     - Consider: Background job queue for audit logs (future)

### Optimization Strategies

**1. Database Query Optimization:**
```csharp
// Use AsNoTracking for read-only queries
await _dbContext.Users
    .AsNoTracking()
    .Include(u => u.UserRoles)
    .ThenInclude(ur => ur.Role)
    .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
```

**2. Compiled Queries (Future Enhancement):**
```csharp
// Pre-compile frequently used queries
private static readonly Func<ApplicationDbContext, string, CancellationToken, Task<User?>> 
    GetUserByEmailQuery = EF.CompileAsyncQuery(
        (ApplicationDbContext context, string email, CancellationToken ct) =>
            context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => u.Email == email));
```

**3. Response Caching:**
- Not applicable for login endpoint (user-specific, security-sensitive)
- Do NOT cache authentication responses

**4. Connection Pooling:**
- EF Core automatically uses connection pooling
- Ensure proper DbContext lifetime (scoped per request)

**5. Asynchronous Processing:**
- All I/O operations use async/await
- Prevents thread blocking
- Improves scalability under load

### Scalability Considerations

**Current Load Estimate:**
- Assumption: 1,000 users, average 5 logins per user per week
- Weekly logins: 5,000
- Daily logins: ~715
- Peak hour logins: ~100

**Performance Targets:**
- Average response time: <500ms (including password hashing)
- 95th percentile: <1000ms
- Maximum concurrent requests: 50

**Horizontal Scaling:**
- Stateless endpoint (no server-side session)
- Can be deployed across multiple instances
- Load balancer distributes traffic
- Database is shared bottleneck (Azure SQL handles scaling)

**Future Enhancements:**
- Implement distributed caching (Redis) for user sessions
- Rate limiting middleware to prevent abuse
- CDN for static content (not applicable to this endpoint)
- Database read replicas for high read volumes

### Monitoring Recommendations

**Key Metrics to Track:**
1. Login success rate (target: >95%)
2. Average response time (target: <500ms)
3. Failed login attempts per IP (alert threshold: >10/hour)
4. Database query duration (alert threshold: >200ms)
5. Token generation duration (alert threshold: >50ms)
6. Concurrent login requests (alert threshold: >100)

**Alerting:**
- Alert on sustained 401 errors from single IP (potential brute force)
- Alert on 500 errors (system issues)
- Alert on average response time >1000ms (performance degradation)
- Alert on database connection failures

## 9. Implementation Steps

### Step 1: Add Validation Attributes to LoginRequest DTO

**File:** `Server/Application/DTOs/Auth/LoginRequest.cs`

**Action:** Add data annotations for validation

```csharp
using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for user authentication
/// Contains credentials to validate against User entity
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public required string Email { get; init; }

    /// <summary>
    /// User's password for authentication
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}
```

### Step 2: Create JWT Token Service Interface

**File:** `Server/Application/Interfaces/IJwtTokenService.cs` (NEW)

**Action:** Define interface for JWT token generation

```csharp
using Trivare.Domain.Entities;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for JWT token generation and management
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for authenticated user
    /// Token includes user ID, email, and roles as claims
    /// </summary>
    /// <param name="user">User entity with roles loaded</param>
    /// <returns>JWT access token string</returns>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Generates a JWT refresh token for token renewal
    /// Token includes user ID and refresh token type
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>JWT refresh token string</returns>
    string GenerateRefreshToken(Guid userId);
    
    /// <summary>
    /// Gets the access token expiration time in seconds
    /// </summary>
    int AccessTokenExpiresIn { get; }
}
```

### Step 3: Implement JWT Token Service

**File:** `Server/Application/Services/JwtTokenService.cs` (NEW)

**Action:** Implement JWT token generation using System.IdentityModel.Tokens.Jwt

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for JWT token generation
/// Uses HMAC-SHA256 for token signing
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtTokenService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "Trivare";
        _audience = configuration["Jwt:Audience"] ?? "Trivare";
        _accessTokenExpirationMinutes = 15; // 15 minutes
        _refreshTokenExpirationDays = 7; // 7 days
    }

    public int AccessTokenExpiresIn => _accessTokenExpirationMinutes * 60; // Convert to seconds

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var userRole in user.UserRoles)
        {
            if (userRole.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("type", "refresh"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**NuGet Packages Required:**
- `System.IdentityModel.Tokens.Jwt`
- `Microsoft.IdentityModel.Tokens`

### Step 4: Add JWT Configuration to appsettings.json

**File:** `Server/Api/appsettings.json`

**Action:** Add JWT configuration section

```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-here-minimum-32-characters-for-hs256",
    "Issuer": "Trivare",
    "Audience": "Trivare"
  },
  // ... other configuration
}
```

**File:** `Server/Api/appsettings.Development.json`

**Action:** Add development JWT secret (use strong random key in production)

```json
{
  "Jwt": {
    "SecretKey": "development-secret-key-change-in-production-minimum-32-chars",
    "Issuer": "Trivare",
    "Audience": "Trivare"
  },
  // ... other configuration
}
```

**Security Note:** In production, store JWT secret in Azure Key Vault or environment variables.

### Step 5: Register JWT Service in Dependency Injection

**File:** `Server/Application/DependencyInjection.cs`

**Action:** Register IJwtTokenService in DI container

```csharp
// Add to existing service registration
services.AddScoped<IJwtTokenService, JwtTokenService>();
```

### Step 6: Update IAuthService Interface

**File:** `Server/Application/Interfaces/IAuthService.cs`

**Action:** Add LoginAsync method to interface

```csharp
using Trivare.Application.DTOs.Auth;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for authentication-related operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration details including email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration response with user information</returns>
    /// <exception cref="Exceptions.EmailAlreadyExistsException">Thrown when email is already registered</exception>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authenticates a user with email and password credentials
    /// Returns JWT access and refresh tokens upon successful authentication
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with JWT tokens and user information</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
```

### Step 7: Implement LoginAsync in AuthService

**File:** `Server/Application/Services/AuthService.cs`

**Action:** Add LoginAsync method implementation

```csharp
// Add to constructor parameters:
private readonly IJwtTokenService _jwtTokenService;

public AuthService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IAuditLogRepository auditLogRepository,
    IPasswordHashingService passwordHashingService,
    IJwtTokenService jwtTokenService, // NEW
    ILogger<AuthService> logger)
{
    _userRepository = userRepository;
    _roleRepository = roleRepository;
    _auditLogRepository = auditLogRepository;
    _passwordHashingService = passwordHashingService;
    _jwtTokenService = jwtTokenService; // NEW
    _logger = logger;
}

/// <summary>
/// Authenticates a user with email and password
/// </summary>
public async Task<LoginResponse> LoginAsync(
    LoginRequest request, 
    CancellationToken cancellationToken = default)
{
    // Normalize email to lowercase and trim whitespace
    var email = request.Email.Trim().ToLowerInvariant();

    try
    {
        // Fetch user with roles
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", email);
            
            // Log failed attempt
            await LogAuditAsync(null, "LoginFailed", new
            {
                Email = email,
                Reason = "UserNotFound"
            }, cancellationToken);

            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            _logger.LogWarning("Login failed - invalid password: {Email}, UserId: {UserId}", 
                email, user.Id);
            
            // Log failed attempt with user ID
            await LogAuditAsync(user.Id, "LoginFailed", new
            {
                Email = email,
                Reason = "InvalidPassword"
            }, cancellationToken);

            throw new UnauthorizedAccessException("Invalid credentials");
        }

        // Generate JWT tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        _logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}", 
            user.Email, user.Id);

        // Log successful login
        await LogAuditAsync(user.Id, "LoginSuccessful", new
        {
            Email = user.Email
        }, cancellationToken);

        // Map to UserDto
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "Unknown").ToList()
        };

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.AccessTokenExpiresIn,
            User = userDto
        };
    }
    catch (UnauthorizedAccessException)
    {
        throw; // Re-throw authentication errors
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during login for email: {Email}", email);
        
        // Log error
        await LogAuditAsync(null, "LoginError", new
        {
            Email = email,
            ErrorMessage = ex.Message
        }, cancellationToken);

        throw;
    }
}

// LogAuditAsync method already exists in the class from Register implementation
```

### Step 8: Implement Login Endpoint in AuthController

**File:** `Server/Api/Controllers/AuthController.cs`

**Action:** Add Login endpoint

```csharp
/// <summary>
/// Authenticate user and receive JWT tokens
/// </summary>
/// <param name="request">Login credentials (email and password)</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>JWT tokens and user information</returns>
/// <response code="200">Login successful</response>
/// <response code="400">Invalid input data or validation errors</response>
/// <response code="401">Invalid credentials</response>
/// <response code="500">Internal server error</response>
[HttpPost("login")]
[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<LoginResponse>> Login(
    [FromBody] LoginRequest request,
    CancellationToken cancellationToken)
{
    // Validate model state
    if (!ModelState.IsValid)
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
            );

        return BadRequest(new ErrorResponse
        {
            Error = "ValidationError",
            Message = "One or more validation errors occurred",
            Errors = errors
        });
    }

    try
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
    catch (UnauthorizedAccessException)
    {
        // User not found or invalid password - same message for security
        return Unauthorized(new ErrorResponse
        {
            Error = "InvalidCredentials",
            Message = "Invalid email or password"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
        
        return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
        {
            Error = "InternalError",
            Message = "An error occurred during login. Please try again later."
        });
    }
}
```

### Step 9: Update UserRepository GetByEmailAsync (if needed)

**File:** `Server/Infrastructure/Repositories/UserRepository.cs`

**Action:** Ensure GetByEmailAsync includes roles with eager loading

```csharp
public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
{
    return await _context.Users
        .AsNoTracking() // Read-only query optimization
        .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}
```

**Note:** If this implementation doesn't exist yet, create the UserRepository class following the existing repository pattern.

### Step 10: Add NuGet Packages to Application Project

**File:** `Server/Application/Application.csproj`

**Action:** Add required NuGet packages

```xml
<ItemGroup>
  <PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.3" />
  <PackageReference Include="Microsoft.IdentityModel.Tokens" Version="7.0.3" />
</ItemGroup>
```

**Terminal Command:**
```bash
cd Server/Application
dotnet add package System.IdentityModel.Tokens.Jwt
dotnet add package Microsoft.IdentityModel.Tokens
```

### Step 11: Configure JWT Authentication Middleware (Optional - for protected endpoints)

**File:** `Server/Api/Program.cs`

**Action:** Add JWT authentication configuration (needed for future protected endpoints)

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// Add before builder.Build()
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured"))
        ),
        ClockSkew = TimeSpan.Zero // Remove default 5-minute grace period
    };
});

builder.Services.AddAuthorization();

// Add after app is built, before app.MapControllers()
app.UseAuthentication();
app.UseAuthorization();
```

### Step 12: Test the Login Endpoint

**File:** `Server/Api/Api.http` (add test request)

**Action:** Add manual test request

```http
### Login - Successful
POST {{host}}/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}

### Login - Invalid Credentials
POST {{host}}/api/auth/login
Content-Type: application/json

{
  "email": "nonexistent@example.com",
  "password": "wrongpassword"
}

### Login - Validation Error
POST {{host}}/api/auth/login
Content-Type: application/json

{
  "email": "invalid-email",
  "password": ""
}
```

### Step 13: Create Integration Tests (Optional but Recommended)

**File:** `Server/Tests/Api.Tests/Controllers/AuthControllerTests.cs` (NEW)

**Action:** Create integration tests for login endpoint

```csharp
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Trivare.Application.DTOs.Auth;
using Xunit;

namespace Trivare.Api.Tests.Controllers;

public class AuthControllerLoginTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerLoginTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse.ExpiresIn.Should().Be(900);
        loginResponse.User.Should().NotBeNull();
        loginResponse.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "AnyPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("InvalidCredentials");
        errorResponse.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("InvalidCredentials");
        errorResponse.Message.Should().Be("Invalid email or password");
    }

    [Fact]
    public async Task Login_WithMissingEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new { password = "ValidPassword123!" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("ValidationError");
        errorResponse.Errors.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "invalid-email-format",
            Password = "ValidPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        errorResponse.Should().NotBeNull();
        errorResponse!.Error.Should().Be("ValidationError");
        errorResponse.Errors.Should().ContainKey("Email");
    }
}
```

### Step 14: Update API Documentation (Swagger)

**File:** `Server/Api/Program.cs`

**Action:** Ensure Swagger is configured to document the login endpoint (should be automatic with XML comments)

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Trivare API",
        Version = "v1",
        Description = "REST API for Trivare trip planning application"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Add JWT authentication to Swagger (for future protected endpoints)
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

### Step 15: Verify Database Index on Users.Email

**Action:** Ensure non-clustered index exists on `Users(Email)` column for fast lookups

**File:** Check existing migrations or create new migration

```csharp
migrationBuilder.CreateIndex(
    name: "IX_Users_Email",
    table: "Users",
    column: "Email",
    unique: true);
```

**Note:** This index should already exist as part of the UNIQUE constraint on the Email column.

## Summary

This implementation plan provides comprehensive guidance for implementing the Login endpoint, including:

- ✅ JWT token generation with access and refresh tokens
- ✅ Secure password verification with constant-time comparison
- ✅ Comprehensive audit logging for security monitoring
- ✅ Protection against user enumeration attacks
- ✅ Input validation and error handling
- ✅ Clean Architecture separation of concerns
- ✅ Performance optimization with indexed queries
- ✅ Integration testing recommendations
- ✅ Swagger/OpenAPI documentation

**Estimated Implementation Time:** 4-6 hours (including testing)

**Dependencies:**
- JWT NuGet packages
- Existing User and AuditLog repositories
- Password hashing service (already implemented)

**Follow-up Endpoints:**
- Refresh token endpoint (to get new access tokens)
- Logout endpoint (optional, for token revocation)
- Password reset endpoints (already planned)
