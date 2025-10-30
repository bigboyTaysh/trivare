import React from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { MapPin, ExternalLink, Clock, Trash2, GripVertical, Globe, Check, Circle, Edit } from "lucide-react";
import { toast } from "sonner";
import { api } from "@/services/api";
import type { DayAttractionDto } from "@/types/trips";

interface PlaceCardProps {
  placeAttraction: DayAttractionDto;
  index: number;
  onChange: () => void;
  onEdit?: (placeId: string) => void;
}

export const PlaceCard: React.FC<PlaceCardProps> = ({ placeAttraction, index, onChange, onEdit }) => {
  const { place, isVisited } = placeAttraction;

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
      className={`bg-card border border-border rounded-md px-2 py-2 transition-all hover:shadow-sm ${
        isVisited ? "opacity-75" : ""
      }`}
    >
      <div className="flex items-center gap-1.5">
        {/* Drag handle */}
        <div className="flex items-center flex-shrink-0">
          <GripVertical className="h-3 w-3 text-muted-foreground cursor-move hover:text-foreground" />
        </div>

        {/* Visited Toggle */}
        <div className="flex items-center flex-shrink-0">
          <Button
            variant="ghost"
            size="sm"
            className={`h-5 w-5 p-0 rounded-full transition-all duration-200 hover:scale-110 ${
              isVisited
                ? "bg-primary text-primary-foreground hover:bg-primary/90"
                : "text-muted-foreground hover:text-foreground hover:bg-accent"
            }`}
            onClick={handleVisitedToggle}
            title={isVisited ? "Mark as not visited" : "Mark as visited"}
          >
            {isVisited ? <Check className="h-3 w-3" /> : <Circle className="h-3 w-3" />}
          </Button>
        </div>

        {/* Main content */}
        <div className="flex-1 min-w-0">
          <div className="flex items-start justify-between gap-2">
            <div className="flex-1 min-w-0 space-y-0.5">
              {/* First line: name and badge */}
              <div className="flex items-center gap-1.5">
                <h4 className="font-medium text-sm truncate">{place.name}</h4>
                <Badge
                  variant="secondary"
                  className="text-xs px-1 py-0 text-[10px] leading-tight bg-muted text-muted-foreground flex-shrink-0"
                >
                  #{index + 1}
                </Badge>
              </div>

              {/* Second line: address and opening hours */}
              <div className="flex items-center gap-2">
                {place.formattedAddress && (
                  <div className="flex items-center gap-0.5 text-xs text-muted-foreground flex-shrink-0">
                    <MapPin className="h-2.5 w-2.5 flex-shrink-0" />
                    <span className="truncate max-w-[120px]">{place.formattedAddress}</span>
                  </div>
                )}
                {place.openingHoursText && (
                  <div className="flex items-center gap-0.5 text-xs text-muted-foreground flex-shrink-0">
                    <Clock className="h-2.5 w-2.5 flex-shrink-0" />
                    <span className="truncate max-w-[100px]">{place.openingHoursText}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-0.5 ml-1 flex-shrink-0">
              {place.googleMapsLink && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-6 w-6 p-0 hover:bg-accent"
                  title="Open in Google Maps"
                  onClick={() => window.open(place.googleMapsLink, "_blank")}
                >
                  <ExternalLink className="h-3 w-3" />
                </Button>
              )}

              {place.website && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-6 w-6 p-0 hover:bg-accent"
                  title="Visit website"
                  onClick={() => window.open(place.website, "_blank")}
                >
                  <Globe className="h-3 w-3" />
                </Button>
              )}

              {onEdit && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-6 w-6 p-0 hover:bg-accent"
                  title="Edit place details"
                  onClick={() => onEdit(place.id)}
                >
                  <Edit className="h-3 w-3" />
                </Button>
              )}

              <Button
                variant="ghost"
                size="sm"
                className="h-6 w-6 p-0 text-muted-foreground hover:text-foreground hover:bg-accent"
                title="Remove place"
                onClick={handleRemovePlace}
              >
                <Trash2 className="h-3 w-3" />
              </Button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
