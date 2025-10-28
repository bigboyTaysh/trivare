import React, { useEffect } from "react";
import { useTripDetails } from "@/hooks/useTripDetails";
import { useTripFiles } from "@/hooks/useTripFiles";
import TripHeader from "@/components/features/trip-details/TripHeader";
import TripContent from "@/components/features/trip-details/TripContent";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { AlertCircle, ArrowLeft } from "lucide-react";
import { toast } from "sonner";

interface TripDetailsPageProps {
  tripId: string;
}

const TripDetailsPage: React.FC<TripDetailsPageProps> = ({ tripId }) => {
  const { trip, isLoading, error, updateTrip, deleteTrip } = useTripDetails(tripId);
  const { totalFileCount, refreshFileCount } = useTripFiles(tripId);

  useEffect(() => {
    if (error) {
      toast.error("Failed to load trip");
    }
  }, [error]);

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="mb-8">
          <CardHeader>
            <Skeleton className="h-8 w-64" />
            <Skeleton className="h-4 w-48" />
          </CardHeader>
          <CardContent>
            <div className="flex gap-2">
              <Skeleton className="h-10 w-24" />
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div className="md:col-span-4">
            <Skeleton className="h-12 w-full" />
          </div>
          <div className="md:col-span-4">
            <Skeleton className="h-64 w-full" />
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="text-center py-12">
          <CardContent>
            <AlertCircle className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <h2 className="text-2xl font-semibold mb-2">Trip Not Found</h2>
            <p className="text-muted-foreground mb-6">
              The trip you&apos;re looking for doesn&apos;t exist or you don&apos;t have permission to view it.
            </p>
            <Button onClick={() => window.history.back()} variant="outline">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!trip) {
    return (
      <div className="container mx-auto px-4 py-8">
        <Card className="text-center py-12">
          <CardContent>
            <AlertCircle className="mx-auto h-12 w-12 text-muted-foreground mb-4" />
            <h2 className="text-2xl font-semibold mb-2">Trip Not Found</h2>
            <p className="text-muted-foreground mb-6">
              The trip you&apos;re looking for doesn&apos;t exist or you don&apos;t have permission to view it.
            </p>
            <Button onClick={() => window.history.back()} variant="outline">
              <ArrowLeft className="mr-2 h-4 w-4" />
              Go Back
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-4 md:py-6">
      <div className="mb-4">
        <button
          onClick={() => (window.location.href = "/")}
          className="text-xs text-muted-foreground hover:text-foreground flex items-center gap-1"
        >
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="14"
            height="14"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <path d="m15 18-6-6 6-6"></path>
          </svg>
          Back to Dashboard
        </button>
      </div>

      <TripHeader
        trip={trip}
        onUpdate={updateTrip}
        onDelete={deleteTrip}
        totalFileCount={totalFileCount}
        onFileChange={refreshFileCount}
      />
      <TripContent tripId={tripId} totalFileCount={totalFileCount} onFileChange={refreshFileCount} />
    </div>
  );
};

export default TripDetailsPage;
