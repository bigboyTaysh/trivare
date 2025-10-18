# API Endpoint Implementation Plan: User Registration

## 1. Endpoint Overview

The User Registration endpoint allows new users to create an account in the Trivare system. This is a public endpoint that accepts an email and password, validates the input, creates a new user record with hashed credentials, assigns the default "User" role, and returns the newly created user information.

**Key Responsibilities:**
- Validate email format and uniqueness
- Enforce strong password requirements
- Securely hash password with salt using PBKDF2 algorithm
- Create user entity in the database
- Assign default "User" role
- Log registration events to AuditLog
- Return user information (excluding sensitive data)

## 2. Request Details

### HTTP Method
`POST`

### URL Structure
```
/api/auth/register
```

### Parameters

#### Required Body Parameters
| Parameter | Type | Constraints | Description |
|-----------|------|-------------|-------------|
| `email` | `string` | Required, Valid email format, Max 255 chars, Unique | User's email address |
| `password` | `string` | Required, Min 8 chars, Must contain uppercase, lowercase, number, and special character | User's password |

#### Optional Parameters
None

### Request Body Example
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

### Content-Type
`application/json`

### Authentication
None (Public endpoint)

## 3. Used Types

### DTOs (Application Layer)

#### RegisterRequest (Input DTO)
**Location:** `Server/Application/DTOs/Auth/RegisterRequest.cs`

**Status:** Already exists

**Properties:**
- `Email` (string, required): User's email address
- `Password` (string, required): User's password

**Validation Attributes Required:**
```csharp
[Required(ErrorMessage = "Email is required")]
[EmailAddress(ErrorMessage = "Invalid email format")]
[MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
public required string Email { get; init; }

[Required(ErrorMessage = "Password is required")]
[MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
[RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]",
    ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
public required string Password { get; init; }
```

#### RegisterResponse (Output DTO)
**Location:** `Server/Application/DTOs/Auth/RegisterResponse.cs`

**Status:** Already exists

**Properties:**
- `Id` (Guid): Unique user identifier
- `Email` (string): User's email address
- `CreatedAt` (DateTime): Account creation timestamp

### Domain Entities

#### User Entity
**Location:** `Server/Domain/Entities/User.cs`

**Status:** Already exists

**Relevant Properties:**
- `Id` (Guid): Primary key
- `Email` (string): User's email
- `PasswordHash` (byte[]): Hashed password (256 bytes)
- `PasswordSalt` (byte[]): Password salt (128 bytes)
- `CreatedAt` (DateTime): Creation timestamp
- `UserRoles` (ICollection<UserRole>): User's roles

#### Role Entity
**Location:** `Server/Domain/Entities/Role.cs`

**Status:** Already exists

**Properties:**
- `Id` (Guid): Primary key
- `Name` (string): Role name (e.g., "User")

#### UserRole Entity
**Location:** `Server/Domain/Entities/UserRole.cs`

**Status:** Already exists (linking table)

#### AuditLog Entity
**Location:** `Server/Domain/Entities/AuditLog.cs`

**Status:** Already exists

**Properties:**
- `Id` (long): Primary key
- `UserId` (Guid?): User identifier (nullable)
- `EventType` (string): Event type (e.g., "UserRegistered")
- `EventTimestamp` (DateTime): Event timestamp
- `Details` (string?): Additional event details (JSON)

### Service Interfaces (Application Layer)

#### IAuthService
**Location:** `Server/Application/Services/IAuthService.cs`

**Status:** To be created

**Methods:**
```csharp
Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
```

#### IPasswordHashingService
**Location:** `Server/Application/Services/IPasswordHashingService.cs`

**Status:** To be created

**Methods:**
```csharp
(byte[] Hash, byte[] Salt) HashPassword(string password);
bool VerifyPassword(string password, byte[] hash, byte[] salt);
```

### Repository Interfaces (Application Layer)

#### IUserRepository
**Location:** `Server/Application/Repositories/IUserRepository.cs`

**Status:** To be created

**Methods:**
```csharp
Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
```

#### IRoleRepository
**Location:** `Server/Application/Repositories/IRoleRepository.cs`

**Status:** To be created

**Methods:**
```csharp
Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
```

#### IAuditLogRepository
**Location:** `Server/Application/Repositories/IAuditLogRepository.cs`

**Status:** To be created

**Methods:**
```csharp
Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
```

## 4. Response Details

### Success Response (201 Created)

**Status Code:** `201 Created`

**Headers:**
```
Content-Type: application/json
Location: /api/users/{userId}
```

