using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// This is the initial migration which sets up the entire database schema for the Trivare application.
    /// It creates all the tables, defines their columns, primary keys, foreign keys, indexes, and constraints
    /// as specified in the database plan.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create central repository for attractions and points of interest
            migrationBuilder.CreateTable(
                name: "Places",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    GooglePlaceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FormattedAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GoogleMapsLink = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OpeningHoursText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsManuallyAdded = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Places", x => x.Id);
                });

            // Create table to store user roles (e.g., 'Admin', 'User')
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            // Create table for user account information
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "varbinary(256)", maxLength: 256, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "varbinary(128)", maxLength: 128, nullable: false),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // Create table for logging key events for metrics and monitoring
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Create table for core trip information
            migrationBuilder.CreateTable(
                name: "Trips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trips_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create join table to link users to roles (many-to-many)
            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create table for trip accommodation details
            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accommodations_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create table for individual days within a trip
            migrationBuilder.CreateTable(
                name: "Days",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Days_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create table for trip transportation details
            migrationBuilder.CreateTable(
                name: "Transport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DepartureLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ArrivalLocation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DepartureTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transport_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create join table to link places to specific days in a trip itinerary
            migrationBuilder.CreateTable(
                name: "DayAttractions",
                columns: table => new
                {
                    DayId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsVisited = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayAttractions", x => new { x.DayId, x.PlaceId });
                    table.ForeignKey(
                        name: "FK_DayAttractions_Days_DayId",
                        column: x => x.DayId,
                        principalTable: "Days",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayAttractions_Places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "Places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create table for metadata of user-uploaded files
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TripId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccommodationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                    // This CHECK constraint ensures a file is linked to exactly one parent entity.
                    table.CheckConstraint("CK_Files_PolymorphicLink", "(CASE WHEN TripId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN TransportId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN AccommodationId IS NOT NULL THEN 1 ELSE 0 END) + (CASE WHEN DayId IS NOT NULL THEN 1 ELSE 0 END) = 1");
                    table.ForeignKey(
                        name: "FK_Files_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Files_Days_DayId",
                        column: x => x.DayId,
                        principalTable: "Days",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Files_Transport_TransportId",
                        column: x => x.TransportId,
                        principalTable: "Transport",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Files_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id");
                });

            // Create indexes for performance optimization
            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_TripId",
                table: "Accommodations",
                column: "TripId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EventType_EventTimestamp",
                table: "AuditLogs",
                columns: new[] { "EventType", "EventTimestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DayAttractions_PlaceId",
                table: "DayAttractions",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Days_TripId",
                table: "Days",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_AccommodationId",
                table: "Files",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_DayId",
                table: "Files",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_TransportId",
                table: "Files",
                column: "TransportId");

            migrationBuilder.CreateIndex(
                name: "IX_Files_TripId",
                table: "Files",
                column: "TripId");

            migrationBuilder.CreateIndex(
                name: "IX_Places_GooglePlaceId",
                table: "Places",
                column: "GooglePlaceId",
                unique: true,
                filter: "[GooglePlaceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transport_TripId",
                table: "Transport",
                column: "TripId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trips_UserId",
                table: "Trips",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop tables in reverse order of creation to respect foreign key constraints.
            // This is a destructive operation and will result in data loss.
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DayAttractions");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Places");

            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.DropTable(
                name: "Days");

            migrationBuilder.DropTable(
                name: "Transport");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Trips");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
