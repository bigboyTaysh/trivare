using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixFilesPolymorphicConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Files_PolymorphicLink",
                table: "Files");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Files_PolymorphicLink",
                table: "Files",
                sql: "(CASE WHEN TransportId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN AccommodationId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN DayId IS NOT NULL THEN 1 ELSE 0 END) <= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Files_PolymorphicLink",
                table: "Files");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Files_PolymorphicLink",
                table: "Files",
                sql: "(CASE WHEN TripId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN TransportId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN AccommodationId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN DayId IS NOT NULL THEN 1 ELSE 0 END) = 1");
        }
    }
}
