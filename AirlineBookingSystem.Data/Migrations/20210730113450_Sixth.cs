using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlineBookingSystem.Data.Migrations
{
    public partial class Sixth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Sections",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Airlines",
                keyColumn: "Id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "Airlines",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Airlines",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "Id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "Airports",
                keyColumn: "Id",
                keyValue: -1);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Airlines",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { -1, "DELTA" },
                    { -2, "GAMA" },
                    { -3, "GUL" }
                });

            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { -1, "NYC" },
                    { -2, "LAA" },
                    { -3, "GTO" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Authority", "Name" },
                values: new object[,]
                {
                    { -1, 10, "Admin" },
                    { -2, 1, "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "HashedPassword", "Salt", "Status", "Username" },
                values: new object[,]
                {
                    { -1, "admin@admin.bg", "admin", null, 0, "admin" },
                    { -2, "user1@user.bg", "user123", null, 0, "user1" }
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "Id", "AirlineId", "DepartureDate", "DestinationAirportId", "FlightNumber", "LandingDate", "OriginAirportId" },
                values: new object[,]
                {
                    { -1, -1, new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -1, "-123", new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -2 },
                    { -2, -2, new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -2, "-1234", new DateTime(2025, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -1 },
                    { -3, -3, new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -1, "-12345", new DateTime(2025, 7, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), -2 }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "Id", "RoleId", "UserId" },
                values: new object[,]
                {
                    { -1, -1, -1 },
                    { -2, -2, -2 }
                });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "AvailableSeatsCount", "Columns", "FlightId", "Rows", "SeatClass" },
                values: new object[] { -1, 0, 5, -1, 1, 0 });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "AvailableSeatsCount", "Columns", "FlightId", "Rows", "SeatClass" },
                values: new object[] { -2, 0, 5, -2, 1, 1 });

            migrationBuilder.InsertData(
                table: "Sections",
                columns: new[] { "Id", "AvailableSeatsCount", "Columns", "FlightId", "Rows", "SeatClass" },
                values: new object[] { -3, 0, 5, -3, 1, 2 });
        }
    }
}
