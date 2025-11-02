import { useState } from "react";
import type { UserDto } from "@/types/user";
import { Button } from "@/components/ui/button";
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from "@/components/ui/sheet";
import { Menu, Home, UserCircle, LogOut } from "lucide-react";
import { clearAuthData } from "@/lib/auth";
import { ThemeToggle } from "@/components/common/ThemeToggle";

interface MobileNavProps {
  user: UserDto | null;
}

export function MobileNav({ user }: MobileNavProps) {
  const [open, setOpen] = useState(false);

  const handleLogout = () => {
    clearAuthData();
    window.location.href = "/login";
  };

  const handleNavigation = (href: string) => {
    setOpen(false);
    window.location.href = href;
  };

  if (!user) {
    return null;
  }

  return (
    <Sheet open={open} onOpenChange={setOpen}>
      <SheetTrigger asChild>
        <Button variant="ghost" size="icon" className="min-[400px]:hidden">
          <Menu className="h-5 w-5" />
          <span className="sr-only">Toggle menu</span>
        </Button>
      </SheetTrigger>
      <SheetContent side="right" className="w-[200px]">
        <SheetHeader>
          <SheetTitle>Menu</SheetTitle>
          <SheetDescription>
            <div className="flex flex-col space-y-1 text-left">
              <p className="text-sm font-medium text-foreground">{user.userName}</p>
              <p className="text-xs text-muted-foreground">{user.email}</p>
            </div>
          </SheetDescription>
        </SheetHeader>
        <nav className="flex flex-col gap-2 mt-6">
          <Button variant="ghost" className="justify-start" onClick={() => handleNavigation("/")}>
            <Home className="mr-2 h-4 w-4" />
            Dashboard
          </Button>
          <Button variant="ghost" className="justify-start" onClick={() => handleNavigation("/profile")}>
            <UserCircle className="mr-2 h-4 w-4" />
            Profile
          </Button>
          <div className="my-2 border-t" />
          <div className="flex items-center justify-between px-3 py-2">
            <span className="text-sm font-medium">Theme</span>
            <ThemeToggle />
          </div>
          <div className="my-2 border-t" />
          <Button variant="ghost" className="justify-start text-red-600 hover:text-red-600" onClick={handleLogout}>
            <LogOut className="mr-2 h-4 w-4" />
            Log out
          </Button>
        </nav>
      </SheetContent>
    </Sheet>
  );
}
