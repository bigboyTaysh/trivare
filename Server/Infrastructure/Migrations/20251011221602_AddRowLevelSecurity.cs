using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// This migration introduces Row-Level Security (RLS) to the database.
    /// It ensures that users can only access their own data, while users with the 'Admin' role have unrestricted access.
    /// A security schema, a predicate function, and several security policies are created to enforce these rules
    /// on user-specific tables like Trips, Transport, Accommodation, Days, and Files.
    /// </summary>
    public partial class AddRowLevelSecurity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create a dedicated schema for security-related objects to keep them organized.
            migrationBuilder.Sql("CREATE SCHEMA Security;");

            // This function is the core of the RLS implementation.
            // It returns 1 (true) if the user associated with a row matches the currently logged-in user
            // (identified by 'UserId' in the SESSION_CONTEXT) OR if the logged-in user is an Admin.
            migrationBuilder.Sql(@"
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
            ");

            // This function is for tables that don't have a direct UserId column
            // but are linked to the Trips table. It checks ownership by joining through the Trip.
            migrationBuilder.Sql(@"
                CREATE FUNCTION Security.fn_rls_predicate_trip_join(@TripId UNIQUEIDENTIFIER)
                RETURNS TABLE
                WITH SCHEMABINDING
                AS
                RETURN SELECT 1 AS fn_rls_predicate_result
                FROM dbo.Trips t
                WHERE t.Id = @TripId AND
                (
                    t.UserId = CAST(SESSION_CONTEXT(N'UserId') AS UNIQUEIDENTIFIER)
                    OR EXISTS (
                        SELECT 1
                        FROM dbo.UserRoles ur
                        JOIN dbo.Roles r ON ur.RoleId = r.Id
                        WHERE ur.UserId = CAST(SESSION_CONTEXT(N'UserId') AS UNIQUEIDENTIFIER)
                        AND r.Name = 'Admin'
                    )
                );
            ");

            // Apply security policies to user-data tables.
            // FILTER PREDICATE is for SELECT, UPDATE, DELETE operations.
            // BLOCK PREDICATE is for INSERT, preventing users from inserting data for other users.
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.trip_policy ADD FILTER PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips, ADD BLOCK PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips AFTER INSERT;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.transport_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Transport;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.accommodation_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Accommodations;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.day_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Days;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.file_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Files;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // To reverse the migration, drop all security objects in the reverse order of creation.
            // This is a non-destructive operation for the data itself but removes the security layer.
            migrationBuilder.Sql("DROP SECURITY POLICY Security.trip_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.transport_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.accommodation_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.day_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.file_policy;");
            migrationBuilder.Sql("DROP FUNCTION Security.fn_rls_predicate;");
            migrationBuilder.Sql("DROP FUNCTION Security.fn_rls_predicate_trip_join;");
            migrationBuilder.Sql("DROP SCHEMA Security;");
        }
    }
}
