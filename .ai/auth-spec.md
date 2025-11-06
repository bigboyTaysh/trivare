# Authentication Architecture Specification - Trivare

## 1. USER INTERFACE ARCHITECTURE

### Current Frontend Structure

#### Pages Layer (Astro)

- **login.astro**: Entry point for user authentication
- **register.astro**: User account creation page
- **forgot-password.astro**: Password recovery initiation
- **reset-password.astro**: Password reset completion with token validation
- **profile.astro**: User profile management (extends authentication context)

#### Component Architecture

##### Route Protection Components

- **ProtectedRoute.tsx**: Guards authenticated routes, redirects to login if unauthorized
- **PublicRoute.tsx**: Guards public routes, optionally redirects authenticated users

##### Authentication Views

- **LoginView.tsx**: Container for login form with navigation links
- **RegisterView.tsx**: Container for registration form with navigation links

##### Form Components

- **LoginForm.tsx**: Handles email/password authentication with validation
- **RegisterForm.tsx**: Multi-step registration with password strength validation
- **ForgotPasswordForm.tsx**: Email input for password recovery
- **ResetPasswordForm.tsx**: New password input with token validation

##### Navigation Components

- **Nav.tsx**: Main navigation bar with conditional rendering based on auth state
- **ProfileDropdown.tsx**: User menu with logout functionality
- **MobileNav.tsx**: Mobile-optimized navigation

### Authentication State Management

#### React Hooks

- **useAuth.ts**: Custom hooks for authentication state management
  - `useCurrentUser()`: Provides current authenticated user data
  - `useIsAuthenticated()`: Boolean authentication status
  - `notifyAuthChange()`: Triggers auth state updates across components

#### Client-Side Storage

- **localStorage** for persistent authentication tokens
- **sessionStorage** for redirect paths after login

### Form Validation Strategy

#### Client-Side Validation

- **Zod schemas** for runtime type validation
- **React Hook Form** integration with Zod resolver
- **Real-time validation** with onBlur mode
- **Password strength indicators** with visual feedback

#### Validation Rules

- **Email**: RFC-compliant format, uniqueness validation
- **Username**: Alphanumeric with underscores/hyphens, 3-50 characters
- **Password**: Minimum 8 characters, uppercase, lowercase, number, special character
- **Password confirmation**: Must match primary password field

### User Experience Flows

#### Registration Flow

1. User navigates to `/register`
2. PublicRoute ensures unauthenticated access
3. Form validation occurs on blur and submit
4. Password strength indicator provides real-time feedback
5. Success redirects to login with confirmation message

#### Login Flow

1. User navigates to `/login`
2. PublicRoute ensures unauthenticated access
3. Form validation with server-side credential verification
4. Successful login stores tokens and redirects to dashboard
5. Failed login shows generic error to prevent enumeration

#### Password Recovery Flow

1. User requests reset via `/forgot-password`
2. Email sent with secure reset token (1-hour expiry)
3. Token validation on `/reset-password`
4. Password update with strength validation
5. Automatic redirect to login on success

#### Protected Route Access

1. Unauthenticated users redirected to `/login`
2. Return path stored in sessionStorage
3. Post-login redirect to intended destination

### Error Handling and Messaging

#### Client-Side Error Scenarios

- **Network failures**: Generic "try again later" messages
- **Validation errors**: Field-specific messages from Zod schemas
- **Authentication failures**: Generic "invalid credentials" messaging
- **Registration conflicts**: Email/username already exists feedback

#### User Feedback Mechanisms

- **Toast notifications** for success/error states
- **Form field errors** for validation failures
- **Loading states** during async operations
- **Progressive disclosure** for complex forms

## 2. BACKEND LOGIC

### API Endpoint Architecture

#### Authentication Controller (`AuthController.cs`)

RESTful endpoints following clean architecture patterns:

- **POST /api/auth/register**: User registration with validation
- **POST /api/auth/login**: JWT token generation
- **POST /api/auth/refresh**: Token renewal
- **POST /api/auth/logout**: Token invalidation
- **POST /api/auth/forgot-password**: Reset token generation
- **POST /api/auth/reset-password**: Password update with token validation

### Data Models and DTOs

#### Request/Response Contracts

