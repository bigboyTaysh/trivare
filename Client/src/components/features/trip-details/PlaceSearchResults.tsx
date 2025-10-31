import React, { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { MapPin, ExternalLink, Plus, Loader2, Star, ChevronDown } from "lucide-react";
import { getGooglePlacesPhotoUrl } from "@/lib/googlePlaces";
import type { PlaceDto } from "@/types/trips";

interface PlaceSearchResultsProps {
  results: PlaceDto[];
  isLoading?: boolean;
  onSelect: (place: PlaceDto) => void;
  selectedDate?: Date;
}

export const PlaceSearchResults: React.FC<PlaceSearchResultsProps> = ({
  results,
  isLoading,
  onSelect,
  selectedDate,
}) => {
  const [currentStatusIndex, setCurrentStatusIndex] = useState(0);
  const [expandedHours, setExpandedHours] = useState<Set<string>>(new Set());

  // Get the day of the week for the selected date
  const getCurrentDayOfWeek = () => {
    if (!selectedDate) return null;
    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    return days[selectedDate.getDay()];
  };

  // Toggle expanded hours for a place
  const toggleExpandedHours = (placeId: string) => {
    const newExpanded = new Set(expandedHours);
    if (newExpanded.has(placeId)) {
      newExpanded.delete(placeId);
    } else {
      newExpanded.add(placeId);
    }
    setExpandedHours(newExpanded);
  };

  // Get opening hours for a specific day (handles multiple languages)
  const getOpeningHoursForDay = (openingHoursText: string, dayOfWeek: string) => {
    const hours = openingHoursText.split("; ");

    // Try English first
    let match = hours.find((hour) => hour.toLowerCase().startsWith(dayOfWeek.toLowerCase()));
    if (match) return match;

    // Try Polish day names
    const polishDays: Record<string, string> = {
      Monday: "poniedziałek",
      Tuesday: "wtorek",
      Wednesday: "środa",
      Thursday: "czwartek",
      Friday: "piątek",
      Saturday: "sobota",
      Sunday: "niedziela",
    };

    const polishDay = polishDays[dayOfWeek];
    if (polishDay) {
      match = hours.find((hour) => hour.toLowerCase().startsWith(polishDay.toLowerCase()));
      if (match) return match;
    }

    // Try other common patterns (day position in week)
    const dayIndex = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"].indexOf(dayOfWeek);
    if (dayIndex !== -1 && hours.length > dayIndex) {
      return hours[dayIndex];
    }

    return null;
  };

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
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
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

              {place.rating && (
                <div className="flex items-center gap-1 text-sm">
                  <Star className="h-4 w-4 fill-yellow-400 text-yellow-400" />
                  <span className="font-medium">{place.rating.toFixed(1)}</span>
                  {place.userRatingCount && <span className="text-muted-foreground">({place.userRatingCount})</span>}
                </div>
              )}

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

              {place.openingHoursText &&
                (() => {
                  const currentDay = getCurrentDayOfWeek();
                  const isExpanded = expandedHours.has(place.id);
                  const currentDayHours = currentDay ? getOpeningHoursForDay(place.openingHoursText, currentDay) : null;
                  const allHours = place.openingHoursText.split("; ");

                  return (
                    <div className="text-xs text-muted-foreground">
                      <div className="flex items-center justify-between mb-1">
                        <div className="font-medium">Opening hours:</div>
                        {currentDay && (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="h-5 px-2 text-xs"
                            onClick={(e) => {
                              e.stopPropagation();
                              toggleExpandedHours(place.id);
                            }}
                          >
                            {isExpanded ? "Show less" : "Show all"}
                            <ChevronDown
                              className={`h-3 w-3 ml-1 transition-transform ${isExpanded ? "rotate-180" : ""}`}
                            />
                          </Button>
                        )}
                      </div>
                      <div className="space-y-0.5">
                        {isExpanded ? (
                          // Show all days when expanded
                          allHours.map((hours, index) => (
                            <div key={index} className="leading-tight">
                              {hours}
                            </div>
                          ))
                        ) : currentDayHours ? (
                          // Show only current day when collapsed
                          <div className="leading-tight font-medium text-foreground">{currentDayHours}</div>
                        ) : (
                          // Fallback: show first day if no current day match
                          <div className="leading-tight">{allHours[0]}</div>
                        )}
                      </div>
                    </div>
                  );
                })()}

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