**Body:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "createdAt": "2025-10-12T10:30:00Z"
}
```

### Error Responses

#### 400 Bad Request - Invalid Email Format
```json
{
  "error": "ValidationError",
  "message": "Invalid email format",
  "errors": {
    "Email": ["Invalid email format"]
  }
}
```

#### 400 Bad Request - Weak Password
```json
{
  "error": "ValidationError",
  "message": "Password does not meet security requirements",
  "errors": {
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain uppercase, lowercase, number, and special character"
    ]
  }
}
```

#### 400 Bad Request - Missing Required Fields
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

#### 400 Bad Request - Email Too Long
```json
{
  "error": "ValidationError",
  "message": "Email exceeds maximum length",
  "errors": {
    "Email": ["Email must not exceed 255 characters"]
  }
}
```

#### 409 Conflict - Email Already Exists
```json
{
  "error": "EmailAlreadyExists",
  "message": "An account with this email already exists"
}
```

#### 500 Internal Server Error
```json
{
  "error": "InternalServerError",
  "message": "An error occurred while processing your request. Please try again later."
}
```

## 5. Data Flow

### High-Level Flow
```
Client Request
    ↓
API Controller (AuthController.Register)
    ↓
Model Validation (Data Annotations)
    ↓
Application Service (IAuthService.RegisterAsync)
    ↓
Business Logic Layer
    ├─→ Check Email Uniqueness (IUserRepository)
    ├─→ Hash Password (IPasswordHashingService)
    ├─→ Get Default Role (IRoleRepository)
    ├─→ Create User Entity
    ├─→ Assign User Role
    ├─→ Save to Database (IUserRepository)
    └─→ Log to AuditLog (IAuditLogRepository)
    ↓
Map Entity to DTO
    ↓
