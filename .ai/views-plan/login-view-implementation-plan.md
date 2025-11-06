# View Implementation Plan: Login

## 1. Overview
This document outlines the implementation plan for the Login view. The purpose of this view is to provide a secure form for registered users to authenticate and access the application. The view will consist of email and password fields, a submission button, and links to the registration and password recovery pages. It will handle user input, perform client-side validation, communicate with the authentication API, and manage feedback for success and error states.

## 2. View Routing
The Login view will be accessible at the following application path:
-   **Path:** `/login`

This will be a publicly accessible route. Upon successful login, the user will be redirected to the main dashboard (`/`).

## 3. Component Structure
The view will be composed of a main Astro page that hosts a client-side rendered React component. The component hierarchy is as follows:

```
/src/pages/login.astro
└── /src/components/views/LoginView.tsx (client:load)
    ├── Card (shadcn/ui)
    │   ├── CardHeader
    │   │   ├── CardTitle
    │   │   └── CardDescription
    │   ├── CardContent
    │   │   └── LoginForm.tsx
    │   │       ├── Form (shadcn/ui)
    │   │       │   ├── Input (for email)
    │   │       │   ├── Input (for password)
    │   │       │   └── Button (for submission)
    │   │       └── Toast (shadcn/ui for notifications)
    │   └── CardFooter
    │       └── Links to /register and /forgot-password
    └── Toaster (shadcn/ui)
```

## 4. Component Details

### `login.astro`
-   **Component Description:** The Astro page that serves as the entry point for the `/login` route. It sets up the page layout and renders the main React component with a `client:load` directive to make it immediately interactive.
-   **Main Elements:** `<Layout>`, `<LoginView />`.
-   **Props:** None.

### `LoginView.tsx`
-   **Component Description:** A client-side React component that contains the entire UI for the login page. It manages the overall layout using `Card` components from `shadcn/ui` and includes the `LoginForm`.
-   **Main Elements:** `<Card>`, `<CardHeader>`, `<CardContent>`, `<CardFooter>`, `<LoginForm />`.
-   **Handled Interactions:** None directly; it delegates interactions to child components.
-   **Props:** None.

### `LoginForm.tsx`
-   **Component Description:** The core component responsible for rendering the login form, handling user input, validation, and API submission. It will be built using `react-hook-form` and `zod` for robust validation integrated with `shadcn/ui` `Form` components.
-   **Main Elements:** `<Form>`, `<FormField>`, `<FormLabel>`, `<FormControl>`, `<FormMessage>`, `<Input type="email">`, `<Input type="password">`, `<Button type="submit">`.
-   **Handled Interactions:**
    -   Form input changes for email and password fields.
    -   Form submission via the "Log In" button click or Enter key press.
-   **Handled Validation:**
    -   **Email:**
        -   Must not be empty.
        -   Must be a valid email format (e.g., `user@example.com`).
        -   Maximum length of 255 characters.
    -   **Password:**
        -   Must not be empty.
-   **Types:** `LoginRequest`, `LoginResponse`, `LoginViewModel`.
-   **Props:** None.

## 5. Types
The following types are required for the implementation of the Login view.

### DTOs (Data Transfer Objects)
These types map directly to the API request and response structures.

-   **`LoginRequest`** (for API call)
    ```typescript
    interface LoginRequest {
      email: string;
      password: string;
    }
    ```

-   **`LoginResponse`** (from API call)
    ```typescript
    interface LoginResponse {
      accessToken: string;
      refreshToken: string;
      expiresIn: number;
      user: {
        id: string;
        userName: string;
        email: string;
      };
    }
    ```

### ViewModel
This type is used for form state management and validation within the client.

-   **`LoginViewModel`** (for `react-hook-form` and `zod` validation)
    ```typescript
    import { z } from "zod";

    const LoginViewModel = z.object({
      email: z.string()
        .min(1, { message: "Email is required" })
        .max(255, { message: "Email cannot exceed 255 characters" })
        .email({ message: "Invalid email format" }),
      password: z.string().min(1, { message: "Password is required" }),
    });

    type LoginViewModel = z.infer<typeof LoginViewModel>;
    ```

## 6. State Management
State will be managed locally within the `LoginForm.tsx` component using React hooks. A custom hook is not necessary for this view's complexity.

-   **Form State:** Managed by `react-hook-form`'s `useForm` hook. This will handle field values, validation errors, and submission state.
-   **Loading State:** A `useState` hook (`const [isSubmitting, setIsSubmitting] = useState(false);`) will track the form's submission status. This will be used to disable the submit button and show a loading indicator to prevent multiple submissions.
-   **Error State:** API errors (e.g., invalid credentials) will be caught in the submission handler. The error message will be displayed to the user via the `shadcn/ui` `Toast` component, triggered by calling `toast()`.

