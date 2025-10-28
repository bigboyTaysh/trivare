import React, { useState, useEffect, useCallback, useRef } from "react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/progress";
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from "@/components/ui/accordion";
import { api } from "@/services/api";
import type { FileDto } from "@/types/trips";
import { toast } from "sonner";
import { Trash2, FileText } from "lucide-react";

interface FilesSectionProps {
  entityId: string;
  entityType: "trip" | "accommodation" | "transport" | "day";
  title?: string;
}

const FilesSection: React.FC<FilesSectionProps> = ({ entityId, entityType, title = "Files" }) => {
  const [files, setFiles] = useState<FileDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const loadFiles = useCallback(async () => {
    setIsLoading(true);
    try {
      let files: FileDto[];
      switch (entityType) {
        case "trip":
          files = await api.getTripFiles(entityId);
          break;
        case "accommodation":
          files = await api.getAccommodationFiles(entityId);
          break;
        case "transport":
          files = await api.getTransportFiles(entityId);
          break;
        case "day":
          // TODO: Implement when day files are added
          files = [];
          break;
        default:
          files = [];
      }
      setFiles(files);
    } catch {
      toast.error(`Failed to load ${title.toLowerCase()}`);
      setFiles([]);
    } finally {
      setIsLoading(false);
    }
  }, [entityId, entityType, title]);

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
      toast.error("File size must be less than 5MB");
      return;
    }

    if (files.length >= 10) {
      toast.error(`Maximum 10 files per ${entityType}`);
      return;
    }

    setIsUploading(true);
    setUploadProgress(0);
    try {
      switch (entityType) {
        case "trip":
          await api.uploadTripFile(entityId, file, (progress) => {
            setUploadProgress(progress);
          });
          break;
        case "accommodation":
          await api.uploadAccommodationFile(entityId, file, (progress) => {
            setUploadProgress(progress);
          });
          break;
        case "transport":
          await api.uploadTransportFile(entityId, file, (progress) => {
            setUploadProgress(progress);
          });
          break;
        case "day":
          // TODO: Implement when day files are added
          throw new Error("Day file upload not implemented");
        default:
          throw new Error("Unknown entity type");
      }
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
    <Accordion type="single" collapsible className="border-none">
      <AccordionItem value="files" className="border-none">
        <AccordionTrigger className="py-2 hover:no-underline">
          <div className="flex items-center gap-2">
            <FileText className="h-4 w-4" />
            <span className="text-sm font-medium">Files</span>
            <span className="text-xs text-muted-foreground">({files.length}/10)</span>
          </div>
        </AccordionTrigger>
        <AccordionContent className="pb-2">
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <input
                type="file"
                accept=".png,.jpg,.jpeg,.pdf"
                onChange={handleFileUpload}
                disabled={isUploading || files.length >= 10}
                className="hidden"
                id={`file-upload-${entityType}-${entityId}`}
                ref={fileInputRef}
              />
              <label
                htmlFor={`file-upload-${entityType}-${entityId}`}
                className={isUploading || files.length >= 10 ? "cursor-not-allowed" : "cursor-pointer"}
              >
                <Button
                  asChild
                  size="sm"
                  disabled={isUploading || files.length >= 10}
                  className={files.length >= 10 ? "opacity-50 bg-gray-100 hover:bg-gray-100 text-gray-500" : ""}
                >
                  <span>{isUploading ? "Uploading..." : files.length >= 10 ? "Limit Reached" : "Upload"}</span>
                </Button>
                <span className="sr-only">Upload {entityType} file</span>
              </label>
              <span className="text-xs text-muted-foreground">PNG, JPEG, PDF (max 5MB)</span>
            </div>

            {isUploading && (
              <div className="space-y-1">
                <Progress value={uploadProgress} className="h-1" />
                <p className="text-xs text-muted-foreground">{uploadProgress}%</p>
              </div>
            )}

            {files.length >= 10 && <p className="text-xs text-amber-600">Delete files to upload more</p>}

            {isLoading ? (
              <p className="text-xs text-muted-foreground">Loading...</p>
            ) : files.length > 0 ? (
              <div className="space-y-1">
                {files.map((file) => (
                  <div key={file.id} className="flex items-center justify-between gap-2 p-1.5 border rounded text-xs">
                    <div className="flex-1 min-w-0">
                      <p className="font-medium truncate">{file.fileName}</p>
                      <p className="text-muted-foreground">{(file.fileSize / 1024).toFixed(1)} KB</p>
                    </div>
                    <div className="flex gap-1 shrink-0">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => window.open(file.downloadUrl, "_blank")}
                        className="h-6 px-2 text-xs"
                      >
                        View
                      </Button>
                      <Button size="icon" variant="ghost" onClick={() => handleFileDelete(file.id)} className="h-6 w-6">
                        <Trash2 className="h-3 w-3" />
                        <span className="sr-only">Delete file</span>
                      </Button>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-xs text-muted-foreground">No files uploaded yet</p>
            )}
          </div>
        </AccordionContent>
      </AccordionItem>
    </Accordion>
  );
};

export default FilesSection;
