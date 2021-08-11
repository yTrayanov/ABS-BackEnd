using Microsoft.EntityFrameworkCore.Migrations;

namespace AirlineBookingSystem.Data.Migrations
{
    public partial class ChangedSeatPropName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassangerName",
                table: "Seats",
                newName: "PassengerName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PassengerName",
                table: "Seats",
                newName: "PassangerName");
        }
    }
}
