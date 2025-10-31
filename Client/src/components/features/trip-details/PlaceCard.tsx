import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  MapPin,
  ExternalLink,
  Clock,
  Trash2,
  GripVertical,
  Globe,
  Check,
  Circle,
  Edit,
  Star,
  ChevronDown,
} from "lucide-react";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { toast } from "sonner";
import { api } from "@/services/api";
import { getGooglePlacesPhotoUrl } from "@/lib/googlePlaces";
import type { DayAttractionDto } from "@/types/trips";

interface PlaceCardProps {
  placeAttraction: DayAttractionDto;
  index: number;
  onChange: () => void;
  onEdit?: (placeId: string) => void;
  selectedDate?: Date;
}

export const PlaceCard: React.FC<PlaceCardProps> = ({ placeAttraction, index, onChange, onEdit, selectedDate }) => {
  const { place, isVisited } = placeAttraction;
  const [isExpanded, setIsExpanded] = useState(false);

  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: placeAttraction.placeId,
  });

  // Get the day of the week for the selected date
  const getCurrentDayOfWeek = () => {
    if (!selectedDate) return null;
    const days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
    return days[selectedDate.getDay()];
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

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  const handleVisitedToggle = async () => {
    try {
      await api.updatePlaceOnDay(placeAttraction.dayId, place.id, {
        isVisited: !isVisited,
      });
      toast.success(isVisited ? "Place marked as not visited" : "Place marked as visited");
      onChange();
    } catch (error) {
      console.error("Failed to toggle visited status:", error);
      toast.error("Failed to update visited status");
    }
  };

  const handleRemovePlace = async () => {
    try {
      await api.removePlaceFromDay(placeAttraction.dayId, place.id);
      toast.success("Place removed from day");
      onChange();
    } catch (error) {
      console.error("Failed to remove place:", error);
      toast.error("Failed to remove place");
    }
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`bg-card border border-border rounded-lg px-3 py-3 transition-all hover:shadow-md ${
        isVisited ? "opacity-60 bg-muted/30" : ""
      } ${isDragging ? "shadow-lg z-50" : ""}`}
    >
      <div className="flex items-start gap-3">
        {/* Left side - Checkbox, drag handle, and Photo */}
        <div className="flex items-center gap-2 flex-shrink-0">
          {/* Visited Toggle - on the left */}
          <Button
            variant="ghost"
            size="sm"
            className={`h-6 w-6 p-0 rounded-full transition-all duration-200 hover:scale-110 ${
              isVisited
                ? "bg-primary text-primary-foreground hover:bg-primary/90"
                : "text-muted-foreground hover:text-foreground hover:bg-accent"
            }`}
            onClick={handleVisitedToggle}
            title={isVisited ? "Mark as not visited" : "Mark as visited"}
          >
            {isVisited ? <Check className="h-3 w-3" /> : <Circle className="h-3 w-3" />}
          </Button>

          {/* Drag handle - moved before photo */}
          <div className="flex items-center" {...attributes} {...listeners}>
            <GripVertical className="h-3 w-3 text-muted-foreground cursor-move hover:text-foreground" />
          </div>

          {/* Photo thumbnail */}
          {place.photoReference ? (
            <div
              className={`w-12 h-12 rounded-md overflow-hidden bg-gray-200 flex-shrink-0 ${
                isVisited ? "grayscale opacity-60" : ""
              }`}
            >
              <img
                src={getGooglePlacesPhotoUrl(place.photoReference, 100)}
                alt={place.name}
                className="w-full h-full object-cover"
                onError={(e) => {
                  e.currentTarget.style.display = "none";
                  e.currentTarget.nextElementSibling?.classList.remove("hidden");
                }}
              />
              <div className="w-full h-full bg-gray-200 flex items-center justify-center text-xs text-gray-500 hidden">
                <MapPin className="h-4 w-4" />
              </div>
            </div>
          ) : (
            <div className="w-12 h-12 rounded-md bg-gray-100 flex items-center justify-center flex-shrink-0">
              <MapPin className="h-5 w-5 text-gray-400" />
            </div>
          )}
        </div>

        {/* Main content */}
        <div className={`flex-1 min-w-0 ${isVisited ? "opacity-70" : ""}`}>
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1 min-w-0 space-y-1.5">
              {/* Header line: name, badge, and expand toggle */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2 flex-1 min-w-0">
                  <h4 className="font-medium text-sm truncate">{place.name}</h4>
                  <Badge
                    variant="secondary"
                    className="text-xs px-1.5 py-0.5 text-[10px] leading-tight bg-muted text-muted-foreground flex-shrink-0"
                  >
                    #{index + 1}
                  </Badge>
                </div>

                {/* Expand/Collapse toggle */}
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-6 w-6 p-0 hover:bg-accent flex-shrink-0"
                  onClick={() => setIsExpanded(!isExpanded)}
                  title={isExpanded ? "Collapse details" : "Expand details"}
                >
                  <ChevronDown
                    className={`h-3 w-3 transition-transform duration-200 ${isExpanded ? "rotate-0" : "-rotate-90"}`}
                  />
                </Button>
              </div>

              {/* Address - always visible */}
              {place.formattedAddress && (
                <div className="flex items-start gap-1 text-xs text-muted-foreground">
                  <MapPin className="h-3 w-3 flex-shrink-0 mt-0.5" />
                  <span className="line-clamp-2">{place.formattedAddress}</span>
                </div>
              )}

              {/* Opening hours - always visible, shows current day or all days */}
              {place.openingHoursText &&
                (() => {
                  const currentDay = getCurrentDayOfWeek();
                  const currentDayHours = currentDay ? getOpeningHoursForDay(place.openingHoursText, currentDay) : null;
                  const allHours = place.openingHoursText.split("; ");

                  return (
                    <div className="flex items-start gap-1 text-xs text-muted-foreground">
                      <Clock className="h-3 w-3 flex-shrink-0 mt-0.5" />
                      <div className="flex-1">
                        {isExpanded ? (
                          // Show all days when expanded
                          <div className="space-y-0.5">
                            <div className="font-medium mb-0.5">Opening Hours</div>
                            {allHours.map((hours, index) => (
                              <div key={index} className="leading-tight">
                                {hours}
                              </div>
                            ))}
                          </div>
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

              {/* Expandable details section */}
              {isExpanded && (
                <div className="space-y-2 animate-in slide-in-from-top-2 duration-200">
                  {/* Rating */}
                  {place.rating && (
                    <div className="flex items-center gap-1.5">
                      <div className="flex items-center gap-0.5">
                        <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
                        <span className="text-xs font-medium text-foreground">{place.rating.toFixed(1)}</span>
                      </div>
                      {place.userRatingCount && (
                        <span className="text-xs text-muted-foreground">
                          ({place.userRatingCount.toLocaleString()} reviews)
                        </span>
                      )}
                    </div>
                  )}

                  {/* Additional details */}
                  <div className="pt-1 border-t border-border/50">
                    <div className="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
                      {/* Source indicator */}
                      <div className="flex items-center gap-1">
                        <Globe className="h-3 w-3" />
                        <span>{place.isManuallyAdded ? "Manually added" : "From Google Places"}</span>
                      </div>

                      {/* Google Place ID if available */}
                      {place.googlePlaceId && (
                        <div className="flex items-center gap-1">
                          <span className="text-[10px] bg-muted px-1.5 py-0.5 rounded">
                            ID: {place.googlePlaceId.slice(0, 8)}...
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* Actions */}
            <div className="flex items-start gap-0.5 ml-1 flex-shrink-0">
              {place.googleMapsLink && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 w-7 p-0 hover:bg-accent"
                  title="Open in Google Maps"
                  onClick={() => window.open(place.googleMapsLink, "_blank")}
                >
                  <ExternalLink className="h-3.5 w-3.5" />
                </Button>
              )}

              {place.website && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 w-7 p-0 hover:bg-accent"
                  title="Visit website"
                  onClick={() => window.open(place.website, "_blank")}
                >
                  <Globe className="h-3.5 w-3.5" />
                </Button>
              )}

              {onEdit && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 w-7 p-0 hover:bg-accent"
                  title="Edit place details"
                  onClick={() => onEdit(place.id)}
                >
                  <Edit className="h-3.5 w-3.5" />
                </Button>
              )}

              <Button
                variant="ghost"
                size="sm"
                className="h-7 w-7 p-0 text-muted-foreground hover:text-foreground hover:bg-accent"
                title="Remove place"
                onClick={handleRemovePlace}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
