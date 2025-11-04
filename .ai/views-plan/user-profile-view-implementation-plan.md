# View Implementation Plan: User Profile

## 1. Overview

The User Profile view allows authenticated users to manage their account. It provides functionality to update their username, change their password, and permanently delete their account. The view will be composed of several distinct forms and a confirmation dialog for the deletion action to ensure a secure and clear user experience.

## 2. View Routing

The User Profile view will be accessible at the following path:

- **Path:** `/profile`

This will be a protected route, accessible only to authenticated users.

## 3. Component Structure

The view will be built using a hierarchical component structure to ensure separation of concerns and reusability.

```
/src/pages/profile.astro
└── /src/components/views/UserProfileView.tsx
    ├── /src/components/forms/UpdateUsernameForm.tsx
    │   ├── Card, CardHeader, CardTitle, CardContent
    │   ├── Form, FormField, FormItem, FormLabel, FormControl, FormMessage
    │   ├── Input
    │   └── Button
    ├── /src/components/forms/ChangePasswordForm.tsx
    │   ├── Card, CardHeader, CardTitle, CardContent
    │   ├── Form, FormField, FormItem, FormLabel, FormControl, FormMessage
    │   ├── Input (for current, new, and confirm password)
    │   └── Button
    └── /src/components/features/DeleteAccountSection.tsx
        ├── Card, CardHeader, CardTitle, CardDescription, CardContent
        ├── Button (triggers dialog)
        └── /src/components/dialogs/DeleteAccountDialog.tsx
            ├── Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter
            ├── Form, FormField, FormItem, FormLabel, FormControl, FormMessage
            ├── Input (for password confirmation)
            └── Button (Cancel and Confirm)
```

## 4. Component Details

### `UserProfileView.tsx`

- **Description:** The main container component that fetches user data and manages the overall state for the profile page, including loading, error, and dialog visibility.
- **Main elements:** Renders `UpdateUsernameForm`, `ChangePasswordForm`, and `DeleteAccountSection`.
- **Handled interactions:** Fetches user data on component mount. Manages the open/closed state of the `DeleteAccountDialog`.
- **Validation:** None.
- **Types:** `UserDto`.
- **Props:** None.

### `UpdateUsernameForm.tsx`

- **Description:** A form dedicated to updating the user's username. It displays the current username and provides an input for a new one.
- **Main elements:** `Card`, `Form`, `Input` for username, `Button` for submission.
- **Handled interactions:** Form submission to trigger the API call for updating the username.
- **Handled validation:**
  - Username is required.
  - Username must be 3 to 50 characters long.
  - Username must only contain letters, numbers, underscores (`_`), and hyphens (`-`).
- **Types:** `UpdateUsernameViewModel`, `UserDto`.
- **Props:** `{ user: UserDto; onUpdate: (newUsername: string) => void; }`.

### `ChangePasswordForm.tsx`

- **Description:** A form for changing the user's password. It requires the current password for verification and the new password to be entered twice for confirmation.
- **Main elements:** `Card`, `Form`, `Input` fields for current password, new password, and confirm new password, and a `Button` for submission.
- **Handled interactions:** Form submission to trigger the password change API call.
- **Handled validation:**
  - All fields are required.
  - Current password must be at least 8 characters.
  - New password must be at least 8 characters.
  - New password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (`@$!%*?&`).
  - "Confirm New Password" must match "New Password".
- **Types:** `ChangePasswordViewModel`.
- **Props:** `{ onSubmit: (data: ChangePasswordViewModel) => void; }`.

### `DeleteAccountSection.tsx`

- **Description:** A section that informs the user about the consequences of deleting their account and provides a button to initiate the process.
- **Main elements:** `Card` with descriptive text, a `Button` to open the `DeleteAccountDialog`.
- **Handled interactions:** Clicking the button opens the confirmation dialog.
- **Validation:** None.
- **Types:** None.
- **Props:** `{ onOpenDialog: () => void; }`.

### `DeleteAccountDialog.tsx`

- **Description:** A modal dialog that serves as a final confirmation step for account deletion. It requires the user to re-enter their password to proceed.
- **Main elements:** `Dialog`, `Form`, `Input` for the user's password, a "Cancel" `Button`, and a "Delete Account" `Button`.
- **Handled interactions:**
  - Form submission to trigger the account deletion API call.
  - Clicking "Cancel" or closing the dialog aborts the action.
- **Handled validation:** Password field is required.
- **Types:** `DeleteAccountViewModel`.
- **Props:** `{ isOpen: boolean; onClose: () => void; onDelete: (password: string) => void; }`.

## 5. Types

Detailed TypeScript definitions for DTOs and ViewModels.

```typescript
// --- DTOs (from API) ---

/**
 * User profile data received from the backend.
 */
export interface UserDto {
  id: string;
  userName: string;
  email: string;
  createdAt: string;
  roles: string[];
}

/**
 * Request body for updating user profile (username/password).
 */
export interface UpdateUserRequest {
  userName?: string;
  currentPassword?: string;
  newPassword?: string;
}

/**
 * Request body for deleting a user account.
 * NOTE: This is a new type for a proposed endpoint.
 */
export interface DeleteAccountRequest {
  password: string;
}

// --- ViewModels (Frontend-specific) ---

/**
 * ViewModel for the Update Username form.
 */
export interface UpdateUsernameViewModel {
  userName: string;
}

/**
 * ViewModel for the Change Password form, including confirmation field.
 */
export interface ChangePasswordViewModel {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

/**
 * ViewModel for the Delete Account confirmation dialog.
 */
export interface DeleteAccountViewModel {
  password: string;
}
```

