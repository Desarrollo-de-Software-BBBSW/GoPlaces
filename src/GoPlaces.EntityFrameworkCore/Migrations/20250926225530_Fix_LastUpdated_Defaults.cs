using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoPlaces.Migrations
{
    /// <inheritdoc />
    public partial class Fix_LastUpdated_Defaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_date",
                table: "AppFollowLists",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_date",
                table: "AppDestinations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_date",
                table: "AppFollowLists",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AlterColumn<DateTime>(
                name: "last_updated_date",
                table: "AppDestinations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
            // Backfill existentes (solo si quedaron en 0001-01-01 o NULL)
            migrationBuilder.Sql(@"
                UPDATE fl
                SET fl.last_updated_date = ISNULL(fl.LastModificationTime, fl.CreationTime)
                FROM AppFollowLists fl
                WHERE fl.last_updated_date = '0001-01-01T00:00:00'
                   OR fl.last_updated_date IS NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE d
                SET d.last_updated_date = ISNULL(d.LastModificationTime, d.CreationTime)
                FROM AppDestinations d
                WHERE d.last_updated_date = '0001-01-01T00:00:00'
                   OR d.last_updated_date IS NULL;
            ");
        }
    }
}
