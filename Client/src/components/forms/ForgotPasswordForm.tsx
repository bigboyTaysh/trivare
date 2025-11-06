// File: src/components/forms/ForgotPasswordForm.tsx
import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { toast } from "sonner";
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { forgotPassword } from "@/services/api";
import type { ForgotPasswordFormViewModel } from "@/types/auth";

const forgotPasswordSchema = z.object({
  email: z.string().email("Please enter a valid email address").min(1, "Email is required"),
});

export default function ForgotPasswordForm() {
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [formStatus, setFormStatus] = useState<"idle" | "loading" | "success" | "error">("idle");

  const form = useForm<ForgotPasswordFormViewModel>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: "",
    },
  });

  const onSubmit = async (data: ForgotPasswordFormViewModel) => {
    setFormStatus("loading");
    try {
      await forgotPassword({ email: data.email });
      setFormStatus("success");
      setIsSubmitted(true);
    } catch {
      setFormStatus("error");
      toast.error("An error occurred. Please try again.");
    }
  };

  if (isSubmitted) {
    return (
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Check Your Email</CardTitle>
          <CardDescription>
            If an account with that email exists, a password reset link has been sent. Please check your inbox.
          </CardDescription>
        </CardHeader>
      </Card>
    );
  }

  return (
    <Card className="w-full max-w-md">
      <CardHeader>
        <CardTitle>Forgot Password</CardTitle>
        <CardDescription>
          Enter your email address and we&apos;ll send you a link to reset your password.
        </CardDescription>
      </CardHeader>
      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <CardContent>
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input placeholder="Enter your email" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </CardContent>
          <CardFooter className="mt-4">
            <Button type="submit" disabled={formStatus === "loading"} className="w-full">
              {formStatus === "loading" ? "Sending..." : "Send Reset Link"}
            </Button>
          </CardFooter>
        </form>
      </Form>
    </Card>
  );
}
