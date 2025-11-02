import React, { useState, useCallback } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import TransportSection from "./TransportSection";
import AccommodationSection from "./AccommodationSection";
import DaysSection from "./DaysSection";
import { Calendar, Home, Plane } from "lucide-react";
import { useTripDetails } from "@/hooks/useTripDetails";
import type {
  DayWithPlacesDto,
  AddAccommodationRequest,
  UpdateAccommodationRequest,
  CreateTransportRequest,
  UpdateTransportRequest,
  CreateDayRequest,
  UpdatePlaceRequest,
  AddPlaceRequest,
  AccommodationDto,
  PlaceDto,
  DayAttractionDto,
} from "@/types/trips";

interface TripContentProps {
  tripId: string;
  totalFileCount: number;
  onFileChange: () => void;
  tripStartDate?: string;
  tripEndDate?: string;
  tripDestination?: string;
}

const TripContent: React.FC<TripContentProps> = ({
  tripId,
  totalFileCount,
  onFileChange,
  tripStartDate,
  tripEndDate,
  tripDestination,
}) => {
  const { trip: initialTrip, isLoading: tripLoading, refetch } = useTripDetails(tripId);
  const [activeSection, setActiveSection] = useState<string | null>("days");
  // Shared state for day selection across mobile and desktop views
  const [selectedDay, setSelectedDay] = useState<DayWithPlacesDto | null>(null);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);

  // Local state for optimistic updates
  const [localTrip, setLocalTrip] = useState(initialTrip);

  // Update local state when initial trip data changes
  React.useEffect(() => {
    setLocalTrip(initialTrip);
  }, [initialTrip]);

  // Extract accommodation, transport, and days data from local trip state
  const accommodation = localTrip?.accommodation;
  const transports = localTrip?.transports || [];
  const days = localTrip?.days || [];

  const handleDaySelect = (day: DayWithPlacesDto | null, date: Date | null) => {
    setSelectedDay(day);
    setSelectedDate(date);
  };

  // Optimistic update function for place visited status
  const updatePlaceVisitedStatus = useCallback(
    async (dayId: string, placeId: string, isVisited: boolean) => {
      // Optimistically update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        const updatedDays = prevTrip.days.map((day) => {
          if (day.id !== dayId) return day;

          const updatedPlaces = day.places?.map((placeAttraction) => {
            if (placeAttraction.placeId !== placeId) return placeAttraction;

            return {
              ...placeAttraction,
              isVisited,
            };
          });

          return {
            ...day,
            places: updatedPlaces || null,
          };
        });

        return {
          ...prevTrip,
          days: updatedDays,
        };
      });

      try {
        // Update the backend
        const { api } = await import("@/services/api");
        await api.updatePlaceOnDay(dayId, placeId, { isVisited });
      } catch (error) {
        // Revert optimistic update on error
        setLocalTrip(initialTrip);
        throw error;
      }
    },
    [initialTrip]
  );

  // Optimistic update function for adding places
  const addPlaceOptimistic = useCallback(async (dayId: string, data: AddPlaceRequest) => {
    const { api } = await import("@/services/api");
    const newPlace = await api.addPlaceToDay(dayId, data);

    // Update local state - add the new place to the appropriate day
    setLocalTrip((prevTrip) => {
      if (!prevTrip?.days) return prevTrip;

      const updatedDays = prevTrip.days.map((day) => {
        if (day.id !== dayId) return day;

        // Add the new place to the day's places array
        const updatedPlaces = [...(day.places || []), newPlace];

        return {
          ...day,
          places: updatedPlaces,
        };
      });

      return {
        ...prevTrip,
        days: updatedDays,
      };
    });

    return newPlace;
  }, []);

  // Optimistic update function for place updates (name, etc.)
  const updatePlaceOptimistic = useCallback(
    async (placeId: string, data: UpdatePlaceRequest) => {
      // Find the current place data to store for potential rollback
      let originalPlaceData: PlaceDto | null = null;
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        for (const day of prevTrip.days) {
          const place = day.places?.find((p) => p.place.id === placeId);
          if (place) {
            originalPlaceData = { ...place.place };
            break;
          }
        }
        return prevTrip;
      });

      // Optimistically update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        const updatedDays = prevTrip.days.map((day) => {
          if (!day.places) return day;

          const updatedPlaces = day.places.map((placeAttraction) => {
            if (placeAttraction.place.id !== placeId) return placeAttraction;

            return {
              ...placeAttraction,
              place: {
                ...placeAttraction.place,
                ...data,
              },
            };
          });

          return {
            ...day,
            places: updatedPlaces,
          };
        });

        return {
          ...prevTrip,
          days: updatedDays,
        };
      });

      try {
        // Update the backend
        const { api } = await import("@/services/api");
        await api.updatePlace(placeId, data);
      } catch (error) {
        // Revert optimistic update on error
        if (originalPlaceData) {
          setLocalTrip((prevTrip) => {
            if (!prevTrip?.days) return prevTrip;

            const updatedDays = prevTrip.days.map((day) => {
              if (!day.places) return day;

              const updatedPlaces = day.places.map((placeAttraction) => {
                if (placeAttraction.place.id !== placeId) return placeAttraction;

                return {
                  ...placeAttraction,
                  place: originalPlaceData as PlaceDto,
                };
              });

              return {
                ...day,
                places: updatedPlaces,
              };
            });

            return {
              ...prevTrip,
              days: updatedDays,
            };
          });
        } else {
          // If we don't have original data to revert to, refetch from server
          setLocalTrip(initialTrip);
        }
        throw error;
      }
    },
    [initialTrip]
  );

  // Optimistic update function for reordering places
  const reorderPlacesOptimistic = useCallback(
    async (dayId: string, reorderedPlaces: DayAttractionDto[]) => {
      // Store original order for potential rollback
      let originalPlaces: DayAttractionDto[] | null = null;
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        const day = prevTrip.days.find((d) => d.id === dayId);
        if (day?.places) {
          originalPlaces = [...day.places];
        }
        return prevTrip;
      });

      // Optimistically update local state with reordered places (with updated order properties)
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        const updatedDays = prevTrip.days.map((day) => {
          if (day.id !== dayId) return day;

          // Update the order property for each place to match the new position
          const placesWithUpdatedOrder = reorderedPlaces.map((place, index) => ({
            ...place,
            order: index + 1,
          }));

          return {
            ...day,
            places: placesWithUpdatedOrder,
          };
        });

        return {
          ...prevTrip,
          days: updatedDays,
        };
      });

      try {
        // Update each place's order on the backend
        const { api } = await import("@/services/api");
        await Promise.all(
          reorderedPlaces.map((place, index) => api.updatePlaceOnDay(dayId, place.placeId, { order: index + 1 }))
        );
      } catch (error) {
        // Revert optimistic update on error
        if (originalPlaces) {
          setLocalTrip((prevTrip) => {
            if (!prevTrip?.days) return prevTrip;

            const updatedDays = prevTrip.days.map((day) => {
              if (day.id !== dayId) return day;

              return {
                ...day,
                places: originalPlaces,
              };
            });

            return {
              ...prevTrip,
              days: updatedDays,
            };
          });
        } else {
          // If we don't have original data to revert to, refetch from server
          setLocalTrip(initialTrip);
        }
        throw error;
      }
    },
    [initialTrip]
  );

  // Optimistic update function for deleting places
  const deletePlaceOptimistic = useCallback(
    async (dayId: string, placeId: string) => {
      // Store the place data for potential rollback
      let removedPlaceData: DayAttractionDto | null = null;
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        for (const day of prevTrip.days) {
          const place = day.places?.find((p) => p.place.id === placeId);
          if (place) {
            removedPlaceData = { ...place };
            break;
          }
        }
        return prevTrip;
      });

      // Optimistically remove place from local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip?.days) return prevTrip;

        const updatedDays = prevTrip.days.map((day) => {
          if (day.id !== dayId) return day;

          const filteredPlaces = day.places?.filter((placeAttraction) => placeAttraction.place.id !== placeId) || null;

          return {
            ...day,
            places: filteredPlaces,
          };
        });

        return {
          ...prevTrip,
          days: updatedDays,
        };
      });

      try {
        // Delete from backend
        const { api } = await import("@/services/api");
        await api.removePlaceFromDay(dayId, placeId);
      } catch (error) {
        // Revert optimistic update on error - add the place back
        if (removedPlaceData) {
          setLocalTrip((prevTrip) => {
            if (!prevTrip?.days) return prevTrip;

            const updatedDays = prevTrip.days.map((day) => {
              if (day.id !== dayId) return day;

              const updatedPlaces = [...(day.places || []), removedPlaceData as DayAttractionDto];

              return {
                ...day,
                places: updatedPlaces,
              };
            });

            return {
              ...prevTrip,
              days: updatedDays,
            };
          });
        } else {
          // If we don't have original data to revert to, refetch from server
          setLocalTrip(initialTrip);
        }
        throw error;
      }
    },
    [initialTrip]
  );

  // Optimistic update functions for accommodation
  const addAccommodationOptimistic = useCallback(
    async (data: AddAccommodationRequest) => {
      const { api } = await import("@/services/api");
      const newAccommodation = await api.addAccommodation(tripId, data);

      // Update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip) return prevTrip;
        return {
          ...prevTrip,
          accommodation: {
            ...newAccommodation,
            name: newAccommodation.name || "",
          },
        };
      });

      return newAccommodation;
    },
    [tripId]
  );

  const updateAccommodationOptimistic = useCallback(
    async (data: UpdateAccommodationRequest) => {
      const { api } = await import("@/services/api");
      const updatedAccommodation = await api.updateAccommodation(tripId, data);

      // Update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip) return prevTrip;
        return {
          ...prevTrip,
          accommodation: {
            ...updatedAccommodation,
            name: updatedAccommodation.name || "",
          },
        };
      });

      return updatedAccommodation;
    },
    [tripId]
  );

  const deleteAccommodationOptimistic = useCallback(async () => {
    const { api } = await import("@/services/api");
    await api.deleteAccommodation(tripId);

    // Update local state
    setLocalTrip((prevTrip) => {
      if (!prevTrip) return prevTrip;
      return {
        ...prevTrip,
        accommodation: undefined,
      };
    });
  }, [tripId]);

  // Optimistic update functions for transport
  const addTransportOptimistic = useCallback(
    async (data: CreateTransportRequest) => {
      const { api } = await import("@/services/api");
      const newTransport = await api.addTransport(tripId, data);

      // Update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip) return prevTrip;
        return {
          ...prevTrip,
          transports: [...(prevTrip.transports || []), newTransport],
        };
      });

      return newTransport;
    },
    [tripId]
  );

  const updateTransportOptimistic = useCallback(async (transportId: string, data: UpdateTransportRequest) => {
    const { api } = await import("@/services/api");
    const updatedTransport = await api.updateTransport(transportId, data);

    // Update local state
    setLocalTrip((prevTrip) => {
      if (!prevTrip) return prevTrip;
      return {
        ...prevTrip,
        transports: prevTrip.transports?.map((t) => (t.id === transportId ? updatedTransport : t)),
      };
    });

    return updatedTransport;
  }, []);

  const deleteTransportOptimistic = useCallback(async (transportId: string) => {
    const { api } = await import("@/services/api");
    await api.deleteTransport(transportId);

    // Update local state
    setLocalTrip((prevTrip) => {
      if (!prevTrip) return prevTrip;
      return {
        ...prevTrip,
        transports: prevTrip.transports?.filter((t) => t.id !== transportId),
      };
    });
  }, []);

  // Optimistic update function for adding days
  const addDayOptimistic = useCallback(
    async (data: CreateDayRequest) => {
      const { api } = await import("@/services/api");
      const newDay = await api.createDay(tripId, data);

      // Update local state
      setLocalTrip((prevTrip) => {
        if (!prevTrip) return prevTrip;
        return {
          ...prevTrip,
          days: [...(prevTrip.days || []), newDay],
        };
      });

      return newDay;
    },
    [tripId]
  );

  // Create a refresh function that can be passed down to child components
  const refreshTripData = useCallback(() => {
    // This will trigger a re-fetch of trip data via useTripDetails
    refetch();
  }, [refetch]);

  return (
    <>
      {/* Desktop: Tabs */}
      <div className="hidden md:block">
        <Tabs value={activeSection || ""} onValueChange={setActiveSection}>
          <TabsList className="grid w-full grid-cols-3">
            <TabsTrigger value="days" className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Days
            </TabsTrigger>
            <TabsTrigger value="accommodation" className="flex items-center gap-2">
              <Home className="h-4 w-4" />
              Accommodation
            </TabsTrigger>
            <TabsTrigger value="transport" className="flex items-center gap-2">
              <Plane className="h-4 w-4" />
              Transport
            </TabsTrigger>
          </TabsList>
          <TabsContent value="days" className="mt-6">
            {activeSection === "days" && (
              <DaysSection
                tripId={tripId}
                totalFileCount={totalFileCount}
                onFileChange={onFileChange}
                tripStartDate={tripStartDate}
                tripEndDate={tripEndDate}
                tripDestination={tripDestination}
                selectedDay={selectedDay}
                selectedDate={selectedDate}
                onDaySelect={handleDaySelect}
                days={days}
                onDaysChange={refreshTripData}
                onPlaceVisitedChange={updatePlaceVisitedStatus}
                onPlaceUpdate={updatePlaceOptimistic}
                onAddPlace={addPlaceOptimistic}
                onDeletePlace={deletePlaceOptimistic}
                onReorderPlaces={reorderPlacesOptimistic}
                onAddDay={addDayOptimistic}
                isLoading={tripLoading}
              />
            )}
          </TabsContent>
          <TabsContent value="accommodation" className="mt-6">
            {activeSection === "accommodation" && (
              <AccommodationSection
                tripId={tripId}
                totalFileCount={totalFileCount}
                onFileChange={onFileChange}
                accommodation={accommodation as AccommodationDto}
                onAddAccommodation={addAccommodationOptimistic}
                onUpdateAccommodation={updateAccommodationOptimistic}
                onDeleteAccommodation={deleteAccommodationOptimistic}
                isLoading={tripLoading}
              />
            )}
          </TabsContent>
          <TabsContent value="transport" className="mt-6">
            {activeSection === "transport" && (
              <TransportSection
                tripId={tripId}
                totalFileCount={totalFileCount}
                onFileChange={onFileChange}
                transports={transports}
                onAddTransport={addTransportOptimistic}
                onUpdateTransport={updateTransportOptimistic}
                onDeleteTransport={deleteTransportOptimistic}
                isLoading={tripLoading}
              />
            )}
          </TabsContent>
        </Tabs>
      </div>

      {/* Mobile: Accordion */}
      <div className="md:hidden">
        <Accordion type="single" collapsible value={activeSection || ""} onValueChange={setActiveSection}>
          <AccordionItem value="days">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Calendar className="h-4 w-4" />
                Days
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "days" && (
                <DaysSection
                  tripId={tripId}
                  totalFileCount={totalFileCount}
                  onFileChange={onFileChange}
                  tripStartDate={tripStartDate}
                  tripEndDate={tripEndDate}
                  tripDestination={tripDestination}
                  selectedDay={selectedDay}
                  selectedDate={selectedDate}
                  onDaySelect={handleDaySelect}
                  days={days}
                  onDaysChange={refreshTripData}
                  onPlaceVisitedChange={updatePlaceVisitedStatus}
                  onPlaceUpdate={updatePlaceOptimistic}
                  onAddPlace={addPlaceOptimistic}
                  onDeletePlace={deletePlaceOptimistic}
                  onReorderPlaces={reorderPlacesOptimistic}
                  onAddDay={addDayOptimistic}
                  isLoading={tripLoading}
                />
              )}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="accommodation">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Home className="h-4 w-4" />
                Accommodation
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "accommodation" && (
                <AccommodationSection
                  tripId={tripId}
                  totalFileCount={totalFileCount}
                  onFileChange={onFileChange}
                  accommodation={accommodation as AccommodationDto}
                  onAddAccommodation={addAccommodationOptimistic}
                  onUpdateAccommodation={updateAccommodationOptimistic}
                  onDeleteAccommodation={deleteAccommodationOptimistic}
                  isLoading={tripLoading}
                />
              )}
            </AccordionContent>
          </AccordionItem>
          <AccordionItem value="transport">
            <AccordionTrigger>
              <span className="flex items-center gap-2">
                <Plane className="h-4 w-4" />
                Transport
              </span>
            </AccordionTrigger>
            <AccordionContent className="px-4 py-2">
              {activeSection === "transport" && (
                <TransportSection
                  tripId={tripId}
                  totalFileCount={totalFileCount}
                  onFileChange={onFileChange}
                  transports={transports}
                  onAddTransport={addTransportOptimistic}
                  onUpdateTransport={updateTransportOptimistic}
                  onDeleteTransport={deleteTransportOptimistic}
                  isLoading={tripLoading}
                />
              )}
            </AccordionContent>
          </AccordionItem>
        </Accordion>
      </div>
    </>
  );
};

export default TripContent;
