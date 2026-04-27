using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSupabaseSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phase = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    group_label = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    home_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    away_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    kickoff_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    reference_home = table.Column<int>(type: "integer", nullable: true),
                    reference_away = table.Column<int>(type: "integer", nullable: true),
                    went_to_penalties = table.Column<bool>(type: "boolean", nullable: false),
                    penalty_winner_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pool_memberships",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    joined_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pool_memberships", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pools",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    visibility = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    invite_code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    admin_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prize_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    entry_cost = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pools", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "predictions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    home = table.Column<int>(type: "integer", nullable: false),
                    away = table.Column<int>(type: "integer", nullable: false),
                    penalty_winner_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    created_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_predictions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    is_platform_admin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_pool_memberships_pool_id_user_id",
                table: "pool_memberships",
                columns: new[] { "pool_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pools_invite_code",
                table: "pools",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_predictions_user_id_pool_id_match_id",
                table: "predictions",
                columns: new[] { "user_id", "pool_id", "match_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_email",
                table: "profiles",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "pool_memberships");

            migrationBuilder.DropTable(
                name: "pools");

            migrationBuilder.DropTable(
                name: "predictions");

            migrationBuilder.DropTable(
                name: "profiles");
        }
    }
}
