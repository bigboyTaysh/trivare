export function LoadingSpinner() {
  return (
    <div data-testid="loading-container" className="flex min-h-screen items-center justify-center bg-background">
      <div data-testid="loading-content" className="flex flex-col items-center space-y-4">
        <div
          data-testid="loading-spinner"
          className="h-12 w-12 animate-spin rounded-full border-4 border-primary border-t-transparent"
        ></div>
        <p className="text-sm text-muted-foreground">Loading...</p>
      </div>
    </div>
  );
}