## 7. API Integration
The view will integrate with the backend authentication endpoint.

-   **Endpoint:** `POST /api/auth/login`
-   **Request:**
    -   The `onSubmit` function in `LoginForm.tsx` will construct a `LoginRequest` object from the validated form data.
    -   A `POST` request will be sent to the endpoint with the `LoginRequest` object in the body.
-   **Response:**
    -   **On Success (200 OK):** The `LoginResponse` data (`accessToken`, `refreshToken`, `user`) will be received. The tokens and user information should be stored securely (e.g., in a secure, HTTP-only cookie or local storage). The user will then be redirected to the dashboard (`/`).
    -   **On Error (401 Unauthorized):** An error message will be displayed using a `Toast` notification, informing the user of "Invalid credentials."

## 8. User Interactions
-   **Typing in fields:** The `Input` components update the form state managed by `react-hook-form`. Validation messages appear as the user types or on blur, depending on the `mode` configuration of `useForm`.
-   **Clicking "Log In":**
    -   Triggers the form submission process.
    -   The button becomes disabled, and a loading spinner appears.
    -   If validation fails, error messages are displayed below the respective fields, and the submission is blocked.
    -   If validation succeeds, the API call is made.
-   **Clicking "Sign up":** Navigates the user to the `/register` page.
-   **Clicking "Forgot password?":** Navigates the user to the `/forgot-password` page.

## 9. Conditions and Validation
-   **Form Submission:** The "Log In" button is disabled if the form is currently being submitted (`isSubmitting === true`).
-   **Email Validation:** The `email` field in `LoginForm.tsx` is validated against the `LoginViewModel` schema to ensure it is a non-empty, valid email format and within the character limit.
-   **Password Validation:** The `password` field is validated to ensure it is not empty.
-   **Feedback:** Real-time validation feedback is provided to the user via `FormMessage` components associated with each form field.

## 10. Error Handling
-   **Invalid Credentials (`401 Unauthorized`):** A `Toast` notification with a `destructive` variant will appear, displaying a message like "Invalid email or password. Please try again."
-   **Server/Network Errors (`500` or network failure):** A generic error `Toast` will be shown, e.g., "An unexpected error occurred. Please try again later."
-   **Validation Errors:** Handled by `react-hook-form` and displayed inline under each invalid field. The form submission is prevented until all errors are resolved.

## 11. Implementation Steps
1.  **Create File Structure:**
    -   Create `src/pages/login.astro`.
    -   Create `src/components/views/LoginView.tsx`.
    -   Create `src/components/forms/LoginForm.tsx`.
2.  **Set up `login.astro`:**
    -   Import and use the main `Layout`.
    -   Import and render `<LoginView client:load />` within the layout.
3.  **Implement `LoginView.tsx`:**
    -   Build the static layout using `Card`, `CardHeader`, `CardContent`, and `CardFooter` from `shadcn/ui`.
    -   Add the title "Login" and description "Enter your credentials to access your account."
    -   Place the `<LoginForm />` component inside `CardContent`.
    -   Add links to `/register` and `/forgot-password` in the `CardFooter`.
4.  **Define Types and Validation:**
    -   Create a `types/auth.ts` file (if not existing) and define the `LoginRequest` and `LoginResponse` interfaces.
    -   In `LoginForm.tsx`, define the `LoginViewModel` using `zod` for validation.
5.  **Implement `LoginForm.tsx`:**
    -   Set up the form using `useForm` from `react-hook-form` with the `zodResolver`.
    -   Create the form structure using `shadcn/ui`'s `Form` components, binding them to the `useForm` instance.
    -   Add `Input` fields for `email` and `password`.
    -   Add a `Button` of `type="submit"` and manage its disabled and loading state based on `isSubmitting`.
6.  **Implement Form Submission Logic:**
    -   Create an `onSubmit` async function that will be passed to the form.
    -   Inside `onSubmit`, set `isSubmitting` to `true`.
    -   Call the API service with the form data.
    -   Use a `try...catch` block for error handling.
7.  **Handle API Response:**
    -   On success, save the authentication tokens and user data, then redirect the user to the dashboard.
    -   On failure, use `toast()` from `sonner` (or `react-hot-toast`) to display the appropriate error message.
    -   In a `finally` block, set `isSubmitting` back to `false`.
8.  **Add Toaster:**
    -   Ensure the `Toaster` component from `shadcn/ui` is present in the root layout (`Layout.astro`) to display notifications.
9.  **Testing:**
    -   Manually test all validation rules.
    -   Test the successful login flow and redirection.
    -   Test the error handling for invalid credentials and server errors.
    -   Verify all links work as expected.
    -   Check for accessibility, ensuring all form fields have labels.
