import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { LoginViewModel, type LoginRequest, type LoginResponse } from "@/types/auth";
import { API_BASE_URL } from "@/config/api";

export function LoginForm() {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<LoginViewModel>({
    resolver: zodResolver(LoginViewModel),
    defaultValues: {
      email: "",
      password: "",
    },
    mode: "onBlur",
  });

  const onSubmit = async (data: LoginViewModel) => {
    setIsSubmitting(true);

    try {
      const loginRequest: LoginRequest = {
        email: data.email,
        password: data.password,
      };

      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(loginRequest),
      });

      if (!response.ok) {
        if (response.status === 401) {
          toast.error("Invalid email or password. Please try again.");
        } else {
          toast.error("An unexpected error occurred. Please try again later.");
        }
        return;
      }

      const loginResponse: LoginResponse = await response.json();

      // Store authentication data
      localStorage.setItem("accessToken", loginResponse.accessToken);
      localStorage.setItem("refreshToken", loginResponse.refreshToken);
      localStorage.setItem("user", JSON.stringify(loginResponse.user));

      // Notify auth hooks of the change
      const { notifyAuthChange } = await import("@/hooks/useAuth");
      notifyAuthChange();

      toast.success("Login successful! Redirecting...");

      // Redirect to the stored path or dashboard
      setTimeout(() => {
        const redirectPath = sessionStorage.getItem("redirectAfterLogin") || "/";
        sessionStorage.removeItem("redirectAfterLogin");
        window.location.href = redirectPath;
      }, 1000);
    } catch (error) {
      // eslint-disable-next-line no-console
      console.error("Login error:", error);
      toast.error("An unexpected error occurred. Please try again later.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Email</FormLabel>
              <FormControl>
                <Input type="email" placeholder="you@example.com" autoComplete="email" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Password</FormLabel>
              <FormControl>
                <Input type="password" placeholder="Enter your password" autoComplete="current-password" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting ? "Logging in..." : "Log In"}
        </Button>
      </form>
    </Form>
  );
}
