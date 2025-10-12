# Trivare Azure SQL Database Schema

This document outlines the database schema for the Trivare project, designed for Azure SQL.

## 1. Tables

### Users and Roles

**`Roles`** - Stores user roles (e.g., 'Admin', 'User').
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `Name` | `NVARCHAR(50)` | `NOT NULL, UNIQUE` |

**`Users`** - Stores user account information.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `Email` | `NVARCHAR(255)` | `NOT NULL, UNIQUE` |
| `PasswordHash` | `VARBINARY(256)` | `NOT NULL` |
| `PasswordSalt` | `VARBINARY(128)` | `NOT NULL` |
| `PasswordResetToken` | `NVARCHAR(255)` | `NULL` |
| `PasswordResetTokenExpiry` | `DATETIME2` | `NULL` |
| `CreatedAt` | `DATETIME2` | `NOT NULL DEFAULT GETUTCDATE()` |

**`UserRoles`** - Links users to roles.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `UserId` | `UNIQUEIDENTIFIER` | `PRIMARY KEY, FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE` |
| `RoleId` | `UNIQUEIDENTIFIER` | `PRIMARY KEY, FOREIGN KEY REFERENCES Roles(Id) ON DELETE CASCADE` |

---

### Trip Planning

**`Trips`** - Stores core trip information.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `UserId` | `UNIQUEIDENTIFIER` | `NOT NULL, FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE` |
| `Name` | `NVARCHAR(255)` | `NOT NULL` |
| `Destination` | `NVARCHAR(255)` | `NULL` |
| `StartDate` | `DATE` | `NOT NULL` |
| `EndDate` | `DATE` | `NOT NULL` |
| `Notes` | `NVARCHAR(2000)` | `NULL` |
| `CreatedAt` | `DATETIME2` | `NOT NULL DEFAULT GETUTCDATE()` |

**`Transport`** - Stores trip transportation details.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `TripId` | `UNIQUEIDENTIFIER` | `NOT NULL, UNIQUE, FOREIGN KEY REFERENCES Trips(Id) ON DELETE CASCADE` |
| `Type` | `NVARCHAR(100)` | `NULL` |
| `DepartureLocation` | `NVARCHAR(255)` | `NULL` |
| `ArrivalLocation` | `NVARCHAR(255)` | `NULL` |
| `DepartureTime` | `DATETIME2` | `NULL` |
| `ArrivalTime` | `DATETIME2` | `NULL` |
| `Notes` | `NVARCHAR(2000)` | `NULL` |

**`Accommodation`** - Stores trip accommodation details.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `TripId` | `UNIQUEIDENTIFIER` | `NOT NULL, UNIQUE, FOREIGN KEY REFERENCES Trips(Id) ON DELETE CASCADE` |
| `Name` | `NVARCHAR(255)` | `NULL` |
| `Address` | `NVARCHAR(500)` | `NULL` |
| `CheckInDate` | `DATETIME2` | `NULL` |
| `CheckOutDate` | `DATETIME2` | `NULL` |
| `Notes` | `NVARCHAR(2000)` | `NULL` |

**`Days`** - Represents individual days within a trip.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `TripId` | `UNIQUEIDENTIFIER` | `NOT NULL, FOREIGN KEY REFERENCES Trips(Id) ON DELETE CASCADE` |
| `Date` | `DATE` | `NOT NULL` |
| `Notes` | `NVARCHAR(2000)` | `NULL` |

---

### Places and Attractions

**`Places`** - A central repository for attractions and points of interest.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `GooglePlaceId` | `NVARCHAR(255)` | `NULL, UNIQUE` |
| `Name` | `NVARCHAR(255)` | `NOT NULL` |
| `FormattedAddress` | `NVARCHAR(500)` | `NULL` |
| `Website` | `NVARCHAR(500)` | `NULL` |
| `GoogleMapsLink` | `NVARCHAR(500)` | `NULL` |
| `OpeningHoursText` | `NVARCHAR(1000)` | `NULL` |
| `IsManuallyAdded` | `BIT` | `NOT NULL DEFAULT 0` |

**`DayAttractions`** - Links places to specific days in a trip itinerary.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `DayId` | `UNIQUEIDENTIFIER` | `PRIMARY KEY, FOREIGN KEY REFERENCES Days(Id) ON DELETE CASCADE` |
| `PlaceId` | `UNIQUEIDENTIFIER` | `PRIMARY KEY, FOREIGN KEY REFERENCES Places(Id) ON DELETE CASCADE` |
| `Order` | `INT` | `NOT NULL` |
| `IsVisited` | `BIT` | `NOT NULL DEFAULT 0` |

---

### File Storage and Auditing