## 6. State Management

A custom hook, `useUserProfile`, will be created to encapsulate the business logic, including API calls, state management for user data, loading, and errors. This approach will keep the UI components clean and focused on presentation.

- **`useUserProfile` Hook:**
  - **State:**
    - `user: UserDto | null`: Stores the fetched user profile.
    - `isLoading: boolean`: Tracks loading state for the initial data fetch.
    - `isUpdating: boolean`: Tracks loading state for update/delete operations.
    - `error: Error | null`: Stores any API errors.
  - **Actions:**
    - `fetchUser()`: Fetches the user profile.
    - `updateUsername(data: UpdateUsernameViewModel)`: Updates the username.
    - `changePassword(data: ChangePasswordViewModel)`: Changes the password.
    - `deleteAccount(data: DeleteAccountViewModel)`: Deletes the account.

The individual forms will manage their own transient state using `react-hook-form` and `zod` for validation.

## 7. API Integration

The view will interact with three endpoints under `/api/users`. API calls will be managed within the `useUserProfile` hook.

- **`GET /api/users/me`**

  - **Purpose:** Fetch the current user's profile data.
  - **Action:** Called on component mount.
  - **Response Type:** `UserDto`.

- **`PATCH /api/users/me`**

  - **Purpose:** Update the username or password.
  - **Action:** Called on form submission from `UpdateUsernameForm` or `ChangePasswordForm`.
  - **Request Type:** `UpdateUserRequest`.
  - **Response Type:** `UserDto`.

- **`DELETE /api/users/me`**
  - **Purpose:** Permanently delete the user's account.
  - **Action:** Called on form submission from `DeleteAccountDialog`.
  - **Request Type:** `DeleteAccountRequest`.
  - **Response:** `204 No Content` on success.
  - **Note:** This endpoint needs to be created in the backend.

## 8. User Interactions

- **Page Load:** The view displays a loading state, fetches user data, and then renders the forms with the user's information.
- **Username Update:** The user types a new username and submits. The form shows a loading state. On success, a success toast is displayed and the username is updated in the UI. On failure, an error message is shown.
- **Password Change:** The user fills all three password fields and submits. The form shows a loading state. On success, a success toast appears and the form fields are cleared. On failure (e.g., incorrect current password), an error message is shown.
- **Account Deletion:**
  1. User clicks the "Delete Account" button, which opens a confirmation dialog.
  2. The user must type their current password into the input field.
  3. The user clicks the final "Delete Account" button in the dialog.
  4. On success, the user is logged out and redirected to the home page. On failure, an error is shown within the dialog.

## 9. Conditions and Validation

Client-side validation will be implemented using `react-hook-form` and `zod` to provide immediate feedback to the user and ensure data integrity before sending requests to the API.

- **`UpdateUsernameForm`:** Validates username length (3-50) and allowed characters (`/^[a-zA-Z0-9_-]+$/`). The submit button is disabled until the form is valid and dirty.
- **`ChangePasswordForm`:** Validates password complexity (min 8 chars, uppercase, lowercase, number, special char) and ensures the new password and confirmation password match. The submit button is disabled until the form is valid.
- **`DeleteAccountDialog`:** Validates that the password field is not empty. The final delete button is disabled until a password is provided.

## 10. Error Handling

- **API Errors:** API call errors will be caught in the `useUserProfile` hook. The error message will be extracted from the response and displayed to the user via `Sonner` (toast notifications) or as a form error message.
- **401 Unauthorized:** If any API call returns a 401 status, the user will be automatically logged out and redirected to the `/login` page.
- **Network Errors:** A generic error toast ("A network error occurred. Please try again.") will be shown for failed `fetch` requests.
- **Validation Errors:** Handled by `react-hook-form` and displayed below the respective input fields.

## 11. Implementation Steps

1.  Create a new Astro page at `src/pages/profile.astro`. This page will render the main React view component and ensure it's client-side rendered.
2.  Create the main view component `src/components/views/UserProfileView.tsx`.
3.  Implement the `useUserProfile` custom hook to handle all state logic and API interactions. Define the required API service functions for fetching, updating, and deleting.
4.  Create the `UpdateUsernameForm.tsx` component with validation schema.
5.  Create the `ChangePasswordForm.tsx` component with validation schema.
6.  Create the `DeleteAccountSection.tsx` and the corresponding `DeleteAccountDialog.tsx` components.
7.  Define all required TypeScript types (`DTOs` and `ViewModels`) in a central types file (e.g., `src/types/user.ts`).
8.  Assemble all components within `UserProfileView.tsx`, passing down state and callbacks from the `useUserProfile` hook.
9.  Add toast notifications for success and error states for all user actions using `Sonner`.
10. Ensure the new `DELETE /api/users/me` endpoint requirements are communicated to the backend team for implementation.
11. Style all components using Tailwind CSS and components from `Shadcn/ui` to match the application's design system.
12. Test all user flows: successful updates, validation errors, API errors, and the full account deletion process.
