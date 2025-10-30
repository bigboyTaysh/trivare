import React from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { MapPin, ExternalLink, Clock, Trash2, GripVertical, Globe } from "lucide-react";
import { toast } from "sonner";
import { api } from "@/services/api";
import type { DayAttractionDto } from "@/types/trips";

interface PlaceCardProps {
  placeAttraction: DayAttractionDto;
  index: number;
  onChange: () => void;
}

export const PlaceCard: React.FC<PlaceCardProps> = ({ placeAttraction, index, onChange }) => {
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
      className={`bg-white border border-gray-200 rounded-md px-2 py-2 transition-all hover:shadow-sm ${
        isVisited ? "opacity-75" : ""
      }`}
    >
      <div className="flex items-center gap-1.5">
        {/* Drag handle */}
        <div className="flex items-center flex-shrink-0">
          <GripVertical className="h-3 w-3 text-gray-400 cursor-move hover:text-gray-600" />
        </div>

        {/* Checkbox */}
        <div className="flex items-center flex-shrink-0">
          <Checkbox
            id={`visited-${place.id}`}
            checked={isVisited}
            onCheckedChange={handleVisitedToggle}
            className="h-3 w-3"
          />
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
                  className="text-xs px-1 py-0 text-[10px] leading-tight bg-gray-100 text-gray-600 flex-shrink-0"
                >
                  #{index + 1}
                </Badge>
              </div>

              {/* Second line: address and opening hours */}
              <div className="flex items-center gap-2">
                {place.formattedAddress && (
                  <div className="flex items-center gap-0.5 text-xs text-gray-500 flex-shrink-0">
                    <MapPin className="h-2.5 w-2.5 flex-shrink-0" />
                    <span className="truncate max-w-[120px]">{place.formattedAddress}</span>
                  </div>
                )}
                {place.openingHoursText && (
                  <div className="flex items-center gap-0.5 text-xs text-gray-500 flex-shrink-0">
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
                  className="h-6 w-6 p-0 hover:bg-gray-100"
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
                  className="h-6 w-6 p-0 hover:bg-gray-100"
                  title="Visit website"
                  onClick={() => window.open(place.website, "_blank")}
                >
                  <Globe className="h-3 w-3" />
                </Button>
              )}

              <Button
                variant="ghost"
                size="sm"
                className="h-6 w-6 p-0 text-gray-600 hover:text-gray-800 hover:bg-gray-100"
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
