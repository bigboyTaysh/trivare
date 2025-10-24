import React from "react";

interface TransportSectionProps {
  tripId: string;
}

const TransportSection: React.FC<TransportSectionProps> = () => {
  return (
    <div className="p-4 border rounded-lg">
      <h3 className="text-lg font-semibold mb-4">Transport</h3>
      <p>Transport section - to be implemented</p>
    </div>
  );
};

export default TransportSection;