Return Response (201 Created)
```

### Detailed Step-by-Step Flow

1. **Request Reception**
   - Client sends POST request to `/api/auth/register`
   - ASP.NET Core model binding deserializes JSON to `RegisterRequest` DTO
   - Data annotations validation executes automatically

2. **Controller Layer** (`AuthController.Register`)
   - Validates model state (if invalid, return 400 Bad Request)
   - Calls `IAuthService.RegisterAsync(request)`
   - Catches exceptions and returns appropriate error responses

3. **Application Service Layer** (`AuthService.RegisterAsync`)
   
   **Step 3.1: Email Uniqueness Check**
   - Call `IUserRepository.EmailExistsAsync(request.Email)`
   - If email exists, throw `EmailAlreadyExistsException` (409 Conflict)
   
   **Step 3.2: Password Hashing**
   - Call `IPasswordHashingService.HashPassword(request.Password)`
   - Service generates random salt (128 bytes)
   - Service hashes password with salt using PBKDF2 (256 bytes)
   - Returns tuple: `(Hash, Salt)`
   
   **Step 3.3: Get Default Role**
   - Call `IRoleRepository.GetByNameAsync("User")`
   - If role not found, throw `InvalidOperationException`
   
   **Step 3.4: Create User Entity**
   - Instantiate new `User` object:
     ```csharp
     var user = new User
     {
         Id = Guid.NewGuid(),
         Email = request.Email,
         PasswordHash = hashResult.Hash,
         PasswordSalt = hashResult.Salt,
         CreatedAt = DateTime.UtcNow
     };
     ```
   
   **Step 3.5: Assign User Role**
   - Create `UserRole` entity linking user to default role:
     ```csharp
     var userRole = new UserRole
     {
         UserId = user.Id,
         RoleId = defaultRole.Id
     };
     user.UserRoles.Add(userRole);
     ```
   
   **Step 3.6: Save to Database**
   - Call `IUserRepository.AddAsync(user)`
   - Entity Framework Core handles cascade insert of `UserRole`
   - Database transaction ensures atomicity
   
   **Step 3.7: Audit Logging**
   - Create `AuditLog` entry:
     ```csharp
     var auditLog = new AuditLog
     {
         UserId = user.Id,
         EventType = "UserRegistered",
         EventTimestamp = DateTime.UtcNow,
         Details = JsonSerializer.Serialize(new { Email = user.Email })
     };
     ```
   - Call `IAuditLogRepository.AddAsync(auditLog)`

4. **Response Mapping**
   - Map `User` entity to `RegisterResponse` DTO:
     ```csharp
     var response = new RegisterResponse
     {
         Id = user.Id,
         Email = user.Email,
         CreatedAt = user.CreatedAt
     };
     ```

5. **Controller Response**
   - Return `CreatedAtAction` with status 201
   - Set `Location` header to `/api/users/{userId}`
   - Serialize `RegisterResponse` to JSON

### Database Interactions

**Tables Modified:**
1. `Users` - INSERT new user record
2. `UserRoles` - INSERT new user-role mapping
3. `AuditLogs` - INSERT registration event

**Transaction Scope:**
- All database operations should be wrapped in a transaction
- If any operation fails, rollback entire transaction
- Entity Framework Core `SaveChangesAsync()` provides transaction support

### External Service Interactions
None for MVP (future: email verification service)

## 6. Security Considerations

### Authentication & Authorization
- **No authentication required** - This is a public endpoint
- **No authorization** - Anyone can register

### Input Validation

#### Email Validation
- **Format validation**: Use `[EmailAddress]` data annotation
- **Length validation**: Max 255 characters (database constraint)
- **Uniqueness validation**: Check database before insert
- **Case sensitivity**: Store and compare emails in lowercase to prevent duplicates (e.g., User@example.com vs user@example.com)
- **Sanitization**: Trim whitespace, validate format

**Implementation:**
```csharp
email = email.Trim().ToLowerInvariant();
```

#### Password Validation
- **Minimum length**: 8 characters
- **Complexity requirements**:
  - At least one uppercase letter (A-Z)
  - At least one lowercase letter (a-z)
  - At least one digit (0-9)
  - At least one special character (@$!%*?&)
- **Regex pattern**: `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$`
- **No common passwords**: Optional for MVP, consider adding password blacklist

### Password Storage Security

#### Hashing Algorithm: PBKDF2
- Use .NET's `Rfc2898DeriveBytes` for PBKDF2 implementation
- **Algorithm**: PBKDF2-HMAC-SHA256
- **Iterations**: 100,000 (recommended by OWASP as of 2024)
- **Salt size**: 128 bytes (as per db-plan.md)
- **Hash size**: 256 bytes (as per db-plan.md)

**Implementation:**
```csharp
public (byte[] Hash, byte[] Salt) HashPassword(string password)
{
    // Generate cryptographically strong random salt
    byte[] salt = new byte[128];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }
    
    // Generate hash using PBKDF2
    using (var pbkdf2 = new Rfc2898DeriveBytes(
        password, 
        salt, 
        iterations: 100000, 
        HashAlgorithmName.SHA256))
    {
        byte[] hash = pbkdf2.GetBytes(256);
        return (hash, salt);
    }
}
```

### Rate Limiting
**Threat**: Brute force registration, mass account creation

**Mitigation:**
- Implement rate limiting middleware
- Limit to 5 registration attempts per IP address per hour
- Consider implementing CAPTCHA for repeated attempts (future enhancement)

**Implementation Options:**
- ASP.NET Core rate limiting middleware (built-in .NET 7+)
- Third-party libraries: AspNetCoreRateLimit

### Email Enumeration
**Threat**: Attackers can discover existing email addresses by attempting registration

**Current Approach**: 
- Return 409 Conflict when email exists (as per API specification)
- This reveals email existence but is required by spec

**Alternative for Future**: 
- Return generic success message
- Send "account already exists" notification to email address
- Prevents enumeration but adds complexity

### SQL Injection Prevention
**Mitigation:**
- Entity Framework Core uses parameterized queries by default
- Never concatenate user input into SQL queries
- All queries through EF Core or stored procedures only

### HTTPS Enforcement
**Requirement**: All API communication must use HTTPS only

**Implementation:**
```csharp
// In Program.cs
app.UseHsts();
app.UseHttpsRedirection();
```

### Data Validation Summary
| Input | Validation | Security Check |
|-------|------------|----------------|
| Email | Format, Length, Uniqueness | Lowercase conversion, SQL injection protection |
| Password | Length, Complexity | Secure hashing with salt, no plaintext storage |

## 7. Error Handling

### Exception Handling Strategy

All exceptions should be caught at the controller level and mapped to appropriate HTTP responses with consistent error format.

### Custom Exceptions

#### EmailAlreadyExistsException
**Namespace:** `Trivare.Application.Exceptions`

**Status Code:** `409 Conflict`

**Usage:** Thrown when attempting to register with an existing email

**Implementation:**
```csharp
public class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException(string email) 
        : base($"An account with email '{email}' already exists") 
    { }
}
```

#### WeakPasswordException
**Namespace:** `Trivare.Application.Exceptions`

**Status Code:** `400 Bad Request`

**Usage:** Thrown when password doesn't meet requirements

**Implementation:**
```csharp
public class WeakPasswordException : Exception
{
    public WeakPasswordException(string message) : base(message) { }
}
```

### Error Scenarios & Handling

#### 1. Invalid Email Format
**Trigger:** Email fails `[EmailAddress]` validation

**HTTP Status:** `400 Bad Request`

**Response:**
```json
{
  "error": "ValidationError",
  "message": "Invalid email format",
  "errors": {
    "Email": ["Invalid email format"]
  }
}
```

**Handling:** Automatic via model validation

---

#### 2. Email Too Long
**Trigger:** Email exceeds 255 characters

**HTTP Status:** `400 Bad Request`

**Response:**
```json
{
  "error": "ValidationError",
  "message": "Email exceeds maximum length",
  "errors": {
    "Email": ["Email must not exceed 255 characters"]
  }
}
```

**Handling:** Automatic via `[MaxLength(255)]` attribute

---

#### 3. Weak Password
**Trigger:** Password fails complexity requirements

**HTTP Status:** `400 Bad Request`

**Response:**
```json
{
  "error": "ValidationError",
  "message": "Password does not meet security requirements",
  "errors": {
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain uppercase, lowercase, number, and special character"
    ]
  }
}
```

**Handling:** Automatic via model validation

---

#### 4. Missing Required Fields
**Trigger:** Email or Password is null/empty

**HTTP Status:** `400 Bad Request`

**Response:**
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

**Handling:** Automatic via `[Required]` attribute

---

#### 5. Email Already Exists (Duplicate Registration)
**Trigger:** Email uniqueness check fails

**HTTP Status:** `409 Conflict`

**Response:**
```json
{
  "error": "EmailAlreadyExists",
  "message": "An account with this email already exists"
}
```

**Handling:**
```csharp
try
{
    return await _authService.RegisterAsync(request);
}
catch (EmailAlreadyExistsException ex)
{
    return Conflict(new ErrorResponse
    {
        Error = "EmailAlreadyExists",
        Message = ex.Message
    });
}
```

**AuditLog Entry:**
```csharp
{
    UserId = null,
    EventType = "RegistrationFailed",
    EventTimestamp = DateTime.UtcNow,
    Details = JsonSerializer.Serialize(new { 
        Email = request.Email, 
        Reason = "EmailAlreadyExists" 
    })
}
```

---

#### 6. Default Role Not Found
**Trigger:** "User" role doesn't exist in database

**HTTP Status:** `500 Internal Server Error`

**Response:**
```json
{
  "error": "InternalServerError",
  "message": "An error occurred while processing your request. Please try again later."
}
```

**Handling:**
```csharp
catch (InvalidOperationException ex) when (ex.Message.Contains("Role"))
{
    _logger.LogCritical(ex, "Default 'User' role not found in database");
    return StatusCode(500, new ErrorResponse
    {
        Error = "InternalServerError",
        Message = "An error occurred while processing your request"
    });
}
```

**Note:** This should never happen in production. Database seeding should ensure "User" role exists.

---

#### 7. Database Connection Error
**Trigger:** Database unavailable or connection timeout

**HTTP Status:** `500 Internal Server Error`

**Response:**
```json
{
  "error": "InternalServerError",
  "message": "An error occurred while processing your request. Please try again later."
}
```

**Handling:**
```csharp
catch (DbUpdateException ex)
{
    _logger.LogError(ex, "Database error during user registration");
    return StatusCode(500, new ErrorResponse
    {
        Error = "InternalServerError",
        Message = "An error occurred while processing your request"
    });
}
```

**AuditLog Entry:**
```csharp
{
    UserId = null,
    EventType = "RegistrationFailed",
    EventTimestamp = DateTime.UtcNow,
    Details = JsonSerializer.Serialize(new { 
        Email = request.Email, 
        Reason = "DatabaseError",
        ErrorMessage = ex.Message 
    })
}
```

---

#### 8. Unexpected Exception
**Trigger:** Any unhandled exception

**HTTP Status:** `500 Internal Server Error`

**Response:**
```json
{
  "error": "InternalServerError",
  "message": "An error occurred while processing your request. Please try again later."
}
```

**Handling:**
```csharp
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error during user registration");
    return StatusCode(500, new ErrorResponse
    {
        Error = "InternalServerError",
        Message = "An error occurred while processing your request"
    });
}
```

### Logging Strategy

**Successful Registration:**
```csharp
_logger.LogInformation("User registered successfully: {Email}, UserId: {UserId}", 
    user.Email, user.Id);
