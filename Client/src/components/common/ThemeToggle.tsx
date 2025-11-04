"use client";

import { Moon, Sun } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCallback, useState } from "react";

export function ThemeToggle() {
  const [isDark, setIsDark] = useState(() => {
    // Get initial theme from localStorage or default to dark
    const stored = localStorage.getItem("trivare-theme") || "dark";
    const darkMode = stored === "dark";

    // Apply theme to document on initialization
    if (darkMode) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }

    return darkMode;
  });

  const toggleTheme = useCallback(() => {
    const newIsDark = !isDark;
    setIsDark(newIsDark);

    // Update document class
    if (newIsDark) {
      document.documentElement.classList.add("dark");
    } else {
      document.documentElement.classList.remove("dark");
    }

    // Save to localStorage
    localStorage.setItem("trivare-theme", newIsDark ? "dark" : "light");

    console.log("Theme toggled to:", newIsDark ? "dark" : "light");
  }, [isDark]);

  return (
    <Button variant="ghost" size="icon" onClick={toggleTheme} className="relative h-10 w-10">
      <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
      <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
      <span className="sr-only">Toggle theme</span>
    </Button>
  );
}
