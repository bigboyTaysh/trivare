import type { UserDto } from "@/types/user";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { User, Home, UserCircle, LogOut } from "lucide-react";
import { clearAuthData } from "@/lib/auth";
import { ThemeToggle } from "@/components/common/ThemeToggle";

interface ProfileDropdownProps {
  user: UserDto | null;
}

export function ProfileDropdown({ user }: ProfileDropdownProps) {
  const handleLogout = () => {
    clearAuthData();
    window.location.href = "/login";
  };

  if (!user) {
    return null;
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="relative h-10 w-10" data-testid="user-menu">
          <User className="h-5 w-5" />
          <span className="sr-only">Open user menu</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <DropdownMenuLabel className="font-normal">
          <div className="flex flex-col space-y-1">
            <p className="text-sm font-medium leading-none">{user.userName}</p>
            <p className="text-xs leading-none text-muted-foreground">{user.email}</p>
          </div>
        </DropdownMenuLabel>
        <DropdownMenuSeparator />
        <DropdownMenuItem asChild>
          <a href="/" className="flex items-center cursor-pointer">
            <Home className="mr-2 h-4 w-4" />
            <span>Dashboard</span>
          </a>
        </DropdownMenuItem>
        <DropdownMenuItem asChild>
          <a href="/profile" className="flex items-center cursor-pointer">
            <UserCircle className="mr-2 h-4 w-4" />
            <span>Profile</span>
          </a>
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <div className="px-2 py-1.5">
          <div className="flex items-center justify-between">
            <span className="text-sm font-medium">Theme</span>
            <ThemeToggle />
          </div>
        </div>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleLogout} className="cursor-pointer text-red-600 focus:text-red-600">
          <LogOut className="mr-2 h-4 w-4" />
          <span>Log out</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
