# REST API Plan for Trivare

## 1. Resources

The API is organized around the following main resources, mapped to database tables:

| Resource | Database Table | Description |
|----------|---------------|-------------|
| Auth | Users, Roles, UserRoles | Authentication and authorization operations |
| Users | Users | User profile management |
| Trips | Trips | Core trip information and planning |
| Transport | Transport | Transportation details for trips (one-to-one with Trip) |
| Accommodation | Accommodation | Accommodation details for trips (one-to-one with Trip) |
| Days | Days | Individual days within trips |
| Places | Places | Attractions and points of interest |
| DayAttractions | DayAttractions | Links places to specific days |
| Files | Files | User-uploaded documents and images |
| Admin | AuditLog | Administrative metrics and audit logs |

## 2. Endpoints

### 2.1 Authentication (`/api/auth`)

#### 2.1.1 Register

- **Method:** `POST`
- **Path:** `/api/auth/register`
- **Description:** Register a new user account
- **Authentication:** None
- **Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```
- **Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "createdAt": "2025-10-12T10:30:00Z"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid email format or weak password
  - `409 Conflict` - Email already exists

---

#### 2.1.2 Login

- **Method:** `POST`
- **Path:** `/api/auth/login`
- **Description:** Authenticate user and receive JWT tokens
- **Authentication:** None
- **Request Body:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```
- **Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com"
  }
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid credentials

---

#### 2.1.3 Refresh Token

