# View Implementation Plan: Trip Details

## 1. Overview

The Trip Details view provides a comprehensive interface for planning and managing a single trip. It displays the trip's core information and organizes details into distinct, lazy-loaded sections for Transport, Accommodation, Days, and Files. The view supports editing trip details, managing files, and organizing the trip's schedule.

## 2. View Routing

The view will be accessible at the following dynamic path:

- **Path:** `/trips/[tripId]`

## 3. Component Structure

The view will be composed of the following component hierarchy:

```
- TripDetailsPage (Astro)
  - TripHeader
    - InlineEdit (for trip name)
    - DateRangePicker
    - TripActions (Delete Trip, etc.)
  - TripContent
    - Tabs (Desktop) / Accordion (Mobile)
      - TransportSection (Lazy-loaded)
        - TransportList
          - TransportItem
            - FileList (Lazy-loaded)
      - AccommodationSection
        - AccommodationDetails
        - FileList (Lazy-loaded)
      - DaysSection (Lazy-loaded)
        - DayList (dnd-kit for reordering)
          - DayItem
            - PlaceList (dnd-kit for reordering)
              - PlaceItem
      - FilesSection (Lazy-loaded)
        - FileUpload
        - FileList
```

## 4. Component Details

### TripHeader

- **Component description:** Displays the trip's name, destination, dates, and action buttons. It allows for inline editing of the trip name.
- **Main elements:** `h1` for the trip name (using `InlineEdit`), `p` for destination and dates, `div` containing action buttons (`Button`).
- **Handled interactions:**
  - Editing the trip name via `InlineEdit`.
  - Opening the "Delete Trip" confirmation `Dialog`.
- **Handled validation:**
  - Trip name cannot be empty.
- **Types:** `TripDetailViewModel`
- **Props:** `trip: TripDetailViewModel`, `onUpdate: (data: UpdateTripRequest) => void`, `onDelete: () => void`

### TripContent

- **Component description:** A responsive container that uses `Tabs` on desktop and `Accordion` on mobile to display different sections of trip details. It manages the lazy loading of section content.
- **Main elements:** `Tabs` and `TabsContent` (Shadcn) for desktop, `Accordion` and `AccordionItem` for mobile.
- **Handled interactions:**
  - Switching between tabs/accordion items to trigger lazy loading of content.
- **Handled validation:** None.
- **Types:** `TripDetailViewModel`
- **Props:** `tripId: string`

### FilesSection

- **Component description:** Manages the display, upload, and deletion of files associated with the entire trip.
- **Main elements:** `FileUpload` component for adding new files, `FileList` component to list existing files.
- **Handled interactions:**
  - Uploading new files.
  - Deleting existing files.
  - Downloading/previewing files.
- **Handled validation:**
  - File type must be `PNG`, `JPEG`, or `PDF`.
  - File size must not exceed 5MB.
  - The total number of files for the trip cannot exceed 10.
- **Types:** `FileDto`, `FileUploadResponse`
- **Props:** `tripId: string`

## 5. Types

### `TripDetailViewModel`

A client-side view model to represent the trip data displayed in the view.

```typescript
export interface TripDetailViewModel {
  id: string;
  name: string;
  destination: string;
  startDate: string;
  endDate: string;
  notes?: string;
  accommodation?: {
    id: string;
    name: string;
    address?: string;
    checkInDate?: string;
    checkOutDate?: string;
    notes?: string;
  };
}
```

### `FileViewModel`

A client-side view model for file data.

```typescript
export interface FileViewModel {
  id: string;
  fileName: string;
  fileSize: number; // in bytes
  fileType: string;
  uploadedAt: string;
  previewUrl: string;
  downloadUrl: string;
}
```

## 6. State Management

A custom hook, `useTripDetails(tripId: string)`, will be created to manage the view's state.

- **Responsibilities:**
  - Fetching the initial trip data (`GET /api/trips/{tripId}`).
  - Providing functions to handle trip updates (`PATCH /api/trips/{tripId}`) and deletion (`DELETE /api/trips/{tripId}`).
  - Managing loading and error states for the view.
- **State Variables:**
  - `trip: TripDetailViewModel | null`
  - `isLoading: boolean`
  - `error: Error | null`