```

**Failed Registration (Duplicate Email):**
```csharp
_logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
```

**Failed Registration (Validation):**
```csharp
_logger.LogWarning("Registration failed - validation error for email: {Email}", request.Email);
```

**Critical Errors:**
```csharp
_logger.LogError(ex, "Critical error during registration for email: {Email}", request.Email);
```

## 8. Performance Considerations

### Database Optimization

#### Indexes
**Email Index** - Already configured in `ApplicationDbContext`:
```csharp
entity.HasIndex(e => e.Email).IsUnique();
```
- **Type**: Unique, non-clustered
- **Purpose**: Fast email lookup for uniqueness validation
- **Query**: `SELECT * FROM Users WHERE Email = @email`
- **Performance**: O(log n) lookup time

#### Query Optimization
- Use `AsNoTracking()` for email existence check (read-only query):
  ```csharp
  await _context.Users.AsNoTracking().AnyAsync(u => u.Email == email);
  ```
- Single database round-trip for user creation (cascade insert of UserRole)

### Password Hashing Performance

**PBKDF2 with 100,000 iterations:**
- Expected time: 100-200ms per hash (acceptable for registration)
- CPU-intensive operation
- Trade-off between security and performance

**Considerations:**
- Hashing is intentionally slow to prevent brute force attacks
- Registration is infrequent operation (compared to login)
- Asynchronous execution prevents blocking

### Potential Bottlenecks

#### 1. Database Email Uniqueness Check
**Issue:** Sequential database query before insert

**Mitigation:**
- Rely on unique index constraint
- Catch `DbUpdateException` for duplicate key violation
- Remove explicit existence check (trade-off: exception handling vs extra query)

**Alternative Approach:**
```csharp
try
{
    await _userRepository.AddAsync(user);
}
catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
{
    throw new EmailAlreadyExistsException(user.Email);
}
```

#### 2. Password Hashing CPU Usage
**Issue:** CPU-intensive operation

**Mitigation:**
- Already using asynchronous pattern
- Consider increasing iterations to 310,000 (OWASP 2024 recommendation)
- For high-volume scenarios: implement queue-based registration

#### 3. AuditLog Write Performance
**Issue:** Additional database write

**Mitigation:**
- Make audit logging asynchronous (fire-and-forget)
- Use background service for audit log writes
- Batch audit log inserts for high-volume scenarios

**Future Optimization:**
```csharp
_backgroundJobQueue.QueueBackgroundWorkItem(async token =>
{
    await _auditLogRepository.AddAsync(auditLog, token);
});
```

### Scalability Considerations

**Horizontal Scaling:**
- Stateless endpoint design
- No session dependencies
- Can scale across multiple API instances

**Database Scaling:**
- Azure SQL supports read replicas
- Consider read-write split for audit logs
- Connection pooling configured in connection string

### Caching Strategy
**Not applicable for registration endpoint:**
- Each registration is unique
- Data is write-heavy, not read-heavy
- No opportunity for caching

## 9. Implementation Steps

### Phase 1: Infrastructure Layer Setup

#### Step 1.1: Implement PasswordHashingService
**File:** `Server/Infrastructure/Services/PasswordHashingService.cs`

**Implementation:**
```csharp
using System.Security.Cryptography;

