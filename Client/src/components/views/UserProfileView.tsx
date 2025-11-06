import { useState } from "react";
import { useUserProfile } from "@/hooks/useUserProfile";
import { UpdateUsernameForm } from "@/components/forms/UpdateUsernameForm";
import { ChangePasswordForm } from "@/components/forms/ChangePasswordForm";
import { DeleteAccountSection } from "@/components/features/DeleteAccountSection";
import { DeleteAccountDialog } from "@/components/dialogs/DeleteAccountDialog";

export function UserProfileView() {
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const { user, isLoading, isUpdating, updateUsername, changePassword, deleteUserAccount } = useUserProfile();

  if (isLoading) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="max-w-2xl mx-auto">
          <h1 className="text-3xl font-bold mb-8">Profile Settings</h1>
          <div className="space-y-6">
            <div className="h-48 bg-muted animate-pulse rounded-lg" />
            <div className="h-64 bg-muted animate-pulse rounded-lg" />
            <div className="h-32 bg-muted animate-pulse rounded-lg" />
          </div>
        </div>
      </div>
    );
  }

  if (!user) {
    return (
      <div className="container mx-auto py-8 px-4">
        <div className="max-w-2xl mx-auto">
          <h1 className="text-3xl font-bold mb-8">Profile Settings</h1>
          <div className="bg-destructive/10 border border-destructive text-destructive px-4 py-3 rounded">
            <p className="font-semibold">Failed to load profile</p>
            <p className="text-sm">There was an error loading your profile. Please try refreshing the page.</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-8 px-4">
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Profile Settings</h1>

        <div className="space-y-6">
          <UpdateUsernameForm user={user} onUpdate={updateUsername} isUpdating={isUpdating} />

          <ChangePasswordForm onSubmit={changePassword} isUpdating={isUpdating} />

          <DeleteAccountSection onOpenDialog={() => setIsDeleteDialogOpen(true)} />
        </div>

        <DeleteAccountDialog
          isOpen={isDeleteDialogOpen}
          onClose={() => setIsDeleteDialogOpen(false)}
          onDelete={deleteUserAccount}
          isDeleting={isUpdating}
        />
      </div>
    </div>
  );
}
