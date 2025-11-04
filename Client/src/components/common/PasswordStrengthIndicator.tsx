import { useMemo } from "react";
import { cn } from "@/lib/utils";

interface PasswordStrengthIndicatorProps {
  password: string;
}

interface PasswordRequirement {
  label: string;
  met: boolean;
}

export function PasswordStrengthIndicator({ password }: PasswordStrengthIndicatorProps) {
  const requirements = useMemo<PasswordRequirement[]>(() => {
    return [
      {
        label: "At least 8 characters",
        met: password.length >= 8,
      },
      {
        label: "Contains lowercase letter",
        met: /[a-z]/.test(password),
      },
      {
        label: "Contains uppercase letter",
        met: /[A-Z]/.test(password),
      },
      {
        label: "Contains number",
        met: /\d/.test(password),
      },
      {
        label: "Contains special character (@$!%*?&.)",
        met: /[@$!%*?&.]/.test(password),
      },
    ];
  }, [password]);

  const strength = useMemo(() => {
    const metCount = requirements.filter((req) => req.met).length;
    if (metCount === 0) return { level: 0, label: "", color: "" };
    if (metCount <= 2) return { level: 1, label: "Weak", color: "bg-red-500" };
    if (metCount <= 3) return { level: 2, label: "Fair", color: "bg-orange-500" };
    if (metCount <= 4) return { level: 3, label: "Good", color: "bg-yellow-500" };
    return { level: 4, label: "Strong", color: "bg-green-500" };
  }, [requirements]);

  if (!password) return null;

  return (
    <div className="space-y-2">
      {/* Strength bar */}
      <div className="flex gap-1">
        {[1, 2, 3, 4].map((level) => (
          <div
            key={level}
            className={cn(
              "h-1 flex-1 rounded-full transition-colors",
              level <= strength.level ? strength.color : "bg-muted"
            )}
          />
        ))}
      </div>

      {/* Strength label */}
      {strength.level > 0 && (
        <p className="text-xs text-muted-foreground">
          Password strength: <span className="font-medium">{strength.label}</span>
        </p>
      )}

      {/* Requirements checklist */}
      <div className="space-y-1">
        {requirements.map((requirement, index) => (
          <div key={index} className="flex items-center gap-2 text-xs">
            <div
              className={cn(
                "flex h-4 w-4 items-center justify-center rounded-full",
                requirement.met ? "bg-green-500 text-white" : "bg-muted text-muted-foreground"
              )}
            >
              {requirement.met ? (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 20 20" fill="currentColor" className="h-3 w-3">
                  <path
                    fillRule="evenodd"
                    d="M16.704 4.153a.75.75 0 01.143 1.052l-8 10.5a.75.75 0 01-1.127.075l-4.5-4.5a.75.75 0 011.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 011.05-.143z"
                    clipRule="evenodd"
                  />
                </svg>
              ) : (
                <span className="text-xs">•</span>
              )}
            </div>
            <span className={cn(requirement.met ? "text-foreground" : "text-muted-foreground")}>
              {requirement.label}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