- **Method:** `POST`
- **Path:** `/api/auth/refresh`
- **Description:** Obtain new access token using refresh token
- **Authentication:** None (refresh token in body)
- **Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```
- **Response (200 OK):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 900
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or expired refresh token

---

#### 2.1.4 Logout

- **Method:** `POST`
- **Path:** `/api/auth/logout`
- **Description:** Invalidate the refresh token to log the user out. The access token will expire on its own.
- **Authentication:** None (refresh token in body)
- **Request Body:**
```json
{
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```
- **Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid refresh token provided.

---

#### 2.1.5 Forgot Password

- **Method:** `POST`
- **Path:** `/api/auth/forgot-password`
- **Description:** Initiate password reset process (sends email with reset token)
- **Authentication:** None
- **Request Body:**
```json
{
  "email": "user@example.com"
}
```
- **Response (200 OK):**
```json
{
  "message": "Password reset link sent to your email"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid email format
  - `404 Not Found` - Email not found (Note: May return 200 for security)

---

#### 2.1.5 Reset Password

- **Method:** `POST`
- **Path:** `/api/auth/reset-password`
- **Description:** Reset password using token from email
- **Authentication:** None (token in body)
- **Request Body:**
```json
{
  "token": "reset-token-from-email",
  "newPassword": "NewSecurePassword123!"
}
```
- **Response (200 OK):**
```json
{
  "message": "Password reset successful"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid or expired token, or weak password
  - `404 Not Found` - Token not found

---

### 2.2 User Profile (`/api/users`)

#### 2.2.1 Get Current User

- **Method:** `GET`
- **Path:** `/api/users/me`
- **Description:** Get current authenticated user profile
- **Authentication:** Required (JWT)
- **Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "createdAt": "2025-10-01T10:30:00Z",
  "roles": ["User"]
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token

---

#### 2.2.2 Update Current User

- **Method:** `PATCH`
- **Path:** `/api/users/me`
- **Description:** Update current user profile
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "email": "newemail@example.com"
}
```
- **Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "newemail@example.com",
  "createdAt": "2025-10-01T10:30:00Z",
  "roles": ["User"]
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid email format
  - `409 Conflict` - Email already exists

---

#### 2.2.3 Delete Current User (GDPR)

- **Method:** `DELETE`
- **Path:** `/api/users/me`
- **Description:** Permanently delete user account and all associated data
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token

---

### 2.3 Trips (`/api/trips`)

#### 2.3.1 List Trips

- **Method:** `GET`
- **Path:** `/api/trips`
- **Description:** Get paginated list of user's trips
- **Authentication:** Required (JWT)
- **Query Parameters:**
  - `page` (integer, default: 1) - Page number
  - `pageSize` (integer, default: 10, max: 50) - Items per page
  - `sortBy` (string, default: "createdAt") - Sort field (createdAt, name, startDate)
  - `sortOrder` (string, default: "desc") - Sort order (asc, desc)
  - `search` (string, optional) - Search in name and destination
- **Response (200 OK):**
```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Summer Vacation",
      "destination": "Paris, France",
      "startDate": "2025-07-01",
      "endDate": "2025-07-10",
      "notes": "Family trip to Paris",
      "createdAt": "2025-06-01T10:30:00Z"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 5,
    "totalPages": 1
  }
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token

---

#### 2.3.2 Get Trip

- **Method:** `GET`
- **Path:** `/api/trips/{tripId}`
- **Description:** Get detailed trip information with optional related data
- **Authentication:** Required (JWT)
- **Query Parameters:**
  - `include` (string, optional) - Comma-separated list of related resources to include (days, transport, accommodation, files)
- **Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Summer Vacation",
  "destination": "Paris, France",
  "startDate": "2025-07-01",
  "endDate": "2025-07-10",
  "notes": "Family trip to Paris",
  "createdAt": "2025-06-01T10:30:00Z",
  "transport": {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "type": "Flight",
    "departureLocation": "New York JFK",
    "arrivalLocation": "Paris CDG",
    "departureTime": "2025-07-01T08:00:00Z",
    "arrivalTime": "2025-07-01T20:00:00Z",
    "notes": "Air France AF007"
  },
  "accommodation": {
    "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
    "name": "Hotel Eiffel",
    "address": "123 Rue de Paris, 75001 Paris",
    "checkInDate": "2025-07-01T15:00:00Z",
    "checkOutDate": "2025-07-10T11:00:00Z",
    "notes": "Booking confirmation #123456"
  },
  "days": [
    {
      "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
      "date": "2025-07-01",
      "notes": "Arrival day"
    }
  ],
  "files": [
    {
      "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
      "fileName": "flight-ticket.pdf",
      "fileSize": 245678,
      "fileType": "application/pdf",
      "createdAt": "2025-06-01T10:30:00Z",
      "previewUrl": "/api/files/7fa85f64-5717-4562-b3fc-2c963f66afaa/preview",
      "downloadUrl": "/api/files/7fa85f64-5717-4562-b3fc-2c963f66afaa/download"
    }
  ]
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

#### 2.3.3 Create Trip

- **Method:** `POST`
- **Path:** `/api/trips`
- **Description:** Create a new trip
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "name": "Summer Vacation",
  "destination": "Paris, France",
  "startDate": "2025-07-01",
  "endDate": "2025-07-10",
  "notes": "Family trip to Paris"
}
```
- **Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Summer Vacation",
  "destination": "Paris, France",
  "startDate": "2025-07-01",
  "endDate": "2025-07-10",
  "notes": "Family trip to Paris",
  "createdAt": "2025-06-01T10:30:00Z"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data (e.g., endDate before startDate)
  - `409 Conflict` - User has reached the 10-trip limit
  ```json
  {
    "error": "TripLimitExceeded",
    "message": "You have reached the maximum limit of 10 trips",
    "currentTripCount": 10,
    "maxTripCount": 10
  }
  ```

---

#### 2.3.4 Update Trip

- **Method:** `PATCH`
- **Path:** `/api/trips/{tripId}`
- **Description:** Update trip information
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "name": "Summer Vacation Updated",
  "destination": "Paris and Lyon, France",
  "startDate": "2025-07-01",
  "endDate": "2025-07-12",
  "notes": "Extended family trip"
}
```
- **Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Summer Vacation Updated",
  "destination": "Paris and Lyon, France",
  "startDate": "2025-07-01",
  "endDate": "2025-07-12",
  "notes": "Extended family trip",
  "createdAt": "2025-06-01T10:30:00Z"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

#### 2.3.5 Delete Trip

- **Method:** `DELETE`
- **Path:** `/api/trips/{tripId}`
- **Description:** Delete a trip and all associated data (cascading delete)
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

### 2.4 Transport (`/api/trips/{tripId}/transport`)

#### 2.4.1 Add Transport

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/transport`
- **Description:** Add transportation details to a trip (one-to-one relationship)
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "type": "Flight",
  "departureLocation": "New York JFK",
  "arrivalLocation": "Paris CDG",
  "departureTime": "2025-07-01T08:00:00Z",
  "arrivalTime": "2025-07-01T20:00:00Z",
  "notes": "Air France AF007"
}
```
- **Response (201 Created):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "Flight",
  "departureLocation": "New York JFK",
  "arrivalLocation": "Paris CDG",
  "departureTime": "2025-07-01T08:00:00Z",
  "arrivalTime": "2025-07-01T20:00:00Z",
  "notes": "Air France AF007"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data (e.g., arrival before departure)
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found
  - `409 Conflict` - Transport already exists for this trip

---

#### 2.4.2 Update Transport

- **Method:** `PATCH`
- **Path:** `/api/trips/{tripId}/transport`
- **Description:** Update transportation details
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "type": "Flight",
  "departureTime": "2025-07-01T09:00:00Z",
  "arrivalTime": "2025-07-01T21:00:00Z"
}
```
- **Response (200 OK):**
```json
{
  "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": "Flight",
  "departureLocation": "New York JFK",
  "arrivalLocation": "Paris CDG",
  "departureTime": "2025-07-01T09:00:00Z",
  "arrivalTime": "2025-07-01T21:00:00Z",
  "notes": "Air France AF007"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip or transport not found

---

#### 2.4.3 Delete Transport

- **Method:** `DELETE`
- **Path:** `/api/trips/{tripId}/transport`
- **Description:** Remove transportation details from trip
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip or transport not found

---

### 2.5 Accommodation (`/api/trips/{tripId}/accommodation`)

#### 2.5.1 Add Accommodation

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/accommodation`
- **Description:** Add accommodation details to a trip (one-to-one relationship)
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "name": "Hotel Eiffel",
  "address": "123 Rue de Paris, 75001 Paris",
  "checkInDate": "2025-07-01T15:00:00Z",
  "checkOutDate": "2025-07-10T11:00:00Z",
  "notes": "Booking confirmation #123456"
}
```
- **Response (201 Created):**
```json
{
  "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Hotel Eiffel",
  "address": "123 Rue de Paris, 75001 Paris",
  "checkInDate": "2025-07-01T15:00:00Z",
  "checkOutDate": "2025-07-10T11:00:00Z",
  "notes": "Booking confirmation #123456"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data (e.g., checkout before checkin)
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found
  - `409 Conflict` - Accommodation already exists for this trip

---

#### 2.5.2 Update Accommodation

- **Method:** `PATCH`
- **Path:** `/api/trips/{tripId}/accommodation`
- **Description:** Update accommodation details
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "checkOutDate": "2025-07-12T11:00:00Z"
}
```
- **Response (200 OK):**
```json
{
  "id": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Hotel Eiffel",
  "address": "123 Rue de Paris, 75001 Paris",
  "checkInDate": "2025-07-01T15:00:00Z",
  "checkOutDate": "2025-07-12T11:00:00Z",
  "notes": "Booking confirmation #123456"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip or accommodation not found

---

#### 2.5.3 Delete Accommodation

- **Method:** `DELETE`
- **Path:** `/api/trips/{tripId}/accommodation`
- **Description:** Remove accommodation details from trip
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip or accommodation not found

---

### 2.6 Days (`/api/trips/{tripId}/days` and `/api/days`)

#### 2.6.1 List Days

- **Method:** `GET`
- **Path:** `/api/trips/{tripId}/days`
- **Description:** Get all days for a trip with optional places
- **Authentication:** Required (JWT)
- **Query Parameters:**
  - `include` (string, optional) - Include "places" to get attractions for each day
- **Response (200 OK):**
```json
{
  "data": [
    {
      "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
      "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "date": "2025-07-01",
      "notes": "Arrival day",
      "places": [
        {
          "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
          "name": "Eiffel Tower",
          "formattedAddress": "Champ de Mars, Paris",
          "website": "https://www.toureiffel.paris",
          "googleMapsLink": "https://maps.google.com/?cid=123456",
          "openingHoursText": "9:00 AM - 11:00 PM",
          "isManuallyAdded": false,
          "order": 1,
          "isVisited": false
        }
      ]
    }
  ]
}
```
- **Error Responses:**
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

#### 2.6.2 Create Day

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/days`
- **Description:** Add a new day to the trip
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "date": "2025-07-01",
  "notes": "Arrival day - explore nearby area"
}
```
- **Response (201 Created):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2025-07-01",
  "notes": "Arrival day - explore nearby area"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid date or date outside trip range
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

#### 2.6.3 Update Day

- **Method:** `PATCH`
- **Path:** `/api/days/{dayId}`
- **Description:** Update day information
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "date": "2025-07-01",
  "notes": "Arrival day - updated notes"
}
```
- **Response (200 OK):**
```json
{
  "id": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "date": "2025-07-01",
  "notes": "Arrival day - updated notes"
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day not found

---

#### 2.6.4 Delete Day

- **Method:** `DELETE`
- **Path:** `/api/days/{dayId}`
- **Description:** Remove a day from trip
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day not found

---

### 2.7 Places (`/api/places` and `/api/days/{dayId}/places`)

#### 2.7.1 Search Places (AI-Powered)

- **Method:** `POST`
- **Path:** `/api/places/search`
- **Description:** Search for places using Google Places API with AI filtering and ranking
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "location": "Paris, France",
  "keyword": "restaurants with local cuisine",
  "preferences": "vegetarian-friendly, outdoor seating"
}
```
- **Response (200 OK):**
```json
{
  "results": [
    {
      "googlePlaceId": "ChIJD7fiBh9u5kcRYJSMaMOCCwQ",
      "name": "Le Potager du Marais",
      "formattedAddress": "22 Rue Rambuteau, 75003 Paris",
      "website": "https://www.lepotagerdumarais.fr",
      "googleMapsLink": "https://maps.google.com/?cid=123456",
      "openingHoursText": "12:00 PM - 11:00 PM",
      "aiRecommendation": "Excellent vegetarian restaurant with outdoor seating in Le Marais district"
    }
  ],
  "count": 5
}
```
- **Error Responses:**
  - `400 Bad Request` - Missing or invalid search parameters
  - `500 Internal Server Error` - Google Places API or AI service error

**Note:** This endpoint logs a search event to the AuditLog table for metrics tracking.

---

#### 2.7.2 Add Place to Day

- **Method:** `POST`
- **Path:** `/api/days/{dayId}/places`
- **Description:** Add a place (attraction) to a specific day
- **Authentication:** Required (JWT)
- **Request Body (existing place):**
```json
{
  "placeId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "order": 1
}
```
**OR Request Body (new manual place):**
```json
{
  "place": {
    "name": "Local Bakery",
    "formattedAddress": "10 Rue du Pain, Paris",
    "website": "https://bakery.example.com",
    "openingHoursText": "7:00 AM - 7:00 PM",
    "isManuallyAdded": true
  },
  "order": 2
}
```
- **Response (201 Created):**
```json
{
  "dayId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "place": {
    "id": "8fa85f64-5717-4562-b3fc-2c963f66afab",
    "name": "Local Bakery",
    "formattedAddress": "10 Rue du Pain, Paris",
    "website": "https://bakery.example.com",
    "openingHoursText": "7:00 AM - 7:00 PM",
    "isManuallyAdded": true
  },
  "order": 2,
  "isVisited": false
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid place data
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day or place not found
  - `409 Conflict` - Place already added to this day

