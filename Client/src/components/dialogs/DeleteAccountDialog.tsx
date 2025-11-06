import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { DeleteAccountViewModel } from "@/types/user";

interface DeleteAccountDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onDelete: (data: DeleteAccountViewModel) => Promise<void>;
  isDeleting: boolean;
}

export function DeleteAccountDialog({ isOpen, onClose, onDelete, isDeleting }: DeleteAccountDialogProps) {
  const form = useForm<DeleteAccountViewModel>({
    resolver: zodResolver(DeleteAccountViewModel),
    defaultValues: {
      password: "",
    },
    mode: "onBlur",
  });

  const handleSubmit = async (data: DeleteAccountViewModel) => {
    await onDelete(data);
    form.reset();
  };

  const handleClose = () => {
    form.reset();
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Account</DialogTitle>
          <DialogDescription>
            This action cannot be undone. This will permanently delete your account and remove all your data from our
            servers.
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Confirm your password</FormLabel>
                  <FormControl>
                    <Input
                      type="password"
                      placeholder="Enter your password"
                      autoComplete="current-password"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="button" variant="outline" onClick={handleClose} disabled={isDeleting}>
                Cancel
              </Button>
              <Button type="submit" variant="destructive" disabled={!form.formState.isValid || isDeleting}>
                {isDeleting ? "Deleting..." : "Delete Account"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