## 7. API Integration

- **Initial Data Fetch:**

  - **Endpoint:** `GET /api/trips/{tripId}`
  - **Action:** Called on component mount by the `useTripDetails` hook.
  - **Response Type:** `TripDetailDto` (maps to `TripDetailViewModel`).

- **Update Trip Name:**

  - **Endpoint:** `PATCH /api/trips/{tripId}`
  - **Action:** Triggered on `InlineEdit` save.
  - **Request Type:** `UpdateTripRequest` (`{ name: string }`)
  - **Response Type:** `TripDetailDto`

- **Delete Trip:**

  - **Endpoint:** `DELETE /api/trips/{tripId}`
  - **Action:** Triggered from the "Delete Trip" confirmation dialog.
  - **Response:** `204 No Content`. On success, redirect the user to the trips list.

- **File Operations (within `FilesSection`):**
  - **List:** `GET /api/trips/{tripId}/files` -> `FileListResponse`
  - **Upload:** `POST /api/trips/{tripId}/files` -> `FileUploadResponse`
  - **Delete:** `DELETE /api/files/{fileId}` -> `204 No Content`

## 8. User Interactions

- **Edit Trip Name:** User clicks the trip name, `InlineEdit` becomes active. On save, a `PATCH` request is sent. The UI shows a loading state and updates on success.
- **Delete Trip:** User clicks the delete button, a confirmation `Dialog` appears. On confirmation, a `DELETE` request is sent.
- **View Section:** User clicks a tab (desktop) or expands an accordion item (mobile). The corresponding section component is rendered, and its data is lazy-loaded.
- **Upload File:** User drags a file or clicks the `FileUpload` component. A `POST` request is sent with `multipart/form-data`. The file list updates optimistically or on success.
- **Delete File:** User clicks a delete icon on a file item. A confirmation is shown, and a `DELETE` request is sent.

## 9. Conditions and Validation

- **File Upload:** The `FileUpload` component will validate file type and size on the client-side before uploading. The upload button will be disabled if the trip file count is 10 or more.
- **Trip Name:** The `InlineEdit` component will not allow saving an empty name.
- **Authentication:** All API calls require an authenticated user. If a request fails with a 401 or 403 status, the user should be redirected to the login page.

## 10. Error Handling

- **Data Fetching Errors:** If the initial `GET /api/trips/{tripId}` fails (e.g., 404 Not Found), display a full-page error component with a "Go Back" button.
- **Update/Delete Errors:** If `PATCH` or `DELETE` requests fail, show a toast notification (e.g., using `Sonner`) with a descriptive error message.
- **File Operation Errors:** Display toast notifications for failures in uploading or deleting files. For a `409 Conflict` (file limit), show a specific error message.
- **Loading States:** All interactive elements (buttons, lists) will show loading indicators (e.g., spinners) during asynchronous operations to provide feedback.

## 11. Implementation Steps

1.  **Create Page and Routing:** Create the Astro page file at `Client/src/pages/trips/[tripId].astro`.
2.  **Develop `useTripDetails` Hook:** Implement the custom hook to fetch and manage the main trip data.
3.  **Build `TripHeader` Component:** Create the header with `InlineEdit` for the name and action buttons.
4.  **Build `TripContent` Component:** Implement the responsive Tabs/Accordion structure for the main content sections.
5.  **Implement Lazy Loading:**
    - Create placeholder components for `TransportSection`, `DaysSection`, and `FilesSection`.
    - Use a state variable to conditionally render these components when their corresponding tab/accordion is activated.
6.  **Develop `FilesSection`:**
    - Create the `FileUpload` component with client-side validation.
    - Create the `FileList` component to display, preview, download, and delete files.
    - Integrate API calls for all file operations.
7.  **Develop `AccommodationSection`:** Create the component to display the accommodation details fetched in the initial load.
8.  **Develop `TransportSection` and `DaysSection`:** Implement the lazy-loaded list components for transport and days (as per future implementation plans).
9.  **Add Error Handling and Toasts:** Integrate `Sonner` or a similar library to show feedback for all user actions and errors.
10. **Finalize Styling and Accessibility:** Ensure the view is fully responsive and all interactive elements are keyboard-accessible.
