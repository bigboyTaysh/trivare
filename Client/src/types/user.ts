import { z } from "zod";

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
 */
export interface DeleteAccountRequest {
  password: string;
}

// --- ViewModels (Frontend-specific) ---

/**
 * ViewModel for the Update Username form.
 */
export const UpdateUsernameViewModel = z.object({
  userName: z
    .string()
    .min(3, { message: "Username must be at least 3 characters" })
    .max(50, { message: "Username must not exceed 50 characters" })
    .regex(/^[a-zA-Z0-9_-]+$/, { message: "Username can only contain letters, numbers, underscores and hyphens" }),
});

export type UpdateUsernameViewModel = z.infer<typeof UpdateUsernameViewModel>;

/**
 * ViewModel for the Change Password form, including confirmation field.
 */
export const ChangePasswordViewModel = z
  .object({
    currentPassword: z.string().min(8, { message: "Current password must be at least 8 characters" }),
    newPassword: z
      .string()
      .min(8, { message: "New password must be at least 8 characters" })
      .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$/, {
        message: "Password must contain uppercase, lowercase, number, and special character (@$!%*?&)",
      }),
    confirmNewPassword: z.string().min(1, { message: "Please confirm your new password" }),
  })
  .refine((data) => data.newPassword === data.confirmNewPassword, {
    message: "Passwords don't match",
    path: ["confirmNewPassword"],
  });

export type ChangePasswordViewModel = z.infer<typeof ChangePasswordViewModel>;

/**
 * ViewModel for the Delete Account confirmation dialog.
 */
export const DeleteAccountViewModel = z.object({
  password: z.string().min(1, { message: "Password is required" }),
});

export type DeleteAccountViewModel = z.infer<typeof DeleteAccountViewModel>;
