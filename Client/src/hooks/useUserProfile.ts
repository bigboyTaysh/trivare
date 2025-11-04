import { useState, useEffect } from "react";
import { toast } from "sonner";
import { getMe, updateUser, deleteAccount } from "@/services/api";
import type { UserDto, UpdateUsernameViewModel, ChangePasswordViewModel, DeleteAccountViewModel } from "@/types/user";

interface UseUserProfileReturn {
  user: UserDto | null;
  isLoading: boolean;
  isUpdating: boolean;
  error: Error | null;
  fetchUser: () => Promise<void>;
  updateUsername: (data: UpdateUsernameViewModel) => Promise<void>;
  changePassword: (data: ChangePasswordViewModel) => Promise<void>;
  deleteUserAccount: (data: DeleteAccountViewModel) => Promise<void>;
}

export function useUserProfile(): UseUserProfileReturn {
  const [user, setUser] = useState<UserDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchUser = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const userData = await getMe();
      setUser(userData);
    } catch (err) {
      const error = err instanceof Error ? err : new Error("Failed to fetch user profile");
      setError(error);
      toast.error("Failed to load your profile. Please try again.");
    } finally {
      setIsLoading(false);
    }
  };

  const updateUsername = async (data: UpdateUsernameViewModel) => {
    try {
      setIsUpdating(true);
      setError(null);
      const updatedUser = await updateUser({ userName: data.userName });
      setUser(updatedUser);

      // Update user in localStorage
      if (typeof window !== "undefined") {
        const storedUser = localStorage.getItem("user");
        if (storedUser) {
          const parsedUser = JSON.parse(storedUser);
          localStorage.setItem("user", JSON.stringify({ ...parsedUser, userName: updatedUser.userName }));
        }
      }

      toast.success("Username updated successfully!");
    } catch (err) {
      const error = err instanceof Error ? err : new Error("Failed to update username");
      setError(error);
      toast.error("Failed to update username. Please try again.");
      throw error;
    } finally {
      setIsUpdating(false);
    }
  };

  const changePassword = async (data: ChangePasswordViewModel) => {
    try {
      setIsUpdating(true);
      setError(null);
      await updateUser({
        currentPassword: data.currentPassword,
        newPassword: data.newPassword,
      });
      toast.success("Password changed successfully!");
    } catch (err) {
      const error = err instanceof Error ? err : new Error("Failed to change password");
      setError(error);

      // Check if it's an incorrect password error
      if (error.message.includes("401") || error.message.toLowerCase().includes("unauthorized")) {
        toast.error("Current password is incorrect.");
      } else {
        toast.error("Failed to change password. Please try again.");
      }
      throw error;
    } finally {
      setIsUpdating(false);
    }
  };

  const deleteUserAccount = async (data: DeleteAccountViewModel) => {
    try {
      setIsUpdating(true);
      setError(null);
      await deleteAccount({ password: data.password });

      // Clear auth data and redirect to home
      if (typeof window !== "undefined") {
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("user");
        toast.success("Account deleted successfully.");

        // Redirect after a brief delay
        setTimeout(() => {
          window.location.href = "/";
        }, 1500);
      }
    } catch (err) {
      const error = err instanceof Error ? err : new Error("Failed to delete account");
      setError(error);

      // Check if it's an incorrect password error
      if (error.message.includes("401") || error.message.toLowerCase().includes("unauthorized")) {
        toast.error("Incorrect password. Please try again.");
      } else {
        toast.error("Failed to delete account. Please try again.");
      }
      throw error;
    } finally {
      setIsUpdating(false);
    }
  };

  // Fetch user on mount
  useEffect(() => {
    fetchUser();
  }, []);

  return {
    user,
    isLoading,
    isUpdating,
    error,
    fetchUser,
    updateUsername,
    changePassword,
    deleteUserAccount,
  };
}