namespace Trivare.Infrastructure.Services;

public class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 128; // bytes
    private const int HashSize = 256; // bytes
    private const int Iterations = 100000;

    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        // Generate cryptographically strong random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        
        // Generate hash using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(
            password, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return (hash, salt);
        }
    }

    public bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(
            password, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256))
        {
            byte[] computedHash = pbkdf2.GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(computedHash, hash);
        }
    }
}
```

**Tests to Write:**
- Verify salt is randomly generated (different each time)
- Verify hash length is 256 bytes
- Verify salt length is 128 bytes
- Verify same password with different salts produces different hashes
- Verify VerifyPassword returns true for correct password
- Verify VerifyPassword returns false for incorrect password

---

#### Step 1.2: Implement UserRepository
**File:** `Server/Infrastructure/Repositories/UserRepository.cs`

**Implementation:**
```csharp
using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);
        return user;
    }
}
```

**Tests to Write:**
- Verify AddAsync creates user in database
- Verify EmailExistsAsync returns true for existing email
- Verify EmailExistsAsync returns false for non-existing email
- Verify GetByEmailAsync returns user with roles
- Verify GetByEmailAsync returns null for non-existing email

---

#### Step 1.3: Implement RoleRepository
**File:** `Server/Infrastructure/Repositories/RoleRepository.cs`

**Implementation:**
```csharp
using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }
}
```

**Tests to Write:**
- Verify GetByNameAsync returns correct role
- Verify GetByNameAsync returns null for non-existing role
- Verify case-sensitive role name matching

---

#### Step 1.4: Implement AuditLogRepository
**File:** `Server/Infrastructure/Repositories/AuditLogRepository.cs`

**Implementation:**
```csharp
using Trivare.Domain.Entities;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly ApplicationDbContext _context;

    public AuditLogRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

