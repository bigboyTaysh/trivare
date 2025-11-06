using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleTransportsPerTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the unique index on Transport.TripId to allow multiple transports per trip
            migrationBuilder.DropIndex(
                name: "IX_Transport_TripId",
                table: "Transport");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate the unique index on Transport.TripId to restore one-to-one relationship
            migrationBuilder.CreateIndex(
                name: "IX_Transport_TripId",
                table: "Transport",
                column: "TripId",
                unique: true);
        }
    }
}
