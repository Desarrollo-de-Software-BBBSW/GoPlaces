using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoPlaces.Migrations
{
    /// <inheritdoc />
    public partial class DestinationsAndFollowList_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppDestinations");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppFollowLists",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_date",
                table: "AppFollowLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "AppDestinations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "Population",
                table: "AppDestinations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Url_Image",
                table: "AppDestinations",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated_date",
                table: "AppDestinations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_AppDestinations_Country",
                table: "AppDestinations",
                column: "Country");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppDestinations_Country",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppFollowLists");

            migrationBuilder.DropColumn(
                name: "last_updated_date",
                table: "AppFollowLists");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "Population",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "Url_Image",
                table: "AppDestinations");

            migrationBuilder.DropColumn(
                name: "last_updated_date",
                table: "AppDestinations");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppDestinations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
