using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigBall.Api.Migrations
{
    /// <inheritdoc />
    public partial class HostCitiesAndMatchLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "host_city_id",
                table: "matches",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "host_cities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    city_name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    country = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    venue_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    region_cluster = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    airport_code = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_host_cities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matches_host_city_id",
                table: "matches",
                column: "host_city_id");

            migrationBuilder.AddForeignKey(
                name: "FK_matches_host_cities_host_city_id",
                table: "matches",
                column: "host_city_id",
                principalTable: "host_cities",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            // Seed 2026 host venues (id matches docs/host_cities.csv; MetLife = New Jersey).
            migrationBuilder.Sql("""
                INSERT INTO host_cities (id, city_name, country, venue_name, region_cluster, airport_code) VALUES
                (1, 'Atlanta', 'USA', 'Mercedes-Benz Stadium', 'East', 'ATL'),
                (2, 'Boston', 'USA', 'Gillette Stadium', 'East', 'BOS'),
                (3, 'Dallas', 'USA', 'AT&T Stadium', 'Central', 'DAL'),
                (4, 'Houston', 'USA', 'NRG Stadium', 'Central', 'IAH'),
                (5, 'Kansas City', 'USA', 'Arrowhead Stadium', 'Central', 'MCI'),
                (6, 'Los Angeles', 'USA', 'SoFi Stadium', 'West', 'LAX'),
                (7, 'Miami', 'USA', 'Hard Rock Stadium', 'East', 'MIA'),
                (8, 'New Jersey', 'USA', 'MetLife Stadium', 'East', 'EWR'),
                (9, 'Philadelphia', 'USA', 'Lincoln Financial Field', 'East', 'PHL'),
                (10, 'San Francisco Bay Area', 'USA', 'Levi''s Stadium', 'West', 'SFO'),
                (11, 'Seattle', 'USA', 'Lumen Field', 'West', 'SEA'),
                (12, 'Toronto', 'Canada', 'BMO Field', 'East', 'YYZ'),
                (13, 'Vancouver', 'Canada', 'BC Place', 'West', 'YVR'),
                (14, 'Guadalajara', 'Mexico', 'Estadio Akron', 'Central', 'GDL'),
                (15, 'Mexico City', 'Mexico', 'Estadio Azteca', 'Central', 'MEX'),
                (16, 'Monterrey', 'Mexico', 'Estadio BBVA', 'Central', 'MTY');
                """);

            migrationBuilder.Sql("""
                UPDATE matches SET host_city_id = CASE TRIM(venue)
                WHEN 'Atlanta' THEN 1
                WHEN 'Boston (Foxborough)' THEN 2
                WHEN 'Dallas (Arlington)' THEN 3
                WHEN 'Houston' THEN 4
                WHEN 'Kansas City' THEN 5
                WHEN 'Los Angeles (Inglewood)' THEN 6
                WHEN 'Miami (Miami Gardens)' THEN 7
                WHEN 'New York/New Jersey (East Rutherford)' THEN 8
                WHEN 'Philadelphia' THEN 9
                WHEN 'San Francisco Bay Area (Santa Clara)' THEN 10
                WHEN 'Seattle' THEN 11
                WHEN 'Toronto' THEN 12
                WHEN 'Vancouver' THEN 13
                WHEN 'Guadalajara (Zapopan)' THEN 14
                WHEN 'Mexico City' THEN 15
                WHEN 'Monterrey (Guadalupe)' THEN 16
                END
                WHERE venue IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE matches
                SET host_city_id = 8, venue = 'MetLife Stadium'
                WHERE host_city_id IS NULL
                  AND venue IS NOT NULL
                  AND (venue ILIKE '%metlife%'
                    OR venue ILIKE '%East Rutherford%'
                    OR TRIM(venue) = 'New York'
                    OR venue ILIKE '%New York/New Jersey%');
                """);

            migrationBuilder.Sql("""
                UPDATE matches m
                SET venue = h.venue_name
                FROM host_cities h
                WHERE m.host_city_id = h.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_matches_host_cities_host_city_id",
                table: "matches");

            migrationBuilder.DropTable(
                name: "host_cities");

            migrationBuilder.DropIndex(
                name: "IX_matches_host_city_id",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "host_city_id",
                table: "matches");
        }
    }
}
