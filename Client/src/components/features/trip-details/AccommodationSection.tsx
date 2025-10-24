import React from "react";

interface AccommodationSectionProps {
  tripId: string;
}

const AccommodationSection: React.FC<AccommodationSectionProps> = () => {
  return (
    <div className="p-4 border rounded-lg">
      <h3 className="text-lg font-semibold mb-4">Accommodation</h3>
      <p>Accommodation section - to be implemented</p>
    </div>
  );
};

export default AccommodationSection;
