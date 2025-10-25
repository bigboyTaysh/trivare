import React, { useState, useEffect, useCallback, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { api } from "@/services/api";
import type { FileDto } from "@/types/trips";
import { toast } from "sonner";
import { Trash2 } from "lucide-react";

interface FilesSectionProps {
  tripId: string;
}

const FilesSection: React.FC<FilesSectionProps> = ({ tripId }) => {
  const [files, setFiles] = useState<FileDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);

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
    setUploadProgress(0);
    try {
      await api.uploadTripFile(tripId, file, (progress) => {
        setUploadProgress(progress);
      });
      toast.success("File uploaded successfully");
      loadFiles(); // Refresh list
    } catch {
      toast.error("Failed to upload file");
    } finally {
      setIsUploading(false);
      setUploadProgress(0);
      // Reset the file input to allow uploading the same file again
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
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
      <CardContent>
        <div className="space-y-2">
          <div>
            <input
              type="file"
              accept=".png,.jpg,.jpeg,.pdf"
              onChange={handleFileUpload}
              disabled={isUploading || files.length >= 10}
              className="hidden"
              id="file-upload"
              ref={fileInputRef}
            />
            <label
              htmlFor="file-upload"
              className={isUploading || files.length >= 10 ? "cursor-not-allowed" : "cursor-pointer"}
            >
              <Button
                asChild
                disabled={isUploading || files.length >= 10}
                className={files.length >= 10 ? "opacity-50 bg-gray-100 hover:bg-gray-100 text-gray-500" : ""}
              >
                <span>{isUploading ? "Uploading..." : files.length >= 10 ? "File Limit Reached" : "Upload File"}</span>
              </Button>
              <span className="sr-only">Upload trip file</span>
            </label>
            <p className="text-sm text-muted-foreground mt-1">
              PNG, JPEG, PDF up to 5MB. {files.length}/10 files uploaded.
            </p>
            {files.length >= 8 && files.length < 10 && (
              <p className="text-sm text-amber-600 mt-1">
                Approaching file limit ({files.length}/10). Consider deleting unused files.
              </p>
            )}
            {files.length >= 10 && (
              <p className="text-sm text-amber-600 mt-1">
                Maximum file limit reached. Delete some files to upload more.
              </p>
            )}
            {isUploading && (
              <div className="mt-2">
                <Progress value={uploadProgress} className="w-full" />
                <p className="text-sm text-muted-foreground mt-1">{uploadProgress}% uploaded</p>
              </div>
            )}
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
                    <Button size="icon" variant="ghost" onClick={() => handleFileDelete(file.id)} className="h-8 w-8">
                      <Trash2 className="h-4 w-4" />
                      <span className="sr-only">Delete file</span>
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
