import { useState, useEffect, useCallback } from "react";
import { api } from "@/services/api";
import type { FileDto } from "@/types/trips";

interface UseTripFilesReturn {
  totalFileCount: number;
  isLoading: boolean;
  refreshFileCount: () => Promise<void>;
  allFiles: FileDto[];
}

export const useTripFiles = (tripId: string): UseTripFilesReturn => {
  const [allFiles, setAllFiles] = useState<FileDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const refreshFileCount = useCallback(async () => {
    if (!tripId) return;

    setIsLoading(true);
    try {
      const files = await api.getTripFiles(tripId);
      setAllFiles(files);
    } catch {
      setAllFiles([]);
    } finally {
      setIsLoading(false);
    }
  }, [tripId]);

  useEffect(() => {
    refreshFileCount();
  }, [refreshFileCount]);

  return {
    totalFileCount: allFiles.length,
    isLoading,
    refreshFileCount,
    allFiles,
  };
};
