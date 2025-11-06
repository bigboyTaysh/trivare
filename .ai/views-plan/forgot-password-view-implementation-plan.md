# View Implementation Plan: Forgot Password

## 1. Overview

This document outlines the implementation plan for the "Forgot Password" view. This view allows users who have forgotten their password to initiate the reset process by submitting their email address. Upon submission, the system will send a password reset link to the provided email. For security, the view will display a generic confirmation message regardless of whether the email exists in the database, preventing user enumeration.

## 2. View Routing

The Forgot Password view will be accessible at the following application path:

- **Path:** `/forgot-password`

## 3. Component Structure

The view will be composed of a main page component (`ForgotPasswordPage`) which orchestrates several smaller, reusable components.

```
ForgotPasswordPage (Astro Page)
└── ForgotPasswordForm (React Component)
    ├── Card
    │   ├── CardHeader
    │   │   ├── CardTitle
    │   │   └── CardDescription
    │   ├── CardContent
    │   │   └── Form
    │   │       ├── FormField (for email)
    │   │       │   ├── FormLabel
    │   │       │   ├── FormControl
    │   │       │   │   └── Input
    │   │       │   └── FormMessage
    │   │       └── Button (Submit)
    │   └── CardFooter
    └── ConfirmationMessage (Conditional)
```

## 4. Component Details

### `ForgotPasswordPage.astro`

- **Component Description:** This is the main Astro page for the view. It sets up the overall page layout and renders the interactive `ForgotPasswordForm` React component as a client-side island.
- **Main Elements:**
  - `Layout`: The main site layout component.
  - `ForgotPasswordForm`: The React component handling the form logic, rendered with `client:load`.
- **Props:** None.

### `ForgotPasswordForm.tsx`

- **Component Description:** A client-side React component that manages the state and logic for the forgot password form. It handles user input, form submission, validation, and displays a confirmation message upon success.
- **Main Elements:**
  - `Card`, `CardHeader`, `CardTitle`, `CardDescription`, `CardContent`, `CardFooter` (from Shadcn/ui): To structure the form visually.
  - `Form`, `FormField`, `FormLabel`, `FormControl`, `FormMessage` (from Shadcn/ui & `react-hook-form`): To build the form and manage validation.
  - `Input` (from Shadcn/ui): For the email address field.
  - `Button` (from Shadcn/ui): To submit the form.
  - A custom `ConfirmationMessage` component or JSX block to show after successful submission.
- **Handled Interactions:**
  - `onChange` on the `Input` field to update the form state.
  - `onSubmit` on the `Form` to trigger the password reset process.
- **Handled Validation:**
  - **Email:**
    - Must not be empty.
    - Must be a valid email format (e.g., `user@example.com`).
- **Types:**
  - `ForgotPasswordFormViewModel`
  - `ForgotPasswordRequest`
- **Props:** None.

## 5. Types

### `ForgotPasswordFormViewModel`

This ViewModel represents the shape of the data managed by the `react-hook-form` instance.

```typescript
// File: src/types/viewModels.ts
export interface ForgotPasswordFormViewModel {
  email: string;
}
```

### `ForgotPasswordRequest`

This DTO type matches the request body expected by the `/api/auth/forgot-password` endpoint.

```typescript
// File: src/types/auth.ts (or a shared API types file)
export interface ForgotPasswordRequest {
  email: string;
}
```

## 6. State Management

State will be managed locally within the `ForgotPasswordForm.tsx` component using React hooks.

- **Form State:** Managed by the `react-hook-form` library. This will handle field values, validation errors, and submission state.
- **Submission State:** A `useState` hook will manage the overall status of the API request (`'idle'`, `'loading'`, `'success'`, `'error'`).
  - `const [formStatus, setFormStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');`
- **Success State:** A boolean state, `isSubmitted`, will be used to conditionally render the confirmation message instead of the form.
  - `const [isSubmitted, setIsSubmitted] = useState<boolean>(false);`

No custom hook is required for this view, as the logic is self-contained and simple.

## 7. API Integration

The form will interact with the backend by sending a request to the "Forgot Password" endpoint.

- **Endpoint:** `POST /api/auth/forgot-password`
- **Request Type:** `ForgotPasswordRequest`
  ```json
  {
    "email": "user@example.com"
  }
  ```
