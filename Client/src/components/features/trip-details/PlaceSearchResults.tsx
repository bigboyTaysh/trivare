import React, { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { MapPin, ExternalLink, Plus, Loader2 } from "lucide-react";
import { getGooglePlacesPhotoUrl } from "@/lib/googlePlaces";
import type { PlaceDto } from "@/types/trips";

interface PlaceSearchResultsProps {
  results: PlaceDto[];
  isLoading?: boolean;
  onSelect: (place: PlaceDto) => void;
}

export const PlaceSearchResults: React.FC<PlaceSearchResultsProps> = ({ results, isLoading, onSelect }) => {
  const [currentStatusIndex, setCurrentStatusIndex] = useState(0);

  const statusMessages = [
    "Searching for places with AI...",
    "Getting list of places...",
    "Looking for the best suggestions...",
    "Finding perfect matches...",
    "Analyzing locations...",
  ];

  useEffect(() => {
    if (!isLoading) return;

    const interval = setInterval(() => {
      setCurrentStatusIndex((prev) => (prev + 1) % statusMessages.length);
    }, 2000); // Change message every 2 seconds

    return () => clearInterval(interval);
  }, [isLoading, statusMessages.length]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="flex flex-col items-center gap-3">
          <Loader2 className="h-8 w-8 animate-spin text-primary" />
          <p className="text-sm text-muted-foreground">{statusMessages[currentStatusIndex]}</p>
        </div>
      </div>
    );
  }

  if (results.length === 0) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="text-center">
          <MapPin className="h-12 w-12 text-gray-300 mx-auto mb-3" />
          <p className="text-sm text-muted-foreground">No places found. Try a different search.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
      {results.map((place) => (
        <Card key={place.id} className="overflow-hidden hover:shadow-lg transition-shadow">
          {place.photoReference && (
            <div className="h-40 w-full bg-gray-200 relative overflow-hidden">
              <img
                src={getGooglePlacesPhotoUrl(place.photoReference)}
                alt={place.name}
                className="w-full h-full object-cover"
                onError={(e) => {
                  e.currentTarget.style.display = "none";
                }}
              />
            </div>
          )}
          <CardContent className="p-4">
            <div className="flex flex-col gap-2">
              <div className="flex items-start justify-between gap-2">
                <h3 className="font-semibold text-sm line-clamp-2">{place.name}</h3>
                {place.googleMapsLink && (
                  <a
                    href={place.googleMapsLink}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-blue-600 hover:text-blue-800 flex-shrink-0"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <ExternalLink className="h-4 w-4" />
                  </a>
                )}
              </div>

              {place.formattedAddress && (
                <div className="flex items-start gap-1.5 text-xs text-muted-foreground">
                  <MapPin className="h-3.5 w-3.5 flex-shrink-0 mt-0.5" />
                  <span className="line-clamp-2">{place.formattedAddress}</span>
                </div>
              )}

              {place.website && (
                <a
                  href={place.website}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-xs text-blue-600 hover:underline truncate"
                  onClick={(e) => e.stopPropagation()}
                >
                  {place.website}
                </a>
              )}

              {place.openingHoursText && (
                <p className="text-xs text-muted-foreground line-clamp-2">{place.openingHoursText.split(";")[0]}</p>
              )}

              <Button onClick={() => onSelect(place)} size="sm" className="w-full mt-2">
                <Plus className="h-4 w-4 mr-1" />
                Add to Day
              </Button>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};