**Tests to Write:**
- Verify AddAsync creates audit log entry
- Verify timestamp is set correctly
- Verify nullable UserId handling

---

### Phase 2: Application Layer Setup

#### Step 2.1: Create Repository Interfaces
**File:** `Server/Application/Repositories/IUserRepository.cs`

```csharp
using Trivare.Domain.Entities;

namespace Trivare.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}
```

**File:** `Server/Application/Repositories/IRoleRepository.cs`

```csharp
using Trivare.Domain.Entities;

namespace Trivare.Application.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
```

**File:** `Server/Application/Repositories/IAuditLogRepository.cs`

```csharp
using Trivare.Domain.Entities;

namespace Trivare.Application.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
```

---

#### Step 2.2: Create Service Interfaces
**File:** `Server/Application/Services/IPasswordHashingService.cs`

```csharp
namespace Trivare.Application.Services;

public interface IPasswordHashingService
{
    (byte[] Hash, byte[] Salt) HashPassword(string password);
    bool VerifyPassword(string password, byte[] hash, byte[] salt);
}
```

**File:** `Server/Application/Services/IAuthService.cs`

```csharp
using Trivare.Application.DTOs.Auth;

namespace Trivare.Application.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
```

---

#### Step 2.3: Create Custom Exceptions
**File:** `Server/Application/Exceptions/EmailAlreadyExistsException.cs`

```csharp
namespace Trivare.Application.Exceptions;

public class EmailAlreadyExistsException : Exception
{
    public string Email { get; }

    public EmailAlreadyExistsException(string email) 
        : base($"An account with email '{email}' already exists")
    {
        Email = email;
    }
}
```

---

#### Step 2.4: Update RegisterRequest with Validation
**File:** `Server/Application/DTOs/Auth/RegisterRequest.cs`

**Add these attributes:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

public record RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
    public required string Password { get; init; }
}
```

---

#### Step 2.5: Create ErrorResponse DTO
**File:** `Server/Application/DTOs/Common/ErrorResponse.cs`

```csharp
namespace Trivare.Application.DTOs.Common;

public record ErrorResponse
{
    public required string Error { get; init; }
    public required string Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
}
```

---

#### Step 2.6: Implement AuthService
**File:** `Server/Application/Services/AuthService.cs`

```csharp
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.Exceptions;
using Trivare.Application.Repositories;
using Trivare.Domain.Entities;

namespace Trivare.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHashingService passwordHashingService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHashingService = passwordHashingService;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Normalize email
        var email = request.Email.Trim().ToLowerInvariant();

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Registration failed - email already exists: {Email}", email);
            
            // Log failed registration attempt
            await LogAuditAsync(null, "RegistrationFailed", new
            {
                Email = email,
                Reason = "EmailAlreadyExists"
            }, cancellationToken);

            throw new EmailAlreadyExistsException(email);
        }

        // Get default "User" role
        var defaultRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
        if (defaultRole == null)
        {
            _logger.LogCritical("Default 'User' role not found in database");
            throw new InvalidOperationException("Default 'User' role not found");
        }

        // Hash password
        var (hash, salt) = _passwordHashingService.HashPassword(request.Password);

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        // Assign default role
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id,
            User = user,
            Role = defaultRole
        };
        user.UserRoles.Add(userRole);

        // Save to database
        try
        {
            await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("User registered successfully: {Email}, UserId: {UserId}", 
                user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error during user registration for email: {Email}", email);
            
            // Log failed registration attempt
            await LogAuditAsync(null, "RegistrationFailed", new
            {
                Email = email,
                Reason = "DatabaseError",
                ErrorMessage = ex.Message
            }, cancellationToken);

            throw;
        }

        // Log successful registration
        await LogAuditAsync(user.Id, "UserRegistered", new
        {
            Email = user.Email
        }, cancellationToken);

        // Map to response DTO
        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task LogAuditAsync(
        Guid? userId, 
        string eventType, 
        object details, 
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                EventType = eventType,
                EventTimestamp = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(details)
            };

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for event: {EventType}", eventType);
            // Don't throw - audit logging failure shouldn't break registration
        }
    }
}
```

**Tests to Write:**
- Verify successful registration flow
- Verify email normalization (lowercase, trim)
- Verify EmailAlreadyExistsException thrown for duplicate email
- Verify password hashing is called
- Verify default role assignment
- Verify audit logging for success
- Verify audit logging for failure
- Verify InvalidOperationException when role not found

---

### Phase 3: API Layer Setup

#### Step 3.1: Create AuthController
**File:** `Server/Api/Controllers/AuthController.cs`

```csharp
using Microsoft.AspNetCore.Mvc;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;
using Trivare.Application.Exceptions;
using Trivare.Application.Services;

