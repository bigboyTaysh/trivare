# View Implementation Plan: Dashboard

## 1. Overview
The Dashboard view serves as the main entry point for authenticated users. It provides a comprehensive overview of their trips, categorized into "Ongoing" (current and future trips) and "Past" (completed trips) sections within a tabbed interface. The view also displays a usage counter for the trip limit (e.g., "8/10 used") and includes a primary call-to-action button for creating a new trip.

## 2. View Routing
- **Path:** `/`
- **File Location:** The view will be implemented in `src/pages/index.astro`, which will render a client-side React component, `DashboardView.tsx`, located at `src/components/views/DashboardView.tsx`.

## 3. Component Structure
The view will be composed of a hierarchy of React components, leveraging the Shadcn/ui library for UI elements.

```
index.astro
└── DashboardView.tsx (Client Component)
    ├── DashboardSkeleton.tsx (Displays during initial data fetch)
    ├── ErrorDisplay.tsx (Displays on API error)
    └── DashboardHeader.tsx
        ├── TripCounter.tsx
        └── CreateTripButton.tsx
    └── TripTabs.tsx (Shadcn Tabs)
        ├── Tab: "Ongoing"
        │   └── TripList.tsx
        │       ├── TripCard.tsx
        │       └── EmptyState.tsx
        └── Tab: "Past"
            └── TripList.tsx
                ├── TripCard.tsx
                └── EmptyState.tsx
```

## 4. Component Details

### `DashboardView.tsx`
- **Description:** The root component for the dashboard, responsible for orchestrating data fetching, state management (loading, error, data), and rendering the appropriate child components based on the current state.
- **Main Elements:** Uses a custom hook `useDashboardData` to manage state. Conditionally renders `DashboardSkeleton`, an error message, or the main dashboard layout.
- **Handled Interactions:** None directly.
- **Handled Validation:** None.
- **Types:** `DashboardViewModel`.
- **Props:** None.

### `DashboardHeader.tsx`
- **Description:** A container component for the view's header, including the title, trip counter, and the "Create New Trip" button.
- **Main Elements:** `div`, `h1`, `TripCounter`, `CreateTripButton`.
- **Handled Interactions:** None.
- **Handled Validation:** None.
- **Types:** `DashboardViewModel`.
- **Props:**
  ```typescript
  interface DashboardHeaderProps {
    totalTripCount: number;
    tripLimit: number;
  }
  ```

### `CreateTripButton.tsx`
- **Description:** A button that navigates the user to the trip creation page. It is disabled if the user has reached their trip limit.
- **Main Elements:** Shadcn `Button`.
- **Handled Interactions:** `onClick` event to navigate to the "/trips/create" page.
- **Handled Validation:** The button's `disabled` state is set to `true` if `totalTripCount >= tripLimit`.
- **Types:** None.
- **Props:**
  ```typescript
  interface CreateTripButtonProps {
    totalTripCount: number;
    tripLimit: number;
  }
  ```

### `TripTabs.tsx`
- **Description:** Uses the Shadcn `Tabs` component to create the "Ongoing" and "Past" trip sections. The "Ongoing" tab combines current and future trips.
- **Main Elements:** Shadcn `Tabs`, `TabsList`, `TabsTrigger`, `TabsContent`. Each `TabsContent` will contain a `TripList` component.
- **Handled Interactions:** Handles tab selection to display the corresponding list of trips.
- **Handled Validation:** None.
- **Types:** `TripListDto`.
- **Props:**
  ```typescript
  interface TripTabsProps {
    ongoingTrips: TripListDto[];
    pastTrips: TripListDto[];
  }
  ```

### `TripList.tsx`
- **Description:** Renders a grid of `TripCard` components. If the provided list of trips is empty, it renders the `EmptyState` component.
- **Main Elements:** A responsive grid (`div` with Tailwind CSS grid classes). It maps over the `trips` prop to render `TripCard` components.
- **Handled Interactions:** None.
- **Handled Validation:** Checks if the `trips` array is empty to conditionally render the `EmptyState` component.
- **Types:** `TripListDto`.
- **Props:**
  ```typescript
  interface TripListProps {
    trips: TripListDto[];
  }
  ```

### `TripCard.tsx`
- **Description:** Displays a summary of a single trip, including its name, destination, and dates. The entire card is a clickable link that navigates to the detailed view of the trip.
- **Main Elements:** Shadcn `Card`, `CardHeader`, `CardTitle`, `CardDescription`, `CardContent`. The root element is an `<a>` tag for navigation.
- **Handled Interactions:** `onClick` (via navigation link) to go to `/trips/{trip.id}`.
- **Handled Validation:** None.
- **Types:** `TripListDto`.
- **Props:**
  ```typescript
  interface TripCardProps {
    trip: TripListDto;
  }
  ```

## 5. Types

### API DTOs (Expected from Backend)
```typescript
// Corresponds to TripListDto.cs
interface TripListDto {
  id: string; // Guid
  name: string;
  destination?: string;
  startDate: string; // Format: "YYYY-MM-DD"
  endDate: string; // Format: "YYYY-MM-DD"
  notes?: string;
  createdAt: string; // ISO 8601 DateTime string
}

// Corresponds to TripListResponse.cs
interface TripListResponse {
  data: TripListDto[];
  pagination: {
    page: number;
    pageSize: number;
    totalItems: number;
    totalPages: number;
  };
}
```

