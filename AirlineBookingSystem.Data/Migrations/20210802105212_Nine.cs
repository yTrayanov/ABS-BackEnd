using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlineBookingSystem.Data.Migrations
{
    public partial class Nine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassangerName",
                table: "Tickets",
                newName: "PassengerName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassengerName",
                table: "Tickets",
                newName: "PassangerName");
        }
    }
}
