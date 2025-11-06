import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

interface DeleteAccountSectionProps {
  onOpenDialog: () => void;
}

export function DeleteAccountSection({ onOpenDialog }: DeleteAccountSectionProps) {
  return (
    <Card className="border-destructive">
      <CardHeader>
        <CardTitle>Delete Account</CardTitle>
        <CardDescription>
          Permanently delete your account and all associated data. This action cannot be undone.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <p className="text-sm text-muted-foreground mb-4">
          Once you delete your account, all of your trips, files, and personal information will be permanently removed
          from our servers. You will not be able to recover this data.
        </p>
        <Button variant="destructive" onClick={onOpenDialog}>
          Delete Account
        </Button>
      </CardContent>
    </Card>
  );
}
