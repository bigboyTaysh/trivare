import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { UpdateUsernameViewModel } from "@/types/user";
import type { UserDto } from "@/types/user";

interface UpdateUsernameFormProps {
  user: UserDto;
  onUpdate: (data: UpdateUsernameViewModel) => Promise<void>;
  isUpdating: boolean;
}

export function UpdateUsernameForm({ user, onUpdate, isUpdating }: UpdateUsernameFormProps) {
  const form = useForm<UpdateUsernameViewModel>({
    resolver: zodResolver(UpdateUsernameViewModel),
    defaultValues: {
      userName: user.userName,
    },
    mode: "onBlur",
  });

  const onSubmit = async (data: UpdateUsernameViewModel) => {
    await onUpdate(data);
    form.reset({ userName: data.userName });
  };

  const isDirty = form.formState.isDirty;
  const isValid = form.formState.isValid;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Update Username</CardTitle>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="userName"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Username</FormLabel>
                  <FormControl>
                    <Input type="text" placeholder="Enter new username" autoComplete="username" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button type="submit" disabled={!isDirty || !isValid || isUpdating}>
              {isUpdating ? "Updating..." : "Update Username"}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
