using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TourPlanner.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTourColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "Tours",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "To",
                table: "Tours",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransportType",
                table: "Tours",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EstimatedTime",
                table: "Tours",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0));

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "Tours",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "RouteImagePath",
                table: "Tours",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "From",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "To",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "RouteImagePath",
                table: "Tours");
        }
    }
}
