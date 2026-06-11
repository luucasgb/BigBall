using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLastLifecyclePhaseToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_lifecycle_phase",
                table: "matches",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Unknown");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_lifecycle_phase",
                table: "matches");
        }
    }
}