- **RegisterRequest**: UserName, Email, Password
- **LoginRequest**: Email, Password
- **LoginResponse**: AccessToken, RefreshToken, ExpiresIn, User
- **RefreshTokenRequest**: RefreshToken
- **ResetPasswordRequest**: Token, NewPassword

#### Domain Entities

- **User**: Core user entity with authentication fields
- **UserRole**: User-role relationship mapping
- **AuditLog**: Authentication event logging

### Business Logic Implementation

#### Authentication Service (`AuthService.cs`)

Comprehensive service handling all auth operations:

##### Registration Logic

1. Email normalization and uniqueness validation
2. Password hashing with salt generation
3. Default role assignment ("User")
4. Audit logging for security tracking

##### Login Logic

1. Email normalization and user lookup
2. Password verification with secure comparison
3. JWT token generation (access + refresh)
4. Refresh token storage with expiry
5. Audit logging of successful authentication

##### Token Management

1. **Access tokens**: Short-lived (15-60 minutes)
2. **Refresh tokens**: Long-lived (7-30 days)
3. **Secure storage**: Database-backed refresh tokens
4. **Token rotation**: New refresh token on each use

##### Password Security

1. **Argon2/BCrypt hashing** with unique salts
2. **Password policies**: Complexity requirements
3. **Reset token security**: Cryptographically secure, URL-safe
4. **Token expiry**: 1-hour window for password resets

### Validation and Error Handling

#### Input Validation

- **Model validation** via Data Annotations
- **Business rule validation** in service layer
- **Security validation** against injection attacks

#### Error Response Strategy

- **Structured error responses** with error codes
- **Security-conscious messaging** (no enumeration)
- **Logging** for debugging and audit trails

#### Security Measures

- **Rate limiting** protection (recommended implementation)
- **Brute force prevention** via account lockout
- **Audit logging** for all authentication events
- **Token blacklisting** capability (architecture ready)

### Integration Points

#### External Services

- **Email service** for password recovery communications
- **JWT token service** for token generation/validation
- **Password hashing service** for secure password management

#### Data Access Layer

- **User repository** for user CRUD operations
- **Role repository** for authorization management
- **Audit log repository** for security event tracking

### Scalability Considerations

#### Database Performance

- **Indexed email lookups** for fast authentication
- **Efficient token validation** queries
- **Audit log partitioning** for large-scale deployments

#### Security Architecture

- **Stateless authentication** via JWT tokens
- **Refresh token rotation** preventing replay attacks
- **Secure password storage** with modern hashing algorithms

## 3. INTEGRATION AND DEPLOYMENT

### Client-Server Communication

#### API Integration

- **Fetch API** for HTTP requests
- **JSON serialization** for data exchange
- **Error handling** with appropriate user feedback

#### State Synchronization

- **Local storage** for token persistence
- **Session storage** for temporary state
- **Real-time updates** via React hooks

### Security Implementation

#### Frontend Security

- **Token storage** in secure localStorage
- **Automatic logout** on token expiry
- **Route protection** preventing unauthorized access

#### Backend Security

- **JWT validation** middleware
- **CORS configuration** for cross-origin requests
- **Request validation** preventing malformed data

### Monitoring and Logging

#### Audit Trail

- **Authentication events** logged to database
- **Security incidents** tracked for compliance
- **Performance metrics** for optimization

#### Error Tracking

- **Client-side errors** logged via console/monitoring
- **Server-side errors** with structured logging
- **User feedback** for error resolution

## 4. COMPLIANCE AND STANDARDS

### GDPR Compliance

- **Account deletion** capability (implemented)
- **Data minimization** principles
- **Audit logging** for data access tracking

### Security Standards

- **OWASP guidelines** for authentication security
- **Secure password policies** implementation
- **Token-based authentication** best practices

## 5. FUTURE ENHANCEMENTS

### Potential Improvements

- **Multi-factor authentication** (MFA) support
- **Social login** integration
- **Account lockout** mechanisms
- **Security event monitoring** dashboard
- **Token blacklisting** for compromised accounts

### Performance Optimizations

- **Token caching** strategies
- **Database query optimization**
- **CDN integration** for static assets
- **Rate limiting** implementation

This architecture provides a solid foundation for secure, scalable user authentication while maintaining excellent user experience and compliance with modern security standards.
