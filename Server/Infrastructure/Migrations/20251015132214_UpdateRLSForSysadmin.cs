using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Updates the Row-Level Security predicates to allow SQL Server sysadmins to bypass RLS filtering.
    /// This enables database administrators to query all data directly without needing to set SESSION_CONTEXT.
    /// </summary>
    public partial class UpdateRLSForSysadmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop existing security policies
            migrationBuilder.Sql("DROP SECURITY POLICY Security.trip_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.transport_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.accommodation_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.day_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.file_policy;");

            // Update the predicate function to include sysadmin check
            migrationBuilder.Sql(@"
                ALTER FUNCTION Security.fn_rls_predicate(@UserId UNIQUEIDENTIFIER)
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
                    )
                    -- Grant access if the current SQL Server user is a sysadmin
                    OR IS_SRVROLEMEMBER('sysadmin') = 1;
            ");

            // Update the trip join predicate function
            migrationBuilder.Sql(@"
                ALTER FUNCTION Security.fn_rls_predicate_trip_join(@TripId UNIQUEIDENTIFIER)
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
                    -- Grant access if the current SQL Server user is a sysadmin
                    OR IS_SRVROLEMEMBER('sysadmin') = 1
                );
            ");

            // Recreate the security policies
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.trip_policy ADD FILTER PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips, ADD BLOCK PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips AFTER INSERT;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.transport_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Transport;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.accommodation_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Accommodations;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.day_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Days;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.file_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Files;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop existing security policies
            migrationBuilder.Sql("DROP SECURITY POLICY Security.trip_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.transport_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.accommodation_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.day_policy;");
            migrationBuilder.Sql("DROP SECURITY POLICY Security.file_policy;");

            // Revert the predicate functions to their original form (without sysadmin check)
            migrationBuilder.Sql(@"
                ALTER FUNCTION Security.fn_rls_predicate(@UserId UNIQUEIDENTIFIER)
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

            migrationBuilder.Sql(@"
                ALTER FUNCTION Security.fn_rls_predicate_trip_join(@TripId UNIQUEIDENTIFIER)
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

            // Recreate the security policies
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.trip_policy ADD FILTER PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips, ADD BLOCK PREDICATE Security.fn_rls_predicate(UserId) ON dbo.Trips AFTER INSERT;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.transport_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Transport;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.accommodation_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Accommodations;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.day_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Days;");
            migrationBuilder.Sql("CREATE SECURITY POLICY Security.file_policy ADD FILTER PREDICATE Security.fn_rls_predicate_trip_join(TripId) ON dbo.Files;");
        }
    }
}
