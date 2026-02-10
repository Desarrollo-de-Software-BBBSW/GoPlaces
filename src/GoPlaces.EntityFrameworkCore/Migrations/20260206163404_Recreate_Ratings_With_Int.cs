using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoPlaces.Migrations
{
    /// <inheritdoc />
    public partial class Recreate_Ratings_With_Int : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinationId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRatings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRatings_DestinationId",
                table: "AppRatings",
                column: "DestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppRatings_DestinationId_UserId",
                table: "AppRatings",
                columns: new[] { "DestinationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppRatings_UserId",
                table: "AppRatings",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRatings");
        }
    }
}
