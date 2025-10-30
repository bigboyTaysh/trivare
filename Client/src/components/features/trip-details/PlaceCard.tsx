import React from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Checkbox } from "@/components/ui/checkbox";
import { MapPin, ExternalLink, Clock, MoreHorizontal, GripVertical } from "lucide-react";
import type { DayAttractionDto } from "@/types/trips";

interface PlaceCardProps {
  placeAttraction: DayAttractionDto;
  index: number;
  onChange: () => void;
}

export const PlaceCard: React.FC<PlaceCardProps> = ({ placeAttraction, index, onChange }) => {
  const { place, isVisited } = placeAttraction;

  const handleVisitedToggle = () => {
    // TODO: Implement toggle visited status
    console.log("Toggle visited for place", place.id);
    onChange();
  };

  const handleRemovePlace = () => {
    // TODO: Implement remove place from day
    console.log("Remove place", place.id, "from day", placeAttraction.dayId);
    onChange();
  };

  return (
    <Card className={`transition-all ${isVisited ? "opacity-75" : ""}`}>
      <CardContent className="p-4">
        <div className="flex items-start gap-3">
          {/* Drag handle */}
          <div className="flex items-center pt-1">
            <GripVertical className="h-4 w-4 text-gray-400 cursor-move" />
          </div>

          {/* Main content */}
          <div className="flex-1 min-w-0">
            <div className="flex items-start justify-between">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 mb-1">
                  <h4 className="font-medium text-sm truncate">{place.name}</h4>
                  <Badge variant="secondary" className="text-xs">
                    #{index + 1}
                  </Badge>
                </div>

                {place.formattedAddress && (
                  <div className="flex items-center gap-1 text-xs text-gray-600 mb-2">
                    <MapPin className="h-3 w-3" />
                    <span className="truncate">{place.formattedAddress}</span>
                  </div>
                )}

                {place.openingHoursText && (
                  <div className="flex items-center gap-1 text-xs text-gray-600 mb-2">
                    <Clock className="h-3 w-3" />
                    <span className="truncate">{place.openingHoursText}</span>
                  </div>
                )}
              </div>

              {/* Actions */}
              <div className="flex items-center gap-2 ml-2">
                {place.googleMapsLink && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0"
                    onClick={() => window.open(place.googleMapsLink, "_blank")}
                  >
                    <ExternalLink className="h-3 w-3" />
                  </Button>
                )}

                <Button variant="ghost" size="sm" className="h-8 w-8 p-0">
                  <MoreHorizontal className="h-3 w-3" />
                </Button>
              </div>
            </div>

            {/* Footer with visited checkbox */}
            <div className="flex items-center justify-between mt-3 pt-3 border-t">
              <div className="flex items-center gap-2">
                <Checkbox id={`visited-${place.id}`} checked={isVisited} onCheckedChange={handleVisitedToggle} />
                <label htmlFor={`visited-${place.id}`} className="text-xs text-gray-600 cursor-pointer">
                  Mark as visited
                </label>
              </div>

              <Button
                variant="ghost"
                size="sm"
                onClick={handleRemovePlace}
                className="text-xs text-red-600 hover:text-red-700 hover:bg-red-50"
              >
                Remove
              </Button>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};
