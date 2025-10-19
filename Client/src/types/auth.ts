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