namespace Trivare.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user information</returns>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
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
            var response = await _authService.RegisterAsync(request, cancellationToken);
            
            return CreatedAtAction(
                actionName: nameof(Register),
                routeValues: new { id = response.Id },
                value: response
            );
        }
        catch (EmailAlreadyExistsException ex)
        {
            _logger.LogWarning(ex, "Registration failed - email already exists");
            return Conflict(new ErrorResponse
            {
                Error = "EmailAlreadyExists",
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Role"))
        {
            _logger.LogCritical(ex, "Default 'User' role not found in database");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your request. Please try again later."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your request. Please try again later."
            });
        }
    }
}
```

**Tests to Write:**
- Verify 201 Created response for valid registration
- Verify 400 Bad Request for invalid email
- Verify 400 Bad Request for weak password
- Verify 400 Bad Request for missing fields
- Verify 409 Conflict for duplicate email
- Verify 500 Internal Server Error for database errors
- Verify Location header is set correctly
- Verify ModelState validation errors format

---

### Phase 4: Dependency Injection Configuration

#### Step 4.1: Register Services in Program.cs
**File:** `Server/Api/Program.cs`

**Add these registrations:**
```csharp
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordHashingService, PasswordHashingService>();
```

**Note:** `IPasswordHashingService` is registered as Singleton because it's stateless.

---

### Phase 5: Database Setup

#### Step 5.1: Seed Default Roles
**File:** `Server/Infrastructure/Data/DbInitializer.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;

namespace Trivare.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.MigrateAsync();

        // Check if roles exist
        if (!await context.Roles.AnyAsync())
        {
            var roles = new[]
            {
                new Role { Id = Guid.NewGuid(), Name = "User" },
                new Role { Id = Guid.NewGuid(), Name = "Admin" }
            };

            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
        }
    }
}
```

**Call from Program.cs:**
```csharp
// After app.Build()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DbInitializer.SeedAsync(context);
}
```

---

### Phase 6: Testing

#### Step 6.1: Unit Tests for PasswordHashingService
**File:** `Server/Tests/Application.Tests/Services/PasswordHashingServiceTests.cs`

**Test cases:**
- `HashPassword_GeneratesUniqueHashes_ForSamePassword`
- `HashPassword_ReturnsCorrectHashSize`
- `HashPassword_ReturnsCorrectSaltSize`
- `VerifyPassword_ReturnsTrue_ForCorrectPassword`
- `VerifyPassword_ReturnsFalse_ForIncorrectPassword`
- `VerifyPassword_ReturnsFalse_ForDifferentSalt`

---

#### Step 6.2: Unit Tests for AuthService
**File:** `Server/Tests/Application.Tests/Services/AuthServiceTests.cs`

**Test cases:**
- `RegisterAsync_SuccessfullyCreatesUser_WithValidInput`
- `RegisterAsync_NormalizesEmail_ToLowerCase`
- `RegisterAsync_ThrowsEmailAlreadyExistsException_WhenEmailExists`
- `RegisterAsync_ThrowsInvalidOperationException_WhenRoleNotFound`
- `RegisterAsync_AssignsDefaultUserRole`
- `RegisterAsync_HashesPasswordCorrectly`
- `RegisterAsync_CreatesAuditLogEntry_OnSuccess`
- `RegisterAsync_CreatesAuditLogEntry_OnFailure`

---

#### Step 6.3: Integration Tests for AuthController
**File:** `Server/Tests/Api.Tests/Controllers/AuthControllerTests.cs`

**Test cases:**
- `Register_Returns201Created_WithValidRequest`
- `Register_Returns400BadRequest_WithInvalidEmail`
- `Register_Returns400BadRequest_WithWeakPassword`
- `Register_Returns400BadRequest_WithMissingFields`
- `Register_Returns409Conflict_WithDuplicateEmail`
- `Register_SetsLocationHeader_OnSuccess`
- `Register_ReturnsCorrectErrorFormat_OnValidationFailure`

---

### Phase 7: API Documentation

#### Step 7.1: Configure Swagger
**File:** `Server/Api/Program.cs`

**Add Swagger configuration:**
```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Trivare API",
        Version = "v1",
        Description = "Trip planning API"
    });

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});
```

**Enable XML documentation in Api.csproj:**
```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

---

### Phase 8: Manual Testing

