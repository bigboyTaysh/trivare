import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { LoginForm } from "@/components/forms/LoginForm";

export function LoginView() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Login</CardTitle>
          <CardDescription>Enter your credentials to access your account.</CardDescription>
        </CardHeader>
        <CardContent>
          <LoginForm />
        </CardContent>
        <CardFooter className="flex flex-col space-y-2 text-sm text-muted-foreground">
          <div>
            Don&apos;t have an account?{" "}
            <a href="/register" className="font-medium text-primary hover:underline">
              Sign up
            </a>
          </div>
          <div>
            <a href="/forgot-password" className="font-medium text-primary hover:underline">
              Forgot password?
            </a>
          </div>
        </CardFooter>
      </Card>
    </div>
  );
}
