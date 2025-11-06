import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from "@/components/ui/card";
import { RegisterForm } from "@/components/forms/RegisterForm";

export function RegisterView() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4 py-8">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Create an account</CardTitle>
          <CardDescription>Enter your information to get started.</CardDescription>
        </CardHeader>
        <CardContent>
          <RegisterForm />
        </CardContent>
        <CardFooter className="flex flex-col space-y-2 text-sm text-muted-foreground">
          <div>
            Already have an account?{" "}
            <a href="/login" className="font-medium text-primary hover:underline">
              Log in
            </a>
          </div>
        </CardFooter>
      </Card>
    </div>
  );
}
