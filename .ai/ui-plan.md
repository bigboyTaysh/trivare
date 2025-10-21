# UI Architecture for Trivare

## 1. UI Structure Overview

The UI architecture for Trivare is designed as a responsive, single-page application (SPA) experience built with Astro and React. It prioritizes a clean, intuitive, and mobile-first user journey. The structure is centered around a main dashboard that serves as the user's home base, providing quick access to their trips. From there, users can drill down into detailed trip planning views or manage their account settings.

The architecture leverages a component-based approach using `shadcn/ui` for a consistent and accessible component library. State management is handled locally with React hooks (`useState`, `useEffect`, `useOptimistic`), and data is fetched via a centralized API service that includes automatic token refresh logic. Navigation is managed by Astro's file-based routing, protected by a client-side authentication guard.

## 2. View List

### 2.1. Authentication Views

-   **View Name:** Login
-   **View Path:** `/login`
-   **Main Purpose:** To allow registered users to securely access their accounts.
-   **Key Information to Display:** Email and password input fields, a link to the "Forgot Password" view, and a link to the "Register" view.
-   **Key View Components:** `Card`, `Input`, `Button`, `Form`, `Toast` (for error notifications).
-   **UX, Accessibility, and Security:** The form will provide clear inline validation and error messages. Inputs will have associated labels for screen readers. The "Log In" button will show a loading state to prevent multiple submissions.

-   **View Name:** Register
-   **View Path:** `/register`
-   **Main Purpose:** To allow new users to create an account.
-   **Key Information to Display:** Username, email, and password input fields, and a link to the "Login" view.
-   **Key View Components:** `Card`, `Input`, `Button`, `Form`, `Toast`.
-   **UX, Accessibility, and Security:** Password strength requirements will be clearly communicated. The system will provide immediate feedback on email uniqueness. All inputs will be properly labeled.

-   **View Name:** Forgot Password
-   **View Path:** `/forgot-password`
-   **Main Purpose:** To initiate the password reset process.
-   **Key Information to Display:** An email input field and instructions for the user.
-   **Key View Components:** `Card`, `Input`, `Button`, `Form`.
-   **UX, Accessibility, and Security:** The view will show a confirmation message upon submission, advising the user to check their email. To prevent user enumeration, the message will be the same whether the email exists in the system or not.

-   **View Name:** Reset Password
-   **View Path:** `/reset-password`
-   **Main Purpose:** To allow users to set a new password using a token from their email.
-   **Key Information to Display:** New password and confirm password input fields.
-   **Key View Components:** `Card`, `Input`, `Button`, `Form`.
-   **UX, Accessibility, and Security:** The form will validate that the two password fields match and meet security requirements. The token will be read from the URL parameters.

### 2.2. Main Application Views

-   **View Name:** Dashboard
-   **View Path:** `/`
-   **Main Purpose:** To provide an overview of the user's trips and serve as the main entry point to the application.
-   **Key Information to Display:** A tabbed list of trips ("Ongoing" for current and future trips, "Past" for completed trips), a prominent "Create New Trip" button, and a usage counter for trips (e.g., "8/10 used").
-   **Key View Components:** `Tabs`, `Card` (for each trip), `Button`, `Skeleton` (for loading state), `EmptyState` component.
-   **UX, Accessibility, and Security:** Trips will be clearly organized and easy to access. The "Create New Trip" button will be disabled if the user has reached their 10-trip limit. The view will be fully responsive.

-   **View Name:** Create Trip
-   **View Path:** `/trips/new`
-   **Main Purpose:** To allow users to create a new trip.
-   **Key Information to Display:** A form with fields for Trip Name, Destination, Start Date, End Date, and Notes.
-   **Key View Components:** `Form`, `Input`, `DatePicker`, `Textarea`, `Button`.
-   **UX, Accessibility, and Security:** The form will use a date picker for intuitive date selection and provide clear validation for all fields (e.g., end date cannot be before start date). Upon successful creation, the user will be redirected to the new trip's detail page.

-   **View Name:** Trip Details
-   **View Path:** `/trips/[tripId]`
-   **Main Purpose:** To provide a comprehensive view for planning and managing a single trip.
-   **Key Information to Display:** Trip header with name and dates, and distinct sections for Transport, Accommodation, Days, and Files.
-   **Key View Components:** `Accordion` (for mobile layout), `InlineEdit` (for trip name), `Dialog` (for modals), `dnd-kit` list (for days/places), `FileUpload` component.
-   **UX, Accessibility, and Security:** Sections will be lazy-loaded to improve performance. Drag-and-drop functionality for reordering places will have keyboard-accessible alternatives (up/down buttons). All actions (add, edit, delete) will provide immediate feedback via loading states and toasts.

