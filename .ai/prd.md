# Product Requirements Document (PRD) - Trivare

## 1. Product Overview
Trivare is an MVP web application that supports the planning, organization, and storage of trip information. It allows users to create, edit, and manage trips, store files (e.g., tickets), and use AI and the Google Places API to search for attractions and restaurants. The system provides simple registration, login, and an admin panel with basic metrics.

## 2. User Problem
Planning engaging trips is time-consuming and requires gathering information from multiple sources. Users struggle with storing tickets, reservations, and planning attractions in one place. There is a lack of a tool that would allow them to quickly find interesting places, store all data, and manage it securely and conveniently.

## 3. Functional Requirements
1.  Registration, login, and password reset (authentication via email/password, JWT).
2.  Trip CRUD: create, edit, view, delete.
3.  Ability to add days to a trip and details (transport, accommodation, attractions).
4.  Manual addition of places and automatic retrieval of suggestions from the Google Places API.
5.  AI integration (OpenRouter.ai) to filter and recommend places based on preferences.
6.  File storage (PNG, JPEG, PDF, max 5MB) associated with trips and days, with a limit of 10 files per trip.
7.  User account system with a profile and account management options.
8.  Admin panel with metrics (number of trips, AI usage, number of searches).
9.  Ability to permanently delete an account and associated data (GDPR).
10. Limitations: 10 trips per user, notifications about approaching limits.

## 4. Product Boundaries
-   No trip sharing between users.
-   No advanced multimedia support (only basic files: PDF, PNG, JPEG).
-   No advanced logistics and time planning.
-   No notification system.
-   No mobile app and no public API.
-   No caching of Google API results in the MVP (architecture prepared for the future).
-   Access to the admin panel is restricted to the admin role only.

## 5. User Stories

### US-001: Account Registration
-   **Title:** New user registration
-   **Description:** As a new user, I want to register an account using my email and password so I can use the application.
-   **Acceptance Criteria:**
    -   The user can create an account by providing an email and password.
    -   The system validates the uniqueness of the email.

### US-002: Login
-   **Title:** Logging into the application
-   **Description:** As a registered user, I want to log in to access my trips.
-   **Acceptance Criteria:**
    -   The user can log in by providing correct credentials.
    -   The system generates a JWT access and refresh token.
    -   Incorrect credentials result in an error message.

### US-003: Password Reset
-   **Title:** Recovering account access
-   **Description:** As a user, I want to reset my password to regain access to my account if I lose it.
-   **Acceptance Criteria:**
    -   The user can initiate a password reset via email.
    -   The user receives a link to change the password.
    -   After changing the password, the user can log in with the new password.

### US-004: Creating a Trip
-   **Title:** Adding a new trip
-   **Description:** As a user, I want to create a new trip to plan my journey.
-   **Acceptance Criteria:**
    -   The user can add a trip with a name, date, destination, and general notes.
    -   The system validates the 10-trip limit.

### US-005: Editing and Deleting a Trip
-   **Title:** Managing trips
-   **Description:** As a user, I want to edit or delete trips to update or remove unnecessary data.
-   **Acceptance Criteria:**
    -   The user can edit trip details.
    -   The user can delete a trip.

### US-006: Adding Days and Details to a Trip
-   **Title:** Planning trip details
-   **Description:** As a user, I want to add days to a trip and details (transport, accommodation, attractions) to better plan my journey.
-   **Acceptance Criteria:**
    -   The user can add any number of days to a trip.
    -   Each day can contain attraction details and notes.
    -   The trip can have details about transport and accommodation.
    -   Transport and accommodation details for the trip are optional (name, address, date, time, notes) and separate files can be added to them.

### US-007: Adding Places Manually
-   **Title:** Adding custom places
-   **Description:** As a user, I want to manually add a place to a trip to include non-standard attractions.
-   **Acceptance Criteria:**
    -   The user can add a place with a name, address, opening hours, link, and notes.

### US-008: Searching for Places via Google Places API
-   **Title:** Automatic attraction search and AI suggestions
-   **Description:** As a user, I want to search for place suggestions via the Google Places API with the help of AI to find attractions faster.
-   **Acceptance Criteria:**
    -   The user can provide a location and a keyword.
    -   The Google Places API retrieves nearby places, and the AI system filters and ranks the suggestions.
    -   The system returns 5 place suggestions.
    -   The user can accept and add the selected places to the trip.

### US-009: Storing Files
-   **Title:** Adding files to a trip
-   **Description:** As a user, I want to add files (e.g., tickets) to a trip, transport, accommodation, or day to have all documents in one place.
-   **Acceptance Criteria:**
    -   The user can add PNG, JPEG, PDF files (max 5MB).
    -   A limit of 10 files per trip.
    -   The system informs the user when they are approaching the limit.

### US-010: Profile Management
-   **Title:** Editing user profile
-   **Description:** As a user, I want to edit my profile data and manage my account.
-   **Acceptance Criteria:**
    -   The user can edit their profile data.
    -   The user can delete their account and associated data (GDPR).

### US-011: Marking Places as Visited
-   **Title:** Marking attractions
-   **Description:** As a user, I want to mark places as visited to track the progress of my trip.
-   **Acceptance Criteria:**
    -   The user can mark a place as visited on a given day.
    -   The system updates the status of the place.

### US-012: Admin Panel
-   **Title:** Access to metrics
-   **Description:** As an administrator, I want to have access to a panel with metrics to monitor user activity.
-   **Acceptance Criteria:**
    -   Only an admin has access to the panel.
    -   The panel displays the number of trips, searches, and AI uses within a 60-day window.
    -   The system logs events in the database.

### US-013: Secure Authentication and Authorization
-   **Title:** Secure data access
-   **Description:** As a user, I want to be sure that my data is protected and accessible only after logging in.
-   **Acceptance Criteria:**
    -   Access to trip and file data only after logging in.
    -   JWT tokens with appropriate time limits.
    -   The system prevents unauthorized access.

## 6. Success Metrics
-   75% of users enter data for more than one trip.
-   50% of users use AI for place suggestions.
-   Number of registrations/logins.
-   Number of trips created within a 60-day window.
-   Number of place searches and AI uses.
-   A functioning login system, trip CRUD, Google Places API + AI integration, and compliance with minimum GDPR requirements.
