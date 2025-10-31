import React, { useState, useEffect, useRef } from "react";
import { Input } from "./input";
import { cn } from "@/lib/utils";
import { getAutocompletePredictions } from "@/services/api";
import type { AutocompletePredictionDto } from "@/types/trips";
import { Loader2, MapPin } from "lucide-react";

interface AutocompleteInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  className?: string;
  disabled?: boolean;
  onSelect?: (prediction: AutocompletePredictionDto) => void;
}

export const AutocompleteInput: React.FC<AutocompleteInputProps> = ({
  value,
  onChange,
  placeholder,
  className,
  disabled = false,
  onSelect,
}) => {
  const [predictions, setPredictions] = useState<AutocompletePredictionDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [showDropdown, setShowDropdown] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState(-1);
  const inputRef = useRef<HTMLInputElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);
  const initialValueRef = useRef(value);
  const hasUserTypedRef = useRef(false);
  const isSelectingRef = useRef(false);

  // Debounced autocomplete search
  useEffect(() => {
    // Don't search if value is too short, we're selecting, or user hasn't typed anything yet
    if (value.length < 3 || isSelectingRef.current || !hasUserTypedRef.current) {
      if (!isSelectingRef.current && hasUserTypedRef.current) {
        setPredictions([]);
        setShowDropdown(false);
      }
      return;
    }

    // Clear previous timer
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    // Set new timer for debounced search
    debounceTimerRef.current = setTimeout(async () => {
      setIsLoading(true);
      try {
        const response = await getAutocompletePredictions(value);
        setPredictions(response.predictions);
        setShowDropdown(response.predictions.length > 0);
        setSelectedIndex(-1);
      } catch (error) {
        console.error("Autocomplete error:", error);
        setPredictions([]);
        setShowDropdown(false);
      } finally {
        setIsLoading(false);
      }
    }, 300); // 300ms debounce

    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, [value]);

  // Handle keyboard navigation
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!showDropdown || predictions.length === 0) return;

    switch (e.key) {
      case "ArrowDown":
        e.preventDefault();
        setSelectedIndex((prev) => (prev < predictions.length - 1 ? prev + 1 : 0));
        break;
      case "ArrowUp":
        e.preventDefault();
        setSelectedIndex((prev) => (prev > 0 ? prev - 1 : predictions.length - 1));
        break;
      case "Enter":
        e.preventDefault();
        if (selectedIndex >= 0 && selectedIndex < predictions.length) {
          handleSelect(predictions[selectedIndex]);
        }
        break;
      case "Escape":
        setShowDropdown(false);
        setSelectedIndex(-1);
        inputRef.current?.blur();
        break;
    }
  };

  const handleSelect = (prediction: AutocompletePredictionDto) => {
    // Prevent any API calls during selection
    isSelectingRef.current = true;

    onChange(prediction.description);
    setShowDropdown(false);
    setSelectedIndex(-1);
    onSelect?.(prediction);

    // Reset selecting flag after the change has propagated
    setTimeout(() => {
      isSelectingRef.current = false;
    }, 100);
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    // Mark that user has typed if the value changed from initial value
    if (!hasUserTypedRef.current && newValue !== initialValueRef.current) {
      hasUserTypedRef.current = true;
    }
    onChange(newValue);
  };

  const handleFocus = () => {
    // Only show dropdown if user has typed something and there are predictions
    if (predictions.length > 0 && value.length >= 3 && hasUserTypedRef.current && !isSelectingRef.current) {
      setShowDropdown(true);
    }
  };

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        inputRef.current &&
        !inputRef.current.contains(event.target as Node) &&
        dropdownRef.current &&
        !dropdownRef.current.contains(event.target as Node)
      ) {
        setShowDropdown(false);
        setSelectedIndex(-1);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  return (
    <div className="relative">
      <div className="relative">
        <Input
          ref={inputRef}
          type="text"
          value={value}
          onChange={handleInputChange}
          onFocus={handleFocus}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className={cn(className)}
          disabled={disabled}
        />
        {isLoading && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
          </div>
        )}
      </div>

      {showDropdown && (
        <div
          ref={dropdownRef}
          className="absolute z-50 w-full mt-1 bg-white border border-gray-200 rounded-md shadow-lg max-h-60 overflow-y-auto"
        >
          {predictions.map((prediction, index) => (
            <button
              key={prediction.placeId}
              type="button"
              className={cn(
                "w-full px-3 py-2 text-left hover:bg-gray-50 focus:bg-gray-50 focus:outline-none flex items-start gap-2",
                index === selectedIndex && "bg-gray-50"
              )}
              onClick={() => handleSelect(prediction)}
              onMouseEnter={() => setSelectedIndex(index)}
            >
              <MapPin className="h-4 w-4 text-gray-400 mt-0.5 flex-shrink-0" />
              <div className="flex-1 min-w-0">
                <div className="text-sm font-medium text-gray-900 truncate">
                  {prediction.structuredFormatting?.mainText || prediction.description}
                </div>
                {prediction.structuredFormatting?.secondaryText && (
                  <div className="text-xs text-gray-500 truncate">{prediction.structuredFormatting.secondaryText}</div>
                )}
              </div>
            </button>
          ))}
        </div>
      )}
    </div>
  );
};