### 2.3. User and Admin Views

-   **View Name:** User Profile
-   **View Path:** `/profile`
-   **Main Purpose:** To allow users to manage their account settings and personal data.
-   **Key Information to Display:** Sections for updating username, changing password, and deleting the account.
-   **Key View Components:** `Form`, `Input`, `Button`, `Dialog` (for delete confirmation).
-   **UX, Accessibility, and Security:** The "Delete Account" action is a critical, destructive operation and will be protected by a confirmation modal that requires the user to re-enter their password, preventing accidental deletion.

-   **View Name:** Admin Dashboard
-   **View Path:** `/admin`
-   **Main Purpose:** To provide administrators with key metrics about application usage.
-   **Key Information to Display:** Data cards showing the number of trips, AI searches, and total users within a selectable time window (default 60 days).
-   **Key View components:** `Card`, `Chart` (e.g., Recharts), `DatePicker` (for date range).
-   **UX, Accessibility, and Security:** Access to this view will be strictly controlled by an "Admin" role check. The data displayed is read-only.

## 3. User Journey Map

The primary user journey involves planning a new trip.

1.  **Registration/Login:** A new user starts at `/register`, creates an account, and is directed to `/login`. An existing user starts at `/login`.
2.  **Dashboard:** Upon successful login, the user lands on the Dashboard (`/`). They see their existing trips or an empty state prompting them to create one.
3.  **Trip Creation:** The user clicks "Create New Trip," which navigates them to the `/trips/new` page. They fill out the form and click "Save."
4.  **Trip Details:** After creation, they are redirected to the Trip Details page (`/trips/[tripId]`). Here, the trip is broken down into manageable sections.
5.  **Adding Details:**
    -   The user adds **Transport** and **Accommodation** details.
    -   They create **Days** for their itinerary.
    -   For each day, they add **Places**. They can use the AI-powered search in a modal to get suggestions or add a place manually via a form.
    -   They upload relevant **Files** (tickets, confirmations) to the trip, transport, or accommodation sections.
6.  **Planning and Management:** The user can reorder places within a day using drag-and-drop and mark places as "visited." All changes are saved automatically with optimistic UI updates.
7.  **Profile Management:** At any time, the user can navigate to `/profile` from the main navigation to update their password or delete their account.
8.  **Logout:** The user clicks the "Logout" button in the main navigation, which invalidates their session and redirects them to the `/login` page.

## 4. Layout and Navigation Structure

The application will use a primary layout that includes a persistent header and a main content area.

-   **Header:** Contains the application logo, primary navigation links, and a user menu.
    -   **Logo:** Links back to the Dashboard (`/`).
    -   **Primary Navigation (Desktop):** Links to "Dashboard" and "Profile."
    -   **User Menu:** An avatar dropdown with links to "Profile" and a "Logout" button.
-   **Hamburger Menu (Mobile):** On smaller screens, the primary navigation will be collapsed into a `Sheet` (hamburger menu) that slides in from the side, providing access to all navigation links.
-   **Content Area:** This is where the main view for the current route is rendered.
-   **Protected Routes:** An `AuthGuard` layout component will wrap all authenticated routes. It will check for a valid session on the client-side. If the user is not authenticated, it will redirect them to the `/login` page, storing the intended destination to redirect them back after login.

## 5. Key Components

These are reusable components that will form the foundation of the UI.

-   **`Form`:** A wrapper around `React Hook Form` to standardize form creation, validation, and submission handling across the application.
-   **`Input` / `Textarea` / `DatePicker`:** Styled `shadcn/ui` components for consistent data entry fields.
-   **`Button`:** A primary action component with built-in loading states (showing a spinner) to provide user feedback and prevent duplicate submissions.
-   **`Card`:** A versatile component used to display self-contained pieces of information, such as trips on the dashboard or metrics in the admin panel.
-   **`Dialog` (Modal):** Used for focused tasks like the AI place search or critical confirmations (e.g., account deletion). It will manage focus and be dismissible via the Escape key for accessibility.
-   **`Toast`:** Provides non-intrusive global notifications for actions (e.g., "Trip saved successfully") and general API errors.
-   **`Skeleton`:** Placeholder component used to indicate loading states for content blocks (like the trip list), improving the perceived performance.
-   **`EmptyState`:** A component shown when a list is empty (e.g., no trips). It will contain a message, an icon, and a call-to-action button (e.g., "Create your first trip").
-   **`FileUpload`:** A dedicated component for handling file uploads, featuring drag-and-drop support, progress indicators, and validation for file type, size, and count.
-   **`InlineEdit`:** A component that displays text but converts to an input field on click, allowing for quick edits of fields like trip names or notes without navigating to a separate form.
