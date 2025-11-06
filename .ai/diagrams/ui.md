# Authentication UI Architecture Diagram

````mermaid
flowchart TD
    %% User Entry Points
    A[User] --> B{Authentication State}
    B -->|Unauthenticated| C[Public Routes]
    B -->|Authenticated| D[Protected Routes]

    %% Public Routes Subgraph
    subgraph "Public Authentication Routes"
        C --> E[login.astro]
        C --> F[register.astro]
        C --> G[forgot-password.astro]
        C --> H[reset-password.astro]
    end

    %% Protected Routes Subgraph
    subgraph "Protected Application Routes"
        D --> I[profile.astro]
        D --> J[trips/create.astro]
        D --> K[trips/[tripId].astro]
        D --> L[index.astro - Dashboard]
    end

    %% Route Protection Components
    subgraph "Route Protection Layer"
        M[PublicRoute.tsx] --> C
        N[ProtectedRoute.tsx] --> D
    end

    %% Page Components Subgraph
    subgraph "Page Components"
        O[LoginPage.tsx] --> E
        P[RegisterPage.tsx] --> F
        Q[HomePage.tsx] --> L
    end

    %% View Components Subgraph
    subgraph "View Components"
        R[LoginView.tsx] --> O
        S[RegisterView.tsx] --> P
    end

    %% Form Components Subgraph
    subgraph "Form Components"
        T[LoginForm.tsx] --> R
        U[RegisterForm.tsx] --> S
        V[ForgotPasswordForm.tsx] --> G
        W[ResetPasswordForm.tsx] --> H
    end

    %% Navigation Components Subgraph
    subgraph "Navigation Components"
        X[Nav.tsx] --> Y[ProfileDropdown.tsx]
        X --> Z[MobileNav.tsx]
    end

    %% State Management Subgraph
    subgraph "Authentication State Management"
        AA[useAuth.ts]
        AA --> BB[useCurrentUser Hook]
        AA --> CC[useIsAuthenticated Hook]
        AA --> DD[notifyAuthChange Function]
    end

    %% Client Storage Subgraph
    subgraph "Client-Side Storage"
        EE[localStorage]
        FF[sessionStorage]
        EE -->|Access Token| GG[Persistent Auth]
        EE -->|Refresh Token| HH[Token Refresh]
        EE -->|User Data| II[User Info]
        FF -->|Return Path| JJ[Post-Login Redirect]
    end

    %% Validation Layer Subgraph
    subgraph "Form Validation Layer"
        KK[Zod Schemas]
        KK --> LL[LoginViewModel]
        KK --> MM[RegisterViewModel]
        KK --> NN[ResetPasswordViewModel]
        OO[React Hook Form] --> KK
    end

    %% API Integration Subgraph
    subgraph "Backend API Integration"
        PP[AuthController.cs]
        QQ[AuthService.cs]
        RR[IAuthService.cs]
        PP --> QQ
        QQ --> RR
    end

    %% Data Flow Connections
    T -.->|Form Submission| SS[API Calls]
    U -.->|Form Submission| SS
    V -.->|Form Submission| SS
    W -.->|Form Submission| SS

    SS --> PP
    PP -->|JWT Tokens| EE
    PP -->|User Data| II

    EE --> AA
    AA --> BB
    AA --> CC
    AA -->|State Updates| X

    KK -.->|Validation| T
    KK -.->|Validation| U
    KK -.->|Validation| W

    B --> AA
    M --> B
    N --> B

    %% Application Dependencies
    X -.->|Conditional Rendering| B
    Y -.->|Logout Action| EE
    Y -.->|User Menu| II

    %% Styling Classes
    classDef authPages fill:#e1f5fe,stroke:#01579b,stroke-width:2px
    classDef routeGuards fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef components fill:#f3e5f5,stroke:#4a148c,stroke-width:2px
    classDef state fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef storage fill:#fff8e1,stroke:#f57f17,stroke-width:2px
    classDef validation fill:#fce4ec,stroke:#880e4f,stroke-width:2px
    classDef api fill:#e0f2f1,stroke:#004d40,stroke-width:2px

    class E,F,G,H authPages
    class M,N routeGuards
    class O,P,Q,R,S,T,U,V,W,X,Y,Z components
    class AA,BB,CC,DD state
    class EE,FF,GG,HH,II,JJ storage
    class KK,LL,MM,NN,OO validation
    class PP,QQ,RR,SS api
```</contents>
</xai:function_call">Here is the comprehensive Mermaid diagram for the authentication UI architecture. The diagram shows:

**Key Architecture Elements:**
- **Public Routes**: Login, registration, and password recovery pages
- **Protected Routes**: Profile and application pages requiring authentication
- **Route Protection**: Guards that control access based on authentication state
- **Form Components**: Validation and submission handling with Zod schemas
- **State Management**: Reactive hooks that sync with localStorage
- **Navigation**: Conditional rendering based on authentication status
- **API Integration**: Backend authentication services and controllers

**Data Flow Highlights:**
- User authentication state determines route access
- Form validation occurs client-side before API submission
- Successful authentication stores tokens in localStorage
- State management hooks provide reactive updates across components
- Navigation components update based on authentication changes

**Component Relationships:**
- Route guards protect access to pages
- View components wrap forms with navigation
- Form components integrate with validation schemas
- State management connects to storage and navigation
- All components depend on authentication state for proper functionality

The diagram uses color-coded subgraphs to clearly separate concerns and shows the complete authentication flow from user entry through backend integration.</content>
</xai:function_call">The diagram has been created successfully. Let me now review it for syntax errors and completeness to ensure it meets all the requirements.
````