---

#### 2.7.3 Update Place on Day

- **Method:** `PATCH`
- **Path:** `/api/days/{dayId}/places/{placeId}`
- **Description:** Update place order or visited status
- **Authentication:** Required (JWT)
- **Request Body:**
```json
{
  "order": 3,
  "isVisited": true
}
```
- **Response (200 OK):**
```json
{
  "dayId": "6fa85f64-5717-4562-b3fc-2c963f66afa9",
  "placeId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "order": 3,
  "isVisited": true
}
```
- **Error Responses:**
  - `400 Bad Request` - Invalid data
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day or place association not found

---

#### 2.7.4 Remove Place from Day

- **Method:** `DELETE`
- **Path:** `/api/days/{dayId}/places/{placeId}`
- **Description:** Remove a place from a day's itinerary
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day or place association not found

---

### 2.8 Files (`/api/trips/{tripId}/files`, `/api/days/{dayId}/files`, `/api/files`)

#### 2.8.1 Upload File to Trip

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/files`
- **Description:** Upload a file associated with a trip
- **Authentication:** Required (JWT)
- **Content-Type:** `multipart/form-data`
- **Request Body:**
  - `file` (file) - The file to upload (PNG, JPEG, or PDF, max 5MB)
- **Response (201 Created):**
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
- **Error Responses:**
  - `400 Bad Request` - Invalid file type or size exceeds 5MB
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found
  - `409 Conflict` - Trip has reached the 10-file limit
  ```json
  {
    "error": "FileLimitExceeded",
    "message": "This trip has reached the maximum limit of 10 files",
    "currentFileCount": 10,
    "maxFileCount": 10
  }
  ```

---

#### 2.8.2 Upload File to Transport

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/transport/files`
- **Description:** Upload a file associated with trip transport
- **Authentication:** Required (JWT)
- **Content-Type:** `multipart/form-data`
- **Request Body:**
  - `file` (file) - The file to upload
