using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTourLogColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "TourLogs",
                newName: "Comment");

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "TourLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TotalDistance",
                table: "TourLogs",
                type: "numeric(9,2)",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalTime",
                table: "TourLogs",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "TotalDistance",
                table: "TourLogs");

            migrationBuilder.DropColumn(
                name: "TotalTime",
                table: "TourLogs");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "TourLogs",
                newName: "Notes");
        }
    }
}
