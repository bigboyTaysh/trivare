# View Implementation Plan: Reset Password

## 1. Overview

This document outlines the implementation plan for the "Reset Password" view. The purpose of this view is to allow users to set a new password for their account by using a secure token received via email. The view will consist of a form where the user can enter and confirm their new password.

## 2. View Routing

- **Path:** `/reset-password`
- **Description:** This page will be accessed via a link sent to the user's email. The URL will contain a `token` as a query parameter (e.g., `/reset-password?token=...`).

## 3. Component Structure

The view will be composed of the following component hierarchy:

```
ResetPasswordPage.astro
└── Layout.astro
    └── ResetPasswordForm.tsx (client:load)
        └── Card (Shadcn/ui)
            ├── CardHeader
            ├── CardContent
            │   └── Form (react-hook-form)
            │       ├── Input (for New Password)
            │       └── Input (for Confirm Password)
            └── CardFooter
                └── Button (submit)
```

## 4. Component Details

### `ResetPasswordPage.astro`

- **Component Description:** This is the main Astro page component for the view. It sets up the page layout and is responsible for reading the `token` from the URL query parameters and passing it down to the interactive React form component.
- **Main Elements:** It will use the main `Layout.astro` and will render the `ResetPasswordForm` component as a client-side hydrated island.
- **Handled Interactions:** None directly. It only extracts the token from the URL on the server side.
- **Handled Validation:** None.
- **Types:** None.
- **Props:** None.

### `ResetPasswordForm.tsx`

- **Component Description:** A client-side React component that provides the user interface for resetting the password. It contains the form with password fields, handles user input, performs validation, and communicates with the API.
- **Main Elements:**
  - `Card`, `CardHeader`, `CardContent`, `CardFooter` for the layout.
  - `Form`, `FormField`, `FormItem`, `FormLabel`, `FormControl`, `FormMessage` for the form structure and validation messages.
  - `Input` for password entry.
  - `Button` for form submission.
- **Handled Interactions:**
  - `onChange` on `Input` fields to update the form state.
  - `onSubmit` on the `Form` to trigger validation and the API call.
- **Handled Validation:**
  - **New Password:**
    - Must not be empty.
    - Must be at least 8 characters long.
    - Must contain at least one uppercase letter, one lowercase letter, one number, and one special character.
  - **Confirm Password:**
    - Must not be empty.
    - Must match the "New Password" field.
- **Types:**
  - `ResetPasswordViewModel`
  - `ResetPasswordRequest`
- **Props:**
  - `token: string`: The password reset token passed from `ResetPasswordPage.astro`.

## 5. Types

### `ResetPasswordViewModel` (New Type)

This ViewModel represents the state of the form within the `ResetPasswordForm` component.

```typescript
// In: src/types/viewModels.ts
export interface ResetPasswordViewModel {
  newPassword: string;
  confirmPassword: string;
}
```

### `ResetPasswordRequest` (Existing Backend DTO)

This type represents the data payload sent to the API endpoint. A corresponding frontend type should be defined.

```typescript
// In: src/types/auth.ts
export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}
```

## 6. State Management

State will be managed locally within the `ResetPasswordForm.tsx` component using React hooks. A form management library like `react-hook-form` with `zod` for schema validation is recommended to handle form state, validation, and submission logic efficiently.

- **Form State:** An object conforming to `ResetPasswordViewModel` will hold the values of the input fields.
- **Loading State:** A boolean state (e.g., `isSubmitting`) will be used to disable the submit button and provide visual feedback during the API call.
- **Error State:** A string state (e.g., `apiError`) will store any errors returned from the API to display to the user.

A custom hook is not necessary for this view, as the logic is self-contained within the `ResetPasswordForm` component.

## 7. API Integration

- **Endpoint:** `POST /api/auth/reset-password`
- **Action:** The `onSubmit` handler in `ResetPasswordForm.tsx` will trigger the API call.
- **Request:**
  - The function will construct a `ResetPasswordRequest` object.
  - The `token` will be sourced from the component's props.
  - The `newPassword` will be taken from the validated form state.
- **Response:**
  - **On Success (200 OK):** The user will be shown a success message and redirected to the login page (`/login`).
  - **On Error (400/404):** The error message from the API response will be displayed to the user.

## 8. User Interactions

1.  **Page Load:** The user arrives at the page from an email link. The form is displayed.
2.  **Typing in Fields:** The user enters their new password and confirms it. Validation messages appear if the inputs do not meet the requirements.
3.  **Submission:** The user clicks the "Reset Password" button.
    - If validation fails, error messages are displayed, and the submission is blocked.
    - If validation passes, the button becomes disabled, and a loading indicator may appear.
4.  **API Response:**
    - **Success:** A success message is shown (e.g., using a toast notification), and the user is redirected to the login page after a short delay.
    - **Failure:** An error message is displayed on the form, and the user can try again.

## 9. Conditions and Validation

- **Token Presence:** The `ResetPasswordPage.astro` should ideally check if the `token` parameter exists in the URL. If not, it could display a message like "Invalid reset link."
- **Password Matching:** The `ResetPasswordForm` will validate that `newPassword` and `confirmPassword` are identical before enabling form submission.
- **Password Strength:** The form will enforce password complexity rules (length, character types) on the `newPassword` field.
- **Form State:** The "Reset Password" button will be disabled if the form is invalid or if a submission is in progress.

## 10. Error Handling

- **Invalid/Expired Token:** If the API returns a `400` or `404` error, the form will display a message like "This password reset link is invalid or has expired. Please request a new one."
- **Weak Password:** If the API rejects the password for being too weak (though this should be caught by frontend validation), the message from the API will be displayed.
- **Network/Server Error:** For 5xx errors or network failures, a generic error message like "An unexpected error occurred. Please try again later." will be shown.
- **Missing Token:** If the page is loaded without a token, a message indicating an invalid link should be displayed.

## 11. Implementation Steps

1.  **Create `ResetPasswordPage.astro`:**
    - Set up the route in `src/pages/reset-password.astro`.
    - Use the shared `Layout.astro`.
    - Implement server-side logic to read the `token` from `Astro.url.searchParams`.
    - Render the `ResetPasswordForm` component, passing the `token` as a prop. Ensure it's loaded as a client island (`client:load`).
2.  **Define Types:**
    - Add the `ResetPasswordViewModel` interface to `src/types/viewModels.ts`.
    - Add the `ResetPasswordRequest` interface to `src/types/auth.ts`.
3.  **Create `ResetPasswordForm.tsx`:**
    - Create the file in `src/components/forms/ResetPasswordForm.tsx`.
    - Build the UI using Shadcn/ui components (`Card`, `Form`, `Input`, `Button`).
    - Set up form handling using `react-hook-form` and a `zod` schema for validation, including password strength and matching fields.
4.  **Implement Form Logic:**
    - Define the `onSubmit` function that will be called when the form is valid.
    - Inside `onSubmit`, manage loading and error states.
5.  **Integrate API Call:**
    - Create a function in `src/services/api.ts` to call the `POST /api/auth/reset-password` endpoint.
    - Call this service function from the `onSubmit` handler in the form component.
6.  **Handle API Responses:**
    - On success, display a toast notification (e.g., using `sonner`) and programmatically navigate the user to `/login`.
    - On failure, update the error state to display a message to the user.
7.  **Refine UX and Styling:**
    - Ensure the form is responsive and accessible.
    - Add loading indicators to the submit button.
    - Style validation and API error messages clearly.
8.  **Testing:**
    - Manually test the form with valid data, invalid data (mismatched passwords, weak passwords), and after the token has expired (if possible).
    - Verify successful redirection and error message displays.