- **Response:** Same structure as 2.8.1 but with `transportId` instead of `tripId`
- **Error Responses:** Same as 2.8.1

---

#### 2.8.3 Upload File to Accommodation

- **Method:** `POST`
- **Path:** `/api/trips/{tripId}/accommodation/files`
- **Description:** Upload a file associated with trip accommodation
- **Authentication:** Required (JWT)
- **Content-Type:** `multipart/form-data`
- **Request Body:**
  - `file` (file) - The file to upload
- **Response:** Same structure as 2.8.1 but with `accommodationId` instead of `tripId`
- **Error Responses:** Same as 2.8.1

---

#### 2.8.4 Upload File to Day

- **Method:** `POST`
- **Path:** `/api/days/{dayId}/files`
- **Description:** Upload a file associated with a specific day
- **Authentication:** Required (JWT)
- **Content-Type:** `multipart/form-data`
- **Request Body:**
  - `file` (file) - The file to upload
- **Response:** Same structure as 2.8.1 but with `dayId` instead of `tripId`
- **Error Responses:** Same as 2.8.1

---

#### 2.8.5 List Files for Trip

- **Method:** `GET`
- **Path:** `/api/trips/{tripId}/files`
- **Description:** Get all files associated with a trip (including transport, accommodation, and days)
- **Authentication:** Required (JWT)
- **Response (200 OK):**
```json
{
  "data": [
    {
      "id": "7fa85f64-5717-4562-b3fc-2c963f66afaa",
      "fileName": "flight-ticket.pdf",
      "fileSize": 245678,
      "fileType": "application/pdf",
      "parentType": "trip",
      "parentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "createdAt": "2025-06-01T10:30:00Z"
    },
    {
      "id": "9fa85f64-5717-4562-b3fc-2c963f66afac",
      "fileName": "hotel-booking.pdf",
      "fileSize": 189234,
      "fileType": "application/pdf",
      "parentType": "accommodation",
      "parentId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
      "createdAt": "2025-06-02T14:20:00Z"
    }
  ],
  "count": 2,
  "maxFiles": 10
}
```
- **Error Responses:**
  - `403 Forbidden` - Trip belongs to another user
  - `404 Not Found` - Trip not found

---

#### 2.8.6 List Files for Day

- **Method:** `GET`
- **Path:** `/api/days/{dayId}/files`
- **Description:** Get all files associated with a specific day
- **Authentication:** Required (JWT)
- **Response (200 OK):**
```json
{
  "data": [
    {
      "id": "afa85f64-5717-4562-b3fc-2c963f66afad",
      "fileName": "museum-ticket.pdf",
      "fileSize": 156789,
      "fileType": "application/pdf",
      "createdAt": "2025-06-03T09:15:00Z",
      "previewUrl": "/api/files/afa85f64-5717-4562-b3fc-2c963f66afad/preview",
      "downloadUrl": "/api/files/afa85f64-5717-4562-b3fc-2c963f66afad/download"
    }
  ],
  "count": 1
}
```
- **Error Responses:**
  - `403 Forbidden` - Day belongs to another user's trip
  - `404 Not Found` - Day not found

---

#### 2.8.7 Preview File

- **Method:** `GET`
- **Path:** `/api/files/{fileId}/preview`
- **Description:** Preview a file in the browser (inline display)
- **Authentication:** Required (JWT)
- **Response (200 OK):**
  - Returns the file content with appropriate `Content-Type` header
  - Sets `Content-Disposition: inline` to display in browser
  - For images: Returns image with proper MIME type
  - For PDFs: Returns PDF for browser PDF viewer
- **Error Responses:**
  - `403 Forbidden` - File belongs to another user
  - `404 Not Found` - File not found

**Note:** This endpoint is optimized for browser preview. Images and PDFs will be displayed inline, while other file types may prompt download depending on browser capabilities.

---

#### 2.8.8 Download File

- **Method:** `GET`
- **Path:** `/api/files/{fileId}/download`
- **Description:** Download a file (forces download with attachment disposition)
- **Authentication:** Required (JWT)
- **Response (200 OK):**
  - Returns the file content with appropriate `Content-Type` header
  - Sets `Content-Disposition: attachment; filename="original-filename.ext"` to force download
- **Error Responses:**
  - `403 Forbidden` - File belongs to another user
  - `404 Not Found` - File not found

---

#### 2.8.9 Delete File

- **Method:** `DELETE`
- **Path:** `/api/files/{fileId}`
- **Description:** Delete a file from storage and database
- **Authentication:** Required (JWT)
- **Response (204 No Content)**
- **Error Responses:**
  - `403 Forbidden` - File belongs to another user
  - `404 Not Found` - File not found

