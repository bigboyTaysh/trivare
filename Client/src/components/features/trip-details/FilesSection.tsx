import React, { useState, useEffect, useCallback } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { api } from "@/services/api";
import type { FileDto } from "@/types/trips";
import { toast } from "sonner";

interface FilesSectionProps {
  tripId: string;
}

const FilesSection: React.FC<FilesSectionProps> = ({ tripId }) => {
  const [files, setFiles] = useState<FileDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);

  const loadFiles = useCallback(async () => {
    setIsLoading(true);
    try {
      const files = await api.getTripFiles(tripId);
      setFiles(files);
    } catch {
      toast.error("Failed to load files");
      setFiles([]); // Set empty array on error
    } finally {
      setIsLoading(false);
    }
  }, [tripId]);

  useEffect(() => {
    loadFiles();
  }, [loadFiles]);

  const handleFileUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validation
    const allowedTypes = ["image/png", "image/jpeg", "application/pdf"];
    if (!allowedTypes.includes(file.type)) {
      toast.error("Only PNG, JPEG, and PDF files are allowed");
      return;
    }

    if (file.size > 5 * 1024 * 1024) {
      // 5MB
      toast.error("File size must be less than 5MB");
      return;
    }

    if (files.length >= 10) {
      toast.error("Maximum 10 files per trip");
      return;
    }

    setIsUploading(true);
    try {
      await api.uploadTripFile(tripId, file);
      toast.success("File uploaded successfully");
      loadFiles(); // Refresh list
    } catch {
      toast.error("Failed to upload file");
    } finally {
      setIsUploading(false);
    }
  };

  const handleFileDelete = async (fileId: string) => {
    try {
      await api.deleteFile(fileId);
      toast.success("File deleted successfully");
      setFiles(files.filter((f) => f.id !== fileId));
    } catch {
      toast.error("Failed to delete file");
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Files</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          <div>
            <input
              type="file"
              accept=".png,.jpg,.jpeg,.pdf"
              onChange={handleFileUpload}
              disabled={isUploading || files.length >= 10}
              className="hidden"
              id="file-upload"
            />
            <label htmlFor="file-upload" className="cursor-pointer">
              <Button asChild disabled={isUploading || files.length >= 10}>
                <span>{isUploading ? "Uploading..." : "Upload File"}</span>
              </Button>
              <span className="sr-only">Upload trip file</span>
            </label>
            <p className="text-sm text-muted-foreground mt-1">PNG, JPEG, PDF up to 5MB. Max 10 files.</p>
          </div>

          {isLoading ? (
            <p>Loading files...</p>
          ) : (
            <div className="space-y-2">
              {files.map((file) => (
                <div key={file.id} className="flex items-center justify-between p-2 border rounded">
                  <div>
                    <p className="font-medium">{file.fileName}</p>
                    <p className="text-sm text-muted-foreground">
                      {(file.fileSize / 1024).toFixed(1)} KB • {file.fileType}
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <Button size="sm" variant="outline" onClick={() => window.open(file.downloadUrl, "_blank")}>
                      Download
                    </Button>
                    <Button size="sm" variant="destructive" onClick={() => handleFileDelete(file.id)}>
                      Delete
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
};

export default FilesSection;
