using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class MatchProviderSyncFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "last_provider_status_code",
                table: "matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_external_match_id",
                table: "matches",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "provider_last_synced_utc",
                table: "matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "provider_daily_api_usage",
                columns: table => new
                {
                    day_utc = table.Column<DateOnly>(type: "date", nullable: false),
                    http_get_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provider_daily_api_usage", x => x.day_utc);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "provider_daily_api_usage");

            migrationBuilder.DropColumn(
                name: "last_provider_status_code",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "provider_external_match_id",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "provider_last_synced_utc",
                table: "matches");
        }
    }
}
