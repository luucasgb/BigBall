using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "teams",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    badge_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    badge_url_small = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    country_image_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    flashscore_team_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    flashscore_team_url = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    last_updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_source = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.code);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "teams");
        }
    }
}
