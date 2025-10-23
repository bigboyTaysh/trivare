import { z } from "zod";

// API Request/Response types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: {
    id: string;
    userName: string;
    email: string;
  };
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  id: string;
  userName: string;
  email: string;
  createdAt: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}

export interface ForgotPasswordFormViewModel {
  email: string;
}

// ViewModel for form validation
export const LoginViewModel = z.object({
  email: z
    .string()
    .min(1, { message: "Email is required" })
    .max(255, { message: "Email cannot exceed 255 characters" })
    .email({ message: "Invalid email format" }),
  password: z.string().min(1, { message: "Password is required" }),
});

export type LoginViewModel = z.infer<typeof LoginViewModel>;

export const RegisterViewModel = z
  .object({
    userName: z
      .string()
      .min(3, { message: "Username must be at least 3 characters" })
      .max(50, { message: "Username must not exceed 50 characters" })
      .regex(/^[a-zA-Z0-9_-]+$/, { message: "Username can only contain letters, numbers, underscores and hyphens" }),
    email: z
      .string()
      .min(1, { message: "Email is required" })
      .max(255, { message: "Email must not exceed 255 characters" })
      .email({ message: "Invalid email format" }),
    password: z
      .string()
      .min(8, { message: "Password must be at least 8 characters" })
      .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]+$/, {
        message: "Password must contain uppercase, lowercase, number, and special character (@$!%*?&.)",
      }),
    confirmPassword: z.string().min(1, { message: "Please confirm your password" }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

export type RegisterViewModel = z.infer<typeof RegisterViewModel>;

export const ResetPasswordViewModel = z
  .object({
    newPassword: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]+$/, {
        message: "Password must contain uppercase, lowercase, number, and special character (@$!%*?&.)",
      }),
    confirmPassword: z.string().min(1, "Confirm password is required"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

export type ResetPasswordViewModel = z.infer<typeof ResetPasswordViewModel>;
