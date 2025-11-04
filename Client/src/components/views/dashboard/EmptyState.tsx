interface EmptyStateProps {
  category: "Ongoing" | "Past";
}

/**
 * Empty state component shown when a trip category has no trips
 */
export function EmptyState({ category }: EmptyStateProps) {
  const messages = {
    Ongoing: "You don't have any ongoing or upcoming trips.",
    Past: "You don't have any past trips.",
  };

  const suggestions = {
    Ongoing: "Start planning your next adventure!",
    Past: "Your completed trips will appear here.",
  };

  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 text-center">
      <div className="rounded-full bg-muted p-6 mb-4">
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
          className="text-muted-foreground"
        >
          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
          <circle cx="9" cy="7" r="4" />
          <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
          <path d="M16 3.13a4 4 0 0 1 0 7.75" />
        </svg>
      </div>
      <h3 className="text-lg font-semibold mb-2">{messages[category]}</h3>
      <p className="text-sm text-muted-foreground">{suggestions[category]}</p>
    </div>
  );
}