---

### 2.9 Admin (`/api/admin`)

#### 2.9.1 Get Metrics

- **Method:** `GET`
- **Path:** `/api/admin/metrics`
- **Description:** Get system metrics for the last 60 days (admin only)
- **Authentication:** Required (JWT with Admin role)
- **Query Parameters:**
  - `days` (integer, default: 60, max: 365) - Number of days to look back
- **Response (200 OK):**
```json
{
  "period": {
    "startDate": "2025-08-13T00:00:00Z",
    "endDate": "2025-10-12T23:59:59Z",
    "days": 60
  },
  "metrics": {
    "totalUsers": 142,
    "totalTrips": 89,
    "newTripsInPeriod": 23,
    "totalPlaceSearches": 156,
    "aiSearchesInPeriod": 156,
    "totalFiles": 234,
    "filesUploadedInPeriod": 67,
    "activeUsers": 45
  }
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token
  - `403 Forbidden` - User does not have Admin role

---

#### 2.9.2 Get Audit Logs

- **Method:** `GET`
- **Path:** `/api/admin/audit-logs`
- **Description:** Get paginated audit logs (admin only)
- **Authentication:** Required (JWT with Admin role)
- **Query Parameters:**
  - `page` (integer, default: 1) - Page number
  - `pageSize` (integer, default: 50, max: 100) - Items per page
  - `eventType` (string, optional) - Filter by event type
  - `userId` (string, optional) - Filter by user ID
  - `startDate` (string, optional) - Filter from date (ISO 8601)
  - `endDate` (string, optional) - Filter to date (ISO 8601)
- **Response (200 OK):**
```json
{
  "data": [
    {
      "id": 12345,
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "eventType": "PlaceSearch",
      "eventTimestamp": "2025-10-12T14:30:00Z",
      "details": "Searched for restaurants in Paris"
    },
    {
      "id": 12344,
      "userId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "eventType": "TripCreated",
      "eventTimestamp": "2025-10-12T13:15:00Z",
      "details": "Trip 'Weekend Getaway' created"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 50,
    "totalItems": 1247,
    "totalPages": 25
  }
}
```
- **Error Responses:**
  - `401 Unauthorized` - Invalid or missing token
  - `403 Forbidden` - User does not have Admin role

---

## 3. Authentication and Authorization

### 3.1 Authentication Mechanism

The API uses **JWT (JSON Web Tokens)** for authentication with a dual-token approach:

#### Access Token
- **Purpose:** Authorize API requests
- **Lifetime:** 15 minutes
- **Storage:** Client-side memory (not localStorage for security)
- **Claims:**
  - `sub` (subject): User ID (GUID)
  - `email`: User email
  - `role`: User role(s) - "User" or "Admin"
  - `exp` (expiration): Token expiration timestamp
  - `iat` (issued at): Token creation timestamp

#### Refresh Token
- **Purpose:** Obtain new access tokens
- **Lifetime:** 7 days
- **Storage:** Secure, HTTP-only cookie
- **Claims:**
  - `sub` (subject): User ID (GUID)
  - `exp` (expiration): Token expiration timestamp
  - `tokenId`: Unique token identifier for revocation

### 3.2 Authentication Flow

1. **Registration:** User registers with email/password → API creates user with hashed password → Returns user data (no automatic login)
2. **Login:** User submits credentials → API validates → Returns access token + refresh token
3. **API Request:** Client includes access token in `Authorization: Bearer <token>` header → API validates token → Extracts UserId and sets SESSION_CONTEXT → Executes request with RLS
4. **Token Refresh:** When access token expires → Client sends refresh token to `/api/auth/refresh` → API validates refresh token → Returns new access token + refresh token
5. **Logout:** Client discards tokens (API can implement token blacklist for stricter security)

### 3.3 Authorization Levels

#### Public Endpoints (No Authentication Required)
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`

#### Authenticated User Endpoints (Valid JWT Required)
- All `/api/users/*` endpoints
- All `/api/trips/*` endpoints
- All `/api/days/*` endpoints
- All `/api/places/*` endpoints
- All `/api/files/*` endpoints

#### Admin-Only Endpoints (JWT + Admin Role Required)
- All `/api/admin/*` endpoints

### 3.4 Row-Level Security (RLS) Implementation

The API leverages Azure SQL's Row-Level Security to ensure data isolation:

1. **Session Context Setup:** On every authenticated request, the API extracts the `UserId` from the JWT and executes:
   ```sql
   EXEC sp_set_session_context 'UserId', '<userId-from-jwt>', @read_only = 1;
   ```

2. **RLS Predicate Function:** The database uses a predicate function that checks:
   - If the row's `UserId` matches the session's `UserId`, OR
   - If the user has the "Admin" role (via `UserRoles` join)

3. **Security Policies:** Applied to tables: `Trips`, `Days`, `Files`, `Transport`, `Accommodation`
   - **Filter Predicate:** Automatically filters SELECT queries
   - **Block Predicate:** Prevents INSERT/UPDATE/DELETE on unauthorized rows

4. **Benefits:**
   - Defense in depth - even if API logic is bypassed, database enforces access control
   - Simplified API code - no need for manual WHERE UserId = ... clauses
   - Audit-friendly - all access patterns are logged

### 3.5 Security Best Practices

1. **Password Security:**
   - Minimum length: 8 characters
   - Require: uppercase, lowercase, number, special character
   - Hash using BCrypt with salt (cost factor: 12)
   - Never log or return password hashes

2. **Token Security:**
   - Sign with HS256 or RS256 algorithm
   - Use strong secret key (min 256 bits)
   - Implement token refresh rotation (new refresh token on each refresh)
   - Consider implementing refresh token revocation list

3. **HTTPS Only:**
   - All API endpoints must use HTTPS in production
   - Set `Secure` flag on refresh token cookie
   - Enable HSTS (HTTP Strict Transport Security)

4. **CORS Configuration:**
   - Allow only frontend domain (e.g., https://trivare.app)
   - Do not use wildcard (*) in production
   - Include credentials for cookie-based refresh tokens

5. **Rate Limiting:**
   - Auth endpoints: 5 requests per minute per IP
   - Search endpoints: 10 requests per minute per user
   - General endpoints: 100 requests per minute per user
   - File upload: 10 requests per hour per user

6. **Input Validation:**
   - Validate all inputs against schema
   - Sanitize file names before storage
   - Validate file MIME types (not just extension)
   - Limit request body size (5MB for files, 1MB for JSON)

7. **File Serving Security:**
   - Set appropriate `Content-Type` headers to prevent MIME sniffing
   - Use `X-Content-Type-Options: nosniff` header
   - For preview endpoint, use `Content-Disposition: inline`
   - For download endpoint, use `Content-Disposition: attachment`
   - Validate file content matches declared MIME type before serving

---

## 4. Validation and Business Logic

### 4.1 Validation Rules by Resource

#### Users
- **Email:**
  - Required on registration
  - Must be valid email format (RFC 5322)
  - Maximum length: 255 characters
  - Must be unique across all users
- **Password:**
  - Required on registration
  - Minimum length: 8 characters
  - Must contain: uppercase, lowercase, digit, special character
  - Maximum length: 128 characters (before hashing)

#### Trips
- **Name:**
  - Required
  - Maximum length: 255 characters
  - Minimum length: 1 character (after trim)
- **Destination:**
  - Optional
  - Maximum length: 255 characters
- **StartDate:**
  - Required
  - Must be valid date (ISO 8601 format: YYYY-MM-DD)
  - Can be in the past (for historical trips)
- **EndDate:**
  - Required
  - Must be valid date (ISO 8601 format: YYYY-MM-DD)
  - Must be >= StartDate
- **Notes:**
  - Optional
  - Maximum length: 2000 characters
- **Business Rule:**
  - User cannot have more than 10 trips
  - Validate on trip creation (POST)

#### Transport
- **Type:**
  - Optional
  - Maximum length: 100 characters
- **DepartureLocation:**
  - Optional
  - Maximum length: 255 characters
- **ArrivalLocation:**
  - Optional
  - Maximum length: 255 characters
- **DepartureTime:**
  - Optional
  - Must be valid ISO 8601 datetime
- **ArrivalTime:**
  - Optional
  - Must be valid ISO 8601 datetime
  - If both DepartureTime and ArrivalTime are provided, ArrivalTime must be > DepartureTime
- **Notes:**
  - Optional
  - Maximum length: 2000 characters
- **Business Rule:**
  - Only one transport per trip (one-to-one relationship)
  - Return 409 Conflict if transport already exists

#### Accommodation
- **Name:**
  - Optional
  - Maximum length: 255 characters
- **Address:**
  - Optional
  - Maximum length: 500 characters
- **CheckInDate:**
  - Optional
  - Must be valid ISO 8601 datetime
- **CheckOutDate:**
  - Optional
  - Must be valid ISO 8601 datetime
  - If both CheckInDate and CheckOutDate are provided, CheckOutDate must be >= CheckInDate
- **Notes:**
  - Optional
  - Maximum length: 2000 characters
- **Business Rule:**
  - Only one accommodation per trip (one-to-one relationship)
  - Return 409 Conflict if accommodation already exists

#### Days
- **Date:**
  - Required
  - Must be valid date (ISO 8601 format: YYYY-MM-DD)
  - Recommended: Should be within trip's StartDate and EndDate (warning, not error)
- **Notes:**
  - Optional
  - Maximum length: 2000 characters

#### Places
- **Name:**
  - Required
  - Maximum length: 255 characters
  - Minimum length: 1 character (after trim)
- **GooglePlaceId:**
  - Optional
  - Maximum length: 255 characters
  - Must be unique if provided
- **FormattedAddress:**
  - Optional
  - Maximum length: 500 characters
- **Website:**
  - Optional
  - Maximum length: 500 characters
  - Must be valid URL format if provided
- **GoogleMapsLink:**
  - Optional
  - Maximum length: 500 characters
  - Must be valid URL format if provided
- **OpeningHoursText:**
  - Optional
  - Maximum length: 1000 characters
- **IsManuallyAdded:**
  - Required (default: false for Google Places, true for manual)
  - Boolean value

#### DayAttractions
- **Order:**
  - Required
  - Must be integer >= 0
  - No need for uniqueness within a day (allows gaps)
- **IsVisited:**
  - Required (default: false)
  - Boolean value
- **Business Rule:**
  - A place cannot be added to the same day twice
  - Return 409 Conflict if duplicate

#### Files
- **File (upload):**
  - Required on upload
  - File type: Must be PNG, JPEG, or PDF
  - Accepted MIME types: `image/png`, `image/jpeg`, `application/pdf`
  - File size: Must be > 0 and <= 5MB (5,242,880 bytes)
- **FileName:**
  - Derived from uploaded file
  - Sanitized to prevent path traversal
  - Maximum length: 255 characters
- **Business Rule:**
  - Maximum 10 files per trip (across trip, transport, accommodation, and all days)
  - File must be associated with exactly one parent entity
  - Return 409 Conflict if limit exceeded
- **File Access:**
  - Preview: `GET /api/files/{fileId}/preview` - Display inline in browser
  - Download: `GET /api/files/{fileId}/download` - Force download with attachment disposition

### 4.2 Business Logic Implementation

#### Trip Limit Enforcement
**Endpoint:** `POST /api/trips`
**Logic:**
1. Extract UserId from JWT
2. Query database: `SELECT COUNT(*) FROM Trips WHERE UserId = @UserId`
3. If count >= 10, return 409 Conflict with error details
4. Otherwise, proceed with trip creation

**Error Response:**
```json
{
  "error": "TripLimitExceeded",
  "message": "You have reached the maximum limit of 10 trips. Please delete an existing trip to create a new one.",
  "currentTripCount": 10,
  "maxTripCount": 10
}
```

#### File Limit Enforcement
**Endpoints:** All file upload endpoints
**Logic:**
1. Determine the TripId for the file (direct for trip files, query for transport/accommodation/day files)
2. Query database:
   ```sql
   SELECT COUNT(*) FROM Files 
   WHERE TripId = @TripId 
      OR TransportId IN (SELECT Id FROM Transport WHERE TripId = @TripId)
      OR AccommodationId IN (SELECT Id FROM Accommodation WHERE TripId = @TripId)
      OR DayId IN (SELECT Id FROM Days WHERE TripId = @TripId)
   ```
3. If count >= 10, return 409 Conflict with error details
4. Otherwise, proceed with file upload

**Error Response:**
```json
{
  "error": "FileLimitExceeded",
  "message": "This trip has reached the maximum limit of 10 files. Please delete an existing file to upload a new one.",
  "currentFileCount": 10,
  "maxFileCount": 10,
  "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### AI Place Search
**Endpoint:** `POST /api/places/search`
**Logic:**
1. Validate input parameters (location, keyword)
2. Call Google Places API with location and keyword
3. Receive list of places (typically 20 results)
4. Prepare prompt for OpenRouter.ai:
   ```
   You are a travel assistant. Filter and rank the following places based on the user's preferences: {preferences}
   
   Places: {json_list_of_places}
   
   Return the top 5 places that best match the preferences, with a brief recommendation for each.
   ```
5. Send prompt to OpenRouter.ai
6. Parse AI response and extract top 5 places
7. For each place, check if it exists in database by GooglePlaceId
   - If exists: use existing Place record
   - If not exists: create new Place record with IsManuallyAdded = false
8. Log event to AuditLog:
   - EventType: "PlaceSearch"
   - UserId: from JWT
   - Details: location, keyword, preferences, result count
9. Return structured list of 5 places with AI recommendations

**Note:** Places are saved to the database for future reference but not yet associated with any day/trip.

#### Place Addition to Day
**Endpoint:** `POST /api/days/{dayId}/places`
**Logic:**

**Option A: Add existing place**
```json
{
  "placeId": "8fa85f64-5717-4562-b3fc-2c963f66afab",
  "order": 1
}
```
1. Verify Place exists
2. Verify Day exists and user owns the parent trip (RLS handles this)
3. Check if DayAttraction already exists (Day + Place combination)
4. If exists, return 409 Conflict
5. Insert into DayAttractions with Order and IsVisited = false

**Option B: Add new manual place**
```json
{
  "place": {
    "name": "Local Bakery",
    "formattedAddress": "10 Rue du Pain, Paris",
    "isManuallyAdded": true
  },
  "order": 2
}
```
1. Validate place data
2. Create new Place record with IsManuallyAdded = true
3. Insert into DayAttractions linking the new Place to the Day

#### Mark Place as Visited
**Endpoint:** `PATCH /api/days/{dayId}/places/{placeId}`
**Logic:**
```json
{
  "isVisited": true,
  "order": 2
}
```
1. Verify DayAttraction exists (Day + Place combination)
2. User owns the parent trip (RLS handles this)
3. Update IsVisited and/or Order in DayAttractions table
4. Return updated DayAttraction data

#### Account Deletion (GDPR)
**Endpoint:** `DELETE /api/users/me`
**Logic:**
1. Extract UserId from JWT
2. Query all Files associated with user's trips:
   ```sql
   SELECT f.* FROM Files f
   JOIN Trips t ON (f.TripId = t.Id OR 
                     f.TransportId IN (SELECT Id FROM Transport WHERE TripId = t.Id) OR
                     f.AccommodationId IN (SELECT Id FROM Accommodation WHERE TripId = t.Id) OR
                     f.DayId IN (SELECT Id FROM Days WHERE TripId = t.Id))
   WHERE t.UserId = @UserId
   ```
3. For each file, delete from Cloudflare R2 storage
4. If any file deletion fails, abort transaction and return 500 error
5. Delete user from database: `DELETE FROM Users WHERE Id = @UserId`
6. Database cascades deletion to:
   - UserRoles
   - Trips (which cascades to Transport, Accommodation, Days, Files)
7. AuditLog records where UserId = @UserId are updated to UserId = NULL (ON DELETE SET NULL)
8. Invalidate all user's tokens (if using token blacklist)
9. Return 204 No Content

**Important:** File deletion from R2 must succeed before database deletion to prevent orphaned files.

#### Admin Metrics Calculation
**Endpoint:** `GET /api/admin/metrics?days=60`
**Logic:**
1. Verify user has Admin role (check JWT claims)
2. Calculate date range: startDate = NOW() - {days} days, endDate = NOW()
3. Execute aggregate queries:
   ```sql
   -- Total users
   SELECT COUNT(*) FROM Users;
   
   -- Total trips
   SELECT COUNT(*) FROM Trips;
   
   -- New trips in period
   SELECT COUNT(*) FROM Trips WHERE CreatedAt >= @startDate;
   
   -- Total place searches in period
   SELECT COUNT(*) FROM AuditLog 
   WHERE EventType = 'PlaceSearch' AND EventTimestamp >= @startDate;
   
   -- Total files
   SELECT COUNT(*) FROM Files;
   
   -- Files uploaded in period
   SELECT COUNT(*) FROM Files WHERE CreatedAt >= @startDate;
   
   -- Active users (users with at least one trip)
   SELECT COUNT(DISTINCT UserId) FROM Trips;
   ```
4. Return aggregated metrics in response

#### Transport/Accommodation One-to-One Enforcement
**Endpoints:** `POST /api/trips/{tripId}/transport` and `POST /api/trips/{tripId}/accommodation`
**Logic:**
1. Verify Trip exists and user owns it (RLS handles this)
2. Check if Transport/Accommodation already exists for this trip:
   ```sql
   SELECT COUNT(*) FROM Transport WHERE TripId = @TripId;
   ```
3. If count > 0, return 409 Conflict:
   ```json
   {
     "error": "TransportAlreadyExists",
     "message": "This trip already has transport details. Use PATCH to update or DELETE to remove.",
     "tripId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
   }
   ```
4. Otherwise, insert new Transport/Accommodation record

**Alternative Approach (PUT for Upsert):**
- Consider using PUT instead of POST for idempotent upsert behavior
- `PUT /api/trips/{tripId}/transport` would create if not exists, or update if exists
- Simplifies client logic

### 4.3 Error Response Format

All error responses follow a consistent structure:

```json
{
  "error": "ErrorCode",
  "message": "Human-readable error message",
  "details": {
    "field": "Specific field that caused the error",
    "value": "The invalid value",
    "constraint": "The constraint that was violated"
  },
  "timestamp": "2025-10-12T14:30:00Z",
  "path": "/api/trips"
}
```

**Common Error Codes:**
- `ValidationError` - Input validation failed
- `Unauthorized` - Missing or invalid token
- `Forbidden` - Insufficient permissions
- `NotFound` - Resource not found
- `Conflict` - Business rule violation (e.g., limits exceeded, duplicate)
- `InternalServerError` - Unexpected server error

### 4.4 Audit Logging

Events logged to AuditLog table for metrics tracking:

| EventType | Trigger | Details |
|-----------|---------|---------|
| UserRegistered | POST /api/auth/register | User email |
| UserLogin | POST /api/auth/login | User email, timestamp |
| TripCreated | POST /api/trips | Trip name, destination |
| TripDeleted | DELETE /api/trips/{tripId} | Trip name |
| PlaceSearch | POST /api/places/search | Location, keyword, result count |
| FileUploaded | POST /api/.../files | File name, size, parent entity |
| UserDeleted | DELETE /api/users/me | User email (before deletion) |

**Implementation:**
- Log asynchronously to avoid blocking API responses
- Use background queue for non-critical events
- Set UserId from JWT (will be NULL for deleted users due to ON DELETE SET NULL)

---

## 5. API Versioning

For the MVP, the API will not implement versioning. All endpoints are under `/api/`.

**Future Consideration:**
- When breaking changes are needed, introduce versioning: `/api/v1/`, `/api/v2/`
- Use content negotiation with custom media types: `Accept: application/vnd.trivare.v2+json`
- Maintain backward compatibility for at least 6 months before deprecating old versions

---

## 6. Additional Technical Considerations

### 6.1 Response Time Targets
- Auth endpoints: < 200ms
- CRUD operations: < 300ms
- AI search: < 3 seconds (due to external API calls)
- File upload: < 5 seconds (for 5MB file)
- File preview/download: < 2 seconds (for 5MB file)

### 6.2 Error Handling for External Services
- **Google Places API failure:** Return 503 Service Unavailable with retry-after header
- **OpenRouter.ai failure:** Fall back to returning Google Places results without AI filtering
- **Cloudflare R2 failure (upload):** Return 503 Service Unavailable, log error, retry with exponential backoff
- **Cloudflare R2 failure (download/preview):** Return 503 Service Unavailable, retry up to 3 times with exponential backoff

### 6.3 Pagination Defaults
- Default page size: 10 items
- Maximum page size: 50 items (100 for admin audit logs)
- Page numbers start at 1
- Include total count in response for client-side pagination UI

### 6.4 Caching Strategy
- **No caching in MVP** as per project requirements
- Future consideration: Cache Google Places results by GooglePlaceId for 24 hours

### 6.5 API Documentation
- Generate OpenAPI/Swagger documentation from code
- Host at `/api/docs` for developer reference
- Include example requests and responses
- Document all error codes and meanings

---

## 7. Implementation Priority

### Phase 1: Core MVP (Must Have)
1. Authentication endpoints (register, login, refresh, password reset)
2. User profile endpoints (get, update, delete)
3. Trip CRUD endpoints
4. Days CRUD endpoints
5. Transport and Accommodation endpoints
6. File upload and management

### Phase 2: Advanced Features
7. Place search with AI integration
8. Place management and day attractions
9. Admin metrics endpoint
10. Admin audit logs endpoint

### Phase 3: Optimization
11. Rate limiting implementation
12. Enhanced error handling
13. Performance optimization
14. API documentation finalization

---

This REST API plan provides a complete foundation for implementing the Trivare backend API with .NET 9, ensuring all business requirements from the PRD are met while maintaining security, scalability, and maintainability.
