import { useState, useEffect } from 'react';
import { api } from '../services/api';

interface ApiResponse {
  message: string;
  timestamp: string;
}

export default function DataFetcher() {
  const [data, setData] = useState<ApiResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.getData()
      .then((response: unknown) => {
        const result = response as ApiResponse;
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        const message = err instanceof Error ? err.message : String(err);
        setError(message);
        setLoading(false);
      });
  }, []);

  if (loading) {
    return <div className="p-4 text-blue-600">Loading...</div>;
  }

  if (error) {
    return <div className="p-4 text-red-600">Error: {error}</div>;
  }

  return (
    <div className="p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold mb-4">Data from .NET Backend</h2>
      <p className="text-gray-700">{data?.message}</p>
      <p className="text-sm text-gray-500 mt-2">
        Timestamp: {new Date(data?.timestamp || '').toLocaleString()}
      </p>
    </div>
  );
}