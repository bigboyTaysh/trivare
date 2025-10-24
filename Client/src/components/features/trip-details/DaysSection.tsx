import React from "react";

interface DaysSectionProps {
  tripId: string;
}

const DaysSection: React.FC<DaysSectionProps> = () => {
  return (
    <div className="p-4 border rounded-lg">
      <h3 className="text-lg font-semibold mb-4">Days</h3>
      <p>Days section - to be implemented</p>
    </div>
  );
};

export default DaysSection;