**`Files`** - Stores metadata for user-uploaded files.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `UNIQUEIDENTIFIER` | `PRIMARY KEY DEFAULT NEWSEQUENTIALID()` |
| `FileName` | `NVARCHAR(255)` | `NOT NULL` |
| `FilePath` | `NVARCHAR(1024)` | `NOT NULL` |
| `FileSize` | `BIGINT` | `NOT NULL` |
| `FileType` | `NVARCHAR(50)` | `NOT NULL` |
| `CreatedAt` | `DATETIME2` | `NOT NULL DEFAULT GETUTCDATE()` |
| `TripId` | `UNIQUEIDENTIFIER` | `NULL, FOREIGN KEY REFERENCES Trips(Id) ON DELETE NO ACTION` |
| `TransportId` | `UNIQUEIDENTIFIER` | `NULL, FOREIGN KEY REFERENCES Transport(Id) ON DELETE NO ACTION` |
| `AccommodationId` | `UNIQUEIDENTIFIER` | `NULL, FOREIGN KEY REFERENCES Accommodation(Id) ON DELETE NO ACTION` |
| `DayId` | `UNIQUEIDENTIFIER` | `NULL, FOREIGN KEY REFERENCES Days(Id) ON DELETE NO ACTION` |
| `PolymorphicLinkConstraint` | `CHECK` | `( (CASE WHEN TripId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN TransportId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN AccommodationId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN DayId IS NOT NULL THEN 1 ELSE 0 END) ) = 1` |

**`AuditLog`** - Logs key events for metrics and monitoring.
| Column | Data Type | Constraints |
| --- | --- | --- |
| `Id` | `BIGINT` | `PRIMARY KEY IDENTITY(1,1)` |
| `UserId` | `UNIQUEIDENTIFIER` | `NULL, FOREIGN KEY REFERENCES Users(Id) ON DELETE SET NULL` |
| `EventType` | `NVARCHAR(100)` | `NOT NULL` |
| `EventTimestamp` | `DATETIME2` | `NOT NULL DEFAULT GETUTCDATE()` |
| `Details` | `NVARCHAR(2000)` | `NULL` |

## 2. Relationships

-   **Users and Roles**: Many-to-many (`Users` <-> `UserRoles` <-> `Roles`).
-   **Users and Trips**: One-to-many (`Users` -> `Trips`). A user can have up to 10 trips (enforced by the application).
-   **Trips and Components**:
    -   One-to-one with `Transport` and `Accommodation`.
    -   One-to-many with `Days`.
-   **Days and Places**: Many-to-many (`Days` <-> `DayAttractions` <-> `Places`).
-   **Files (Polymorphic)**: A file belongs to exactly one parent (`Trip`, `Transport`, `Accommodation`, or `Day`), enforced by a `CHECK` constraint. `ON DELETE` is handled by the application to ensure files are deleted from storage before the database record is removed.
-   **Data Deletion**: `ON DELETE CASCADE` is used on most relationships stemming from `Users` and `Trips` to ensure GDPR-compliant data removal.

## 3. Indexes

-   **Primary Keys**: All primary keys are automatically clustered indexes.
-   **Foreign Keys**: Non-clustered indexes will be created on all foreign key columns to optimize join performance. This includes:
    -   `UserRoles.RoleId`
    -   `Trips.UserId`
    -   `Transport.TripId`
    -   `Accommodation.TripId`
    -   `Days.TripId`
    -   `DayAttractions.PlaceId`
    -   `Files.TripId`, `Files.TransportId`, `Files.AccommodationId`, `Files.DayId`
    -   `AuditLog.UserId`
-   **Query Optimization**: Additional non-clustered indexes will be created on:
    -   `Users(Email)`: For fast login lookups.
    -   `Places(GooglePlaceId)`: To quickly find places imported from Google API.
    -   `AuditLog(EventType, EventTimestamp)`: For efficient metric queries.

## 4. Row-Level Security (RLS)

A security policy will be implemented to ensure users can only access their own data, while admins have access to all data.

### Predicate Function
This function checks if the `UserId` in a table matches the `UserId` set in the session context or if the user is an admin.

```sql
CREATE FUNCTION Security.fn_rls_predicate(@UserId UNIQUEIDENTIFIER)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS fn_rls_predicate_result
WHERE
    -- Grant access if the row's UserId matches the session's UserId
    @UserId = CAST(SESSION_CONTEXT(N'UserId') AS UNIQUEIDENTIFIER)
    -- Grant access if the user is an admin
    OR EXISTS (
        SELECT 1
        FROM dbo.UserRoles ur
        JOIN dbo.Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = CAST(SESSION_CONTEXT(N'UserId') AS UNIQUEIDENTIFIER)
          AND r.Name = 'Admin'
    );
GO
```

### Security Policies
Policies will be applied to tables containing user-specific data.

```sql
-- Policy for Trips table
CREATE SECURITY POLICY Security.trip_policy
ADD FILTER PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips,
ADD BLOCK PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips AFTER INSERT;

-- Note: Similar policies will be created for other user-data tables like Files,
-- Transport, Accommodation, and Days by joining back to the Trips table to get the UserId.
```

## 5. Additional Notes

-   **Business Logic**: Rules such as the 10-trip-per-user limit and 10-file-per-trip limit will be enforced in the application layer to provide better user feedback.
-   **Password Management**: Password hashing and salt generation will be handled by the .NET application.
-   **Session Context**: The .NET application is responsible for setting the `UserId` in the `SESSION_CONTEXT` for each database connection to enable RLS.
-   **File Deletion**: The application must implement logic to delete files from Cloudflare R2 storage before deleting the corresponding record from the `Files` table to prevent orphaned files.
