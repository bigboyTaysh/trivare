import { Button } from "@/components/ui/button";

interface ErrorDisplayProps {
  message?: string;
  onRetry?: () => void;
}

/**
 * Error display component for showing error messages with optional retry action
 */
export function ErrorDisplay({ message, onRetry }: ErrorDisplayProps) {
  const defaultMessage = "Failed to load data. Please try again.";

  return (
    <div className="container mx-auto p-6">
      <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
        <div className="rounded-full bg-destructive/10 p-6 mb-4">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="48"
            height="48"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
            className="text-destructive"
          >
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
        </div>
        <h3 className="text-lg font-semibold mb-2">Something went wrong</h3>
        <p className="text-sm text-muted-foreground mb-6">{message || defaultMessage}</p>
        {onRetry && (
          <Button onClick={onRetry} variant="default">
            Try Again
          </Button>
        )}
        {!onRetry && (
          <Button onClick={() => window.location.reload()} variant="default">
            Refresh Page
          </Button>
        )}
      </div>
    </div>
  );
}
