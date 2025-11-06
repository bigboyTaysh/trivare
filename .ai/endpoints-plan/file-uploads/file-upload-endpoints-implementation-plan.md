# API Endpoint Implementation Plan: File Upload Endpoints

## 1. Endpoint Overview
This plan covers the implementation of four file upload endpoints that allow authenticated users to upload files (PNG, JPEG, or PDF, maximum 5MB) associated with different trip-related entities. The endpoints enforce a business rule of maximum 10 files per trip across all entities. Files are stored in Cloudflare R2 storage with metadata saved in Azure SQL.

## 2. Request Details
- **HTTP Method:** `POST`
- **URL Structures:**
  - `/api/trips/{tripId}/files` - Upload file to trip
  - `/api/transports/{transportId}/files` - Upload file to transport
  - `/api/accommodations/{accommodationId}/files` - Upload file to accommodation
  - `/api/days/{dayId}/files` - Upload file to day
- **Authentication:** Required (JWT)
- **Content-Type:** `multipart/form-data`
- **Parameters:**
  - **Required:**
    - `file` (file) - The file to upload
    - Path parameter: `tripId` (GUID), `dayId` (GUID), `transportId` (GUID) or `accommodationId` (GUID)
  - **Optional:** None
- **Request Body:** Multipart form data with single file field

## 3. Used Types
- **Existing DTOs:**
  - `FileDto` - For response metadata
- **New DTOs needed:**
  - `FileUploadResponse` - Response after successful upload (extends FileDto with URLs)
- **Command Models:** None (multipart upload doesn't use JSON commands)

## 4. Response Details
- **Success Response (201 Created):**
```json
{
  "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
  "fileName": "flight-ticket.pdf",
  "fileSize": 245678,
  "fileType": "application/pdf",
  "filePath": "trips/3fa85f64-5717-4562-b3fc-2c963f66afa6/flight-ticket.pdf",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "createdAt": "2025-06-01T10:30:00Z",
  "previewUrl": "/api/files/7fa85f64-5717-4562-b3fc-2c963f66afaa/preview",
  "downloadUrl": "/api/files/7fa85f64-5717-4562-b3fc-2c963f66afaa/download"
}
```
- **Status Codes:**
  - `201 Created` - File uploaded successfully
  - `400 Bad Request` - Invalid file type, size exceeds 5MB, or missing file
  - `403 Forbidden` - User does not own the trip/day
  - `404 Not Found` - Trip or day not found
  - `409 Conflict` - Trip has reached the 10-file limit
  - `500 Internal Server Error` - Storage or database error

## 5. Data Flow
1. **Authentication:** Validate JWT token and extract UserId
2. **Authorization:** Verify user owns the parent entity (trip/day) using RLS
3. **File Validation:** Check file presence, MIME type, size, and sanitize filename
4. **Business Rule Check:** Count existing files for the trip (< 10)
5. **Storage Upload:** Upload file to Cloudflare R2 with unique path
6. **Database Save:** Create File entity record with metadata
7. **Audit Logging:** Log FileUploaded event to AuditLog
8. **Response:** Return file metadata with preview/download URLs

## 6. Security Considerations
- **Authentication:** JWT required for all endpoints
- **Authorization:** Row-level security ensures users can only upload to their own trips/days
- **File Validation:** Strict MIME type checking (server-side, not just client)
- **Path Security:** Sanitize filenames to prevent path traversal attacks
- **Size Limits:** Enforce 5MB maximum to prevent DoS attacks
- **Storage Security:** Use Cloudflare R2 with proper access controls
- **Audit Trail:** Log all uploads for monitoring and abuse detection

## 7. Error Handling
- **File Validation Errors (400):**
  - Missing file in request
  - Unsupported MIME type (not PNG/JPEG/PDF)
  - File size exceeds 5MB
  - Invalid filename (path traversal attempts)
- **Authorization Errors (403):**
  - User attempting to upload to another user's trip/day/accommodation/transoport
- **Not Found Errors (404):**
  - Invalid tripId or dayId
  - Trip/day/accommodation/transoport exists but user doesn't own it (handled by RLS)
- **Business Rule Violations (409):**
  - Trip already has 10 files across all entities
- **System Errors (500):**
  - Cloudflare R2 upload failure
  - Database save failure
  - Audit logging failure (non-blocking)

## 8. Performance Considerations
- **File Processing:** Stream files directly to R2 without loading entire file in memory
- **Database Queries:** Use efficient queries for file count checks
- **Concurrent Uploads:** Handle multiple simultaneous uploads per user
- **Storage Optimization:** Use appropriate R2 storage class for cost efficiency
- **Response Time Target:** < 5 seconds for 5MB file uploads
- **Rate Limiting:** Consider per-user upload rate limits to prevent abuse

## 9. Implementation Steps
1. **Create File Service Interface** (`IFileService.cs`)
   - Define methods: `UploadFileAsync()`, `ValidateFile()`, `CheckFileLimit()`
   
2. **Implement File Service** (`FileService.cs`)
   - Inject required dependencies (file storage, repository, audit service)
   - Implement file validation logic
   - Implement R2 upload logic
   - Implement database save logic
   - Implement audit logging

3. **Create File Controller** (`FilesController.cs`)
   - Implement four upload action methods
   - Handle multipart form data parsing
   - Call service methods and handle responses
   - Apply proper authorization attributes

4. **Add DTOs**
   - Create `FileUploadResponse.cs` extending `FileDto` with URLs

5. **Update Infrastructure**
   - Add Cloudflare R2 client configuration
   - Add file storage interface and implementation

6. **Add Repository Methods**
   - Add file counting methods to repository
   - Ensure proper transaction handling

7. **Update Dependency Injection**
   - Register file service and storage interfaces

8. **Add Validation Attributes**
   - Create custom validation for file uploads
   - Add model validation to controller actions

9. **Documentation**
    - Update OpenAPI/Swagger documentation
    - Add file upload examples
    - Document error responses and codes