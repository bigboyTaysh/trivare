import { useState } from "react";
import { getCurrentUser } from "@/lib/auth";
import type { UserDto } from "@/types/user";
import { ProfileDropdown } from "./ProfileDropdown";
import { MobileNav } from "./MobileNav";

export function Nav() {
  const [user] = useState<UserDto | null>(() => getCurrentUser());

  return (
    <nav className="sticky top-0 z-40 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-6">
        {/* Logo and App Name */}
        <div className="flex items-center">
          <a href="/" className="flex items-center space-x-2 text-lg font-bold hover:opacity-80 transition-opacity">
            <span className="text-primary">Trivare</span>
          </a>
        </div>

        {/* Desktop Navigation */}
        {user && (
          <div className="flex items-center gap-4">
            <div className="hidden min-[400px]:block">
              <ProfileDropdown user={user} />
            </div>
            <div className="block min-[400px]:hidden">
              <MobileNav user={user} />
            </div>
          </div>
        )}
      </div>
    </nav>
  );
}