### ViewModels (Frontend-specific state)
```typescript
interface DashboardViewModel {
  // Categorized data
  ongoingTrips: TripListDto[]; // Current and future trips combined
  pastTrips: TripListDto[];

  // Metadata for UI logic
  totalTripCount: number;
  readonly tripLimit: 10;

  // UI state flags
  isLoading: boolean;
  error: Error | null;
}
```

## 6. State Management
State will be managed locally within the `DashboardView` component using a custom hook, `useDashboardData`.

### `useDashboardData` Hook
- **Purpose:** To encapsulate all logic related to fetching, processing, and storing the dashboard's data and UI state.
- **Functionality:**
    1.  Initializes state with `isLoading: true`.
    2.  Uses a `useEffect` to fetch data from `/api/trips` on component mount.
    3.  On successful fetch, it processes the raw trip list:
        - It iterates through the trips and categorizes each one as "Ongoing" or "Past" by comparing its `endDate` with the current date.
        - "Ongoing" includes trips where the end date is today or in the future (combines current and future trips).
        - "Past" includes trips where the end date is before today.
        - Date strings from the API ("YYYY-MM-DD") must be parsed into `Date` objects for accurate comparison.
    4.  Updates the state with the categorized lists, `totalTripCount`, and sets `isLoading` to `false`.
    5.  If the fetch fails, it catches the error, updates the `error` state, and sets `isLoading` to `false`.
- **Return Value:** It will return an object matching the `DashboardViewModel` interface.

## 7. API Integration
- **Endpoint:** `GET /api/trips`
- **Service Function:** A dedicated function, `getTrips`, will be created in `src/services/api.ts`.
- **Request:**
  - **Method:** `GET`
  - **URL:** `/api/trips?pageSize=50&sortBy=startDate&sortOrder=asc`
    - `pageSize=50`: To ensure all trips are fetched at once (well above the user limit of 10).
    - `sortBy=startDate`: To receive trips in a logical order.
  - **Headers:** The API client must attach the `Authorization: Bearer <JWT>` header for authentication.
- **Response:**
  - **Type:** The function will return a `Promise<TripListResponse>`.
  - **Success:** The `data` and `pagination` objects are used by the `useDashboardData` hook.
  - **Error:** The function should throw an error for non-2xx responses, which will be caught by the hook.

## 8. User Interactions
- **View Load:** The user sees a skeleton loader while data is being fetched.
- **Tab Navigation:** Clicking on a tab ("Ongoing," "Past") immediately displays the content for that category without a new API call.
- **Create Trip:** Clicking the "Create New Trip" button navigates the user to the `/trips/create` route. This action is disabled if the trip limit is reached.
- **View Trip Details:** Clicking anywhere on a `TripCard` navigates the user to the detailed view for that trip, e.g., `/trips/{trip.id}`.

## 9. Conditions and Validation
- **Authentication:** All API requests will include a JWT. If a `401 Unauthorized` response is received, the user will be redirected to the `/login` page.
- **Trip Limit:** The "Create New Trip" button in `CreateTripButton.tsx` will be disabled using the following condition: `props.totalTripCount >= props.tripLimit`.
- **Empty State:** In `TripList.tsx`, a check for `props.trips.length === 0` will determine whether to display the list of cards or the `EmptyState` component.

## 10. Error Handling
- **API/Network Errors:** If the `useDashboardData` hook fails to fetch data, it will update its state with an `error` object. The `DashboardView` component will detect this state and render a user-friendly error message (e.g., "Failed to load trips. Please refresh the page.") instead of the main dashboard content.
- **Unauthorized Access:** A `401` status from the API should trigger a global response handler that clears any stored session data and redirects the user to `/login`.

## 11. Implementation Steps
1.  **Create File Structure:** Create the following new files:
    - `src/components/views/DashboardView.tsx`
    - `src/components/views/dashboard/DashboardHeader.tsx`
    - `src/components/views/dashboard/CreateTripButton.tsx`
    - `src/components/views/dashboard/TripTabs.tsx`
    - `src/components/views/dashboard/TripList.tsx`
    - `src/components/views/dashboard/TripCard.tsx`
    - `src/components/views/dashboard/DashboardSkeleton.tsx`
    - `src/hooks/useDashboardData.ts`
2.  **Update API Service:** Add the `getTrips` function to `src/services/api.ts` to fetch data from the `GET /api/trips` endpoint.
3.  **Define Types:** Add the `TripListDto`, `TripListResponse`, and `DashboardViewModel` TypeScript interfaces to a relevant types file (e.g., `src/types/trips.ts`).
4.  **Implement `useDashboardData` Hook:** Create the custom hook to handle all state logic: fetching, categorizing, and managing loading/error states.
5.  **Build Skeleton and Child Components:** Implement the presentational components (`DashboardSkeleton`, `TripCard`, `TripList`, etc.), ensuring they are styled with Tailwind CSS and correctly accept props.
6.  **Implement `DashboardView`:** Assemble the main view component. Use the `useDashboardData` hook to get the state and conditionally render the skeleton, error message, or the full dashboard layout.
7.  **Update Astro Page:** In `src/pages/index.astro`, import and render the `DashboardView.tsx` component with the `client:load` directive to make it interactive. Ensure this page is protected and only accessible to authenticated users.
8.  **Testing:** Manually test all user interactions, loading states, empty states, the disabled "Create" button, and error handling scenarios.
