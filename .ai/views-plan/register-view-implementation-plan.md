# View Implementation Plan: Register

## 1. Overview

This document outlines the implementation plan for the user registration view. The primary purpose of this view is to allow new users to create an account by providing a username, email address, and password. The view will feature client-side validation, provide clear feedback on user input, and integrate with the backend registration endpoint.

## 2. View Routing

The Register view will be accessible at the following application path:

- **Path:** `/register`

This will be implemented as a new Astro page at `Client/src/pages/register.astro`.

## 3. Component Structure

The view will be composed of a static Astro page that loads a client-side rendered React form component. The component hierarchy is as follows:

```
RegisterPage.astro
└── Layout.astro
    └── RegisterForm.tsx (client:load)
        └── Card (shadcn/ui)
            ├── CardHeader
            ├── CardContent
            │   └── Form (react-hook-form)
            │       ├── Input (for Username)
            │       ├── Input (for Email)
            │       ├── Input (for Password)
            │       └── Button (submit)
            └── CardFooter
                └── Link (to /login)
```

## 4. Component Details

### `RegisterPage.astro`

- **Component Description:** The main entry point for the `/register` route. It sets up the page layout and renders the interactive `RegisterForm` component.
- **Main Elements:** It will use the main `Layout.astro` and will contain a `<main>` tag that centers the `RegisterForm` component on the page.
- **Handled Interactions:** None (static page).
- **Handled Validation:** None.
- **Types:** None.
- **Props:** None.

### `RegisterForm.tsx`

- **Component Description:** A client-side React component that manages the entire registration process, including form state, validation, API submission, and user feedback via toasts.
- **Main Elements:**
  - `Card`, `CardHeader`, `CardContent`, `CardFooter` for structure.
  - A `<form>` element managed by `react-hook-form`.
  - Three `FormField` components (from `shadcn/ui`), each containing a labeled `Input` for `username`, `email`, and `password`.
  - A `Button` component with `type="submit"` to trigger form submission.
  - A link to the login page (`/login`) in the `CardFooter`.
- **Handled Interactions:**
  - `onChange`: Updates form state for each input field.
  - `onSubmit`: Triggers validation and the API registration call.
- **Handled Validation:** Client-side validation will be implemented using `zod` and `react-hook-form` to mirror the backend rules. See the "Conditions and Validation" section for details.
- **Types:** `RegisterRequest`, `RegisterResponse`, `RegisterFormViewModel`.
- **Props:** None.

## 5. Types

### DTOs (Data Transfer Objects)

These types define the contract with the API.

```typescript
// DTO for the registration request body
export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

// DTO for the successful registration response
export interface RegisterResponse {
  id: string; // from Guid
  userName: string;
  email: string;
  createdAt: string; // from DateTime
}
```

### ViewModels

This type defines the shape of the form's data within the frontend component.

```typescript
// ViewModel for the registration form state
// In this case, it's identical to the Request DTO
export type RegisterFormViewModel = RegisterRequest;
```

## 6. State Management

State will be managed locally within the `RegisterForm.tsx` component.

- **Form State:** `react-hook-form` will be used to manage the form's state, including input values, validation errors, and submission status (`isSubmitting`).
- **Validation Schema:** A `zod` schema will be created to define the validation rules for the form, ensuring consistency with the backend API.
- **API State:** For handling the API request, a library like `TanStack Query` (`react-query`) is recommended. A `useMutation` hook will be used to handle the API call's loading, error, and success states, abstracting the logic from the component.

## 7. API Integration

- **Endpoint:** `POST /api/auth/register`
- **Integration Logic:**

  1.  The `onSubmit` function of the `RegisterForm` component will trigger the mutation.
  2.  The mutation will send a `POST` request to `/api/auth/register` using `fetch` or a configured API client (e.g., Axios).
  3.  The request body will be a JSON-serialized object of type `RegisterRequest`.
  4.  The response will be handled based on the HTTP status code.

- **Request Type:** `RegisterRequest`
- **Response Type (on success):** `RegisterResponse`

## 8. User Interactions

- **Typing in fields:** Form state is updated, and inline validation feedback is provided upon losing focus (`onBlur`).
- **Submitting the form (valid data):**
  1.  The "Create account" button is clicked.
  2.  The button enters a disabled/loading state.
  3.  An API request is sent.
  4.  On success: A success toast is displayed, and the user is redirected to `/login`.
  5.  On failure: An error toast or field-specific error is displayed, and the button is re-enabled.
- **Submitting the form (invalid data):**
  1.  Submission is prevented.
  2.  Validation error messages appear below the corresponding input fields.
- **Clicking the "Login" link:** The user is navigated to the `/login` page.

## 9. Conditions and Validation

The following validation rules, derived from the `RegisterRequest.cs` DTO, will be implemented in the `zod` schema:

- **Username (`userName`):**
  - Required field.
  - Must be between 3 and 50 characters.
  - Must contain only letters, numbers, underscores (`_`), and hyphens (`-`).
- **Email (`email`):**
  - Required field.
  - Must be a valid email format.
  - Maximum length of 255 characters.
- **Password (`password`):**
  - Required field.
  - Minimum 8 characters.
  - Must contain at least one lowercase letter, one uppercase letter, one number, and one special character (`@$!%*?&`).

## 10. Error Handling

- **Client-Side Validation Errors:** Handled by `react-hook-form` and displayed as inline error messages.
- **409 Conflict (Email/Username exists):** The API response will be caught. A specific error message will be set on the corresponding form field (e.g., "This email is already taken").
- **400 Bad Request (Server validation failed):** A generic error toast will be displayed (e.g., "Invalid data provided."). If the API response includes field-specific errors, they will be mapped back to the form fields.
- **500 Internal Server Error:** A generic error toast will be shown (e.g., "An unexpected error occurred. Please try again later.").
- **Network Errors:** A toast will inform the user to check their internet connection.

## 11. Implementation Steps

1.  **Create Astro Page:** Create the file `Client/src/pages/register.astro`. Set up the main layout and include the yet-to-be-created React component with a `client:load` directive.
2.  **Create React Form Component:** Create the file `Client/src/components/auth/RegisterForm.tsx`.
3.  **Define Types:** In a new file, e.g., `Client/src/types/auth.ts`, define the `RegisterRequest` and `RegisterResponse` interfaces.
4.  **Build Form UI:** Inside `RegisterForm.tsx`, use `shadcn/ui` components (`Card`, `Input`, `Button`, `Form`) to build the visual structure of the form.
5.  **Implement Validation:** Create a `zod` schema that enforces all the validation rules specified in section 9.
6.  **Set up Form State:** Integrate `react-hook-form` with the `zod` schema using `@hookform/resolvers/zod`. Hook up the form fields to the form controller.
7.  **Implement API Call:** Create a function or a custom `useMutation` hook to handle the `POST` request to `/api/auth/register`.
8.  **Handle Submission Logic:** Implement the `onSubmit` handler. It should call the API mutation and handle the `onSuccess` and `onError` callbacks.
9.  **Implement User Feedback:**
    - Use a toast library (e.g., `sonner`) to display success and error messages.
    - On successful registration, programmatically navigate the user to the `/login` page.
    - On API error (e.g., 409), use `setError` from `react-hook-form` to display a specific message on the correct field.
10. **Add Link to Login:** Ensure a clear link to the `/login` page is present in the form's footer.
