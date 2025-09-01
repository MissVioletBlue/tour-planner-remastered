using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    public partial class AddTourLogVotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Votes",
                table: "TourLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Votes",
                table: "TourLogs");
        }
    }
}
