using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoPlaces.Migrations
{
    /// <inheritdoc />
    public partial class Added_Rating_To_Experience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rating",
                table: "AppExperiences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "AppExperiences");
        }
    }
}