- **Response Type (Success):** The API returns a simple message object, but the frontend will not use its content.
  ```json
  {
    "message": "Password reset link sent to your email"
  }
  ```
- **Implementation:** An asynchronous function (`onSubmit`) will be called when the form is submitted. This function will use a service layer (e.g., `api.ts`) to make the `POST` request. The `fetch` API or a library like `axios` will be used.

## 8. User Interactions

1.  **User Enters Email:** The user types their email into the `Input` field. The form state is updated.
2.  **User Submits Form:** The user clicks the "Send Reset Link" button.
    - If the form is invalid, validation messages appear, and the submission is blocked.
    - If the form is valid, the `onSubmit` handler is triggered.
3.  **API Call:** The application sends the request to the backend. The submit button is disabled and may show a loading indicator.
4.  **Response Handling:**
    - **On Success (200 OK):** The form is replaced with a confirmation message: "If an account with that email exists, a password reset link has been sent. Please check your inbox." The `isSubmitted` state is set to `true`.
    - **On Client/Server Error (e.g., 400 Bad Request, 500 Internal Server Error):** A toast notification or an inline error message is displayed to the user, indicating that the request failed and they should try again. The form remains visible.

## 9. Conditions and Validation

- **Email Validation:** The `react-hook-form` will be configured with a validation schema (e.g., using `zod`).
  - The schema will enforce that the `email` field is a non-empty string and matches a valid email pattern.
  - Validation messages will be displayed below the input field via the `FormMessage` component if the conditions are not met.
- **Button State:** The submit `Button` will be disabled while the API request is in progress (`formStatus === 'loading'`) to prevent multiple submissions.

## 10. Error Handling

- **Invalid Input (Client-Side):** Handled by `react-hook-form` and `zod`. Error messages are displayed inline.
- **API Errors (Server-Side):**
  - **400 Bad Request:** This indicates an invalid email format that may have bypassed client-side validation. Display a generic error message (e.g., "An error occurred. Please try again.").
  - **5xx Server Errors:** Display a generic error message informing the user that something went wrong on the server and they should try again later.
- **Network Errors:** A `try...catch` block around the API call will handle network failures, displaying a generic error message.
- **Error Display Mechanism:** A toast notification system (like `react-hot-toast` or one integrated with Shadcn/ui) is recommended for non-validation errors to provide unobtrusive feedback.

## 11. Implementation Steps

1.  **Create File Structure:**
    - Create the Astro page file: `src/pages/forgot-password.astro`.
    - Create the React component file: `src/components/forms/ForgotPasswordForm.tsx`.
2.  **Define Types:**
    - Add the `ForgotPasswordFormViewModel` to a relevant view model type definition file.
    - Ensure the `ForgotPasswordRequest` DTO is defined in `src/types/auth.ts`.
3.  **Build the Astro Page (`ForgotPasswordPage.astro`):**
    - Import and use the main `Layout` component.
    - Import and render the `ForgotPasswordForm` component, ensuring it is loaded as a client-side island (`client:load`).
4.  **Build the React Component (`ForgotPasswordForm.tsx`):**
    - Set up the form using `react-hook-form` and a `zod` schema for validation.
    - Construct the UI using Shadcn/ui components (`Card`, `Form`, `Input`, `Button`).
    - Implement the `onSubmit` handler to call the API service.
    - Manage loading and success states using `useState`.
    - Conditionally render the form or the success confirmation message based on the `isSubmitted` state.
5.  **Implement API Call:**
    - In the API service file (`src/services/api.ts`), create a function `forgotPassword(data: ForgotPasswordRequest)` that performs the `POST` request to `/api/auth/forgot-password`.
6.  **Add Routing:**
    - Astro's file-based routing will automatically handle the `/forgot-password` route. No further configuration is needed.
7.  **Implement Error Handling:**
    - Add `try...catch` blocks for API calls.
    - Integrate a toast notification library to display feedback for server/network errors.
8.  **Testing:**
    - Manually test the form with valid and invalid email addresses.
    - Verify that the confirmation message appears on successful submission.
    - Verify that error messages and toasts appear correctly for failed submissions.
    - Check that the submit button is disabled during the request.
