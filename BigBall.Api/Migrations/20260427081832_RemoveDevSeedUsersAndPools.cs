using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDevSeedUsersAndPools : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Removes rows inserted by the former dev DbSeeder (deterministic UUIDs).
            var joao = "11111111-1111-1111-1111-111111111111";
            var ana = "22222222-2222-2222-2222-222222222222";
            var familia = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1";
            var trampo = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2";

            migrationBuilder.Sql($"""
                DELETE FROM predictions
                WHERE pool_id IN ('{familia}'::uuid, '{trampo}'::uuid)
                   OR user_id IN ('{joao}'::uuid, '{ana}'::uuid);

                DELETE FROM pool_memberships
                WHERE pool_id IN ('{familia}'::uuid, '{trampo}'::uuid)
                   OR user_id IN ('{joao}'::uuid, '{ana}'::uuid);

                DELETE FROM pools
                WHERE id IN ('{familia}'::uuid, '{trampo}'::uuid);

                DELETE FROM profiles
                WHERE id IN ('{joao}'::uuid, '{ana}'::uuid);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data removal is not reversed.
        }
    }
}