#### Step 8.1: Test with Valid Input
**Request:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "SecurePass123!"
  }'
```

**Expected Response:** 201 Created
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "test@example.com",
  "createdAt": "2025-10-12T10:30:00Z"
}
```

---

#### Step 8.2: Test with Duplicate Email
**Request:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "AnotherPass123!"
  }'
```

**Expected Response:** 409 Conflict
```json
{
  "error": "EmailAlreadyExists",
  "message": "An account with email 'test@example.com' already exists"
}
```

---

#### Step 8.3: Test with Invalid Email
**Request:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalid-email",
    "password": "SecurePass123!"
  }'
```

**Expected Response:** 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "One or more validation errors occurred",
  "errors": {
    "Email": ["Invalid email format"]
  }
}
```

---

#### Step 8.4: Test with Weak Password
**Request:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test2@example.com",
    "password": "weak"
  }'
```

**Expected Response:** 400 Bad Request
```json
{
  "error": "ValidationError",
  "message": "One or more validation errors occurred",
  "errors": {
    "Password": [
      "Password must be at least 8 characters",
      "Password must contain uppercase, lowercase, number, and special character"
    ]
  }
}
```

---

### Phase 9: Database Verification

#### Step 9.1: Verify User Creation
**Query:**
```sql
SELECT Id, Email, CreatedAt 
FROM Users 
WHERE Email = 'test@example.com';
```

**Expected:** One row with matching email

---

#### Step 9.2: Verify Password Hashing
**Query:**
```sql
SELECT 
    Id, 
    Email, 
    LEN(PasswordHash) AS HashLength,
    LEN(PasswordSalt) AS SaltLength
FROM Users 
WHERE Email = 'test@example.com';
```

**Expected:** 
- HashLength: 256
- SaltLength: 128

---

#### Step 9.3: Verify Role Assignment
**Query:**
```sql
SELECT u.Email, r.Name AS Role
FROM Users u
JOIN UserRoles ur ON u.Id = ur.UserId
JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'test@example.com';
```

**Expected:** One row with Role = "User"

---

#### Step 9.4: Verify Audit Log
**Query:**
```sql
SELECT EventType, EventTimestamp, Details
FROM AuditLogs
WHERE UserId = (SELECT Id FROM Users WHERE Email = 'test@example.com')
ORDER BY EventTimestamp DESC;
```

**Expected:** One or more rows with EventType = "UserRegistered"

---

## 10. Deployment Checklist

- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] Swagger documentation generated correctly
- [ ] Database migration created and applied
- [ ] Default roles seeded in database
- [ ] HTTPS enforced in production
- [ ] Rate limiting configured
- [ ] Logging configured for production
- [ ] Error handling tested for all scenarios
- [ ] Security review completed
- [ ] Performance testing completed (100-200ms response time target)
- [ ] API documentation updated
- [ ] Postman collection created for manual testing

---

## 11. Future Enhancements

1. **Email Verification**
   - Send verification email with token
   - User must verify email before login
   - Add `EmailVerified` flag to User entity

2. **CAPTCHA Integration**
   - Prevent automated registration
   - Add Google reCAPTCHA or similar

3. **Password Strength Meter**
   - Client-side password strength indicator
   - Real-time feedback for users

4. **Password Blacklist**
   - Check against common password lists
   - Prevent use of compromised passwords

5. **Rate Limiting Enhancement**
   - Implement distributed rate limiting (Redis)
   - Per-IP and per-email rate limits

6. **OAuth Integration**
   - Google/Facebook/Microsoft sign-up
   - Simplify registration process

7. **Audit Log Optimization**
   - Asynchronous audit logging
   - Background queue for high-volume scenarios

8. **Monitoring & Alerts**
   - Track registration success/failure rates
   - Alert on unusual registration patterns
   - Monitor response times

---

## Summary

This implementation plan provides a comprehensive guide for developing the User Registration endpoint following Clean Architecture principles, security best practices, and the Trivare project's tech stack. The plan includes detailed steps for implementation, testing, and deployment, ensuring a robust and secure registration system.

**Key Security Features:**
- PBKDF2 password hashing with 100,000 iterations
- 128-byte random salt per password
- Input validation at multiple layers
- Audit logging for security monitoring
- Protection against common attacks (SQL injection, brute force)

**Key Architecture Features:**
- Clean Architecture with clear layer separation
- Repository pattern for data access
- Service layer for business logic
- Comprehensive error handling
- Extensive logging and monitoring

**Key Performance Features:**
- Indexed email lookups
- Asynchronous operations throughout
- Connection pooling
- Optimized database queries with AsNoTracking()
