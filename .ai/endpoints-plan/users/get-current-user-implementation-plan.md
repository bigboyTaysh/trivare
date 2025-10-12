# API Endpoint Implementation Plan: Get Current User

## 1. Endpoint Overview
This endpoint allows authenticated users to retrieve their own profile information, including basic details and assigned roles. It follows RESTful principles and is part of the user management resource group. The endpoint enforces JWT-based authentication and returns user data in JSON format.

## 2. Request Details
- **HTTP Method:** GET
- **URL Structure:** `/api/users/me`
- **Parameters:**
  - Required: None (authentication via JWT in Authorization header)
  - Optional: None
- **Request Body:** None
- **Authentication:** Required (JWT Bearer token)

## 3. Used Types
- **Response DTO:** `UserDto` (existing in `Server/Application/DTOs/Users/`)
  - Properties: `Id` (Guid), `Email` (string), `CreatedAt` (DateTime), `Roles` (List<string>)
- **Domain Entities:** `User`, `Role`, `UserRole` (existing in `Server/Domain/Entities/`)
- **Command Models:** None (read-only endpoint)

## 4. Response Details
- **Success Response (200 OK):**
  ```json
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "createdAt": "2025-10-01T10:30:00Z",
    "roles": ["User"]
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing JWT token
  - `500 Internal Server Error` - Server-side issues (DB connection, service failures)

## 5. Data Flow
1. Request reaches ASP.NET Core controller (`UsersController`).
2. JWT middleware validates token and extracts user ID.
3. Controller calls `IUserService.GetCurrentUserAsync(userId)` or similar.
4. Service queries `IUserRepository` with eager loading of roles via `UserRoles` and `Roles`.
5. Repository uses Entity Framework with Include() for UserRoles and Roles.
6. Data mapped to `UserDto` and returned.
7. Response serialized to JSON.

## 6. Security Considerations
- JWT token validation in middleware (handled by ASP.NET Core).
- Row-Level Security (RLS) in Azure SQL ensures users can only access their own data.
- No sensitive data exposure (password hashes not included).
- Rate limiting recommended to prevent abuse.
- Audit logging for failed access attempts.

## 7. Error Handling
- **401 Unauthorized:** Log to `AuditLog` with event type "UserProfileAccessFailed" if token invalid.
- **500 Internal Server Error:** Log exception details; return generic message to client.
- Use global exception filter for consistent error responses.
- Validate user existence (though JWT should ensure this).

## 8. Performance Considerations
- Eager loading with Include() to avoid N+1 queries.
- Index on `Users.Id` and foreign keys in `UserRoles`.
- Cache user roles if frequently accessed (consider Redis if needed).
- Keep response payload minimal.

## 9. Implementation Steps
1. Verify `UserDto` exists in `Server/Application/DTOs/Users/UserDto.cs` with required properties.
2. Add `GetCurrentUser` method to `IUserService` interface in `Server/Application/Interfaces/`.
3. Implement method in `UserService` class, injecting `IUserRepository`.
4. Update `UserRepository` to include roles in query using Entity Framework Include().
5. Create `UsersController` in `Server/Api/Controllers/` with `[Authorize]` attribute.
6. Implement `GetMe` action method: extract user ID from claims, call service, return DTO.
7. Add Swagger documentation with response examples.
8. Test endpoint with valid/invalid tokens.
9. Add audit logging for errors in service layer.
10. Update API documentation and ensure Clean Architecture compliance.