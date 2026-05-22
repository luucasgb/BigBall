using BigBall.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BigBall.Api.Data;

public static class HostCitiesSeeder
{
    public static async Task SeedAsync(
        BigBallDbContext db,
        IWebHostEnvironment? env = null,
        ILogger? logger = null,
        CancellationToken ct = default)
    {
        var path = FindCsvPath(env);
        if (path is null)
        {
            logger?.LogWarning("host_cities.csv not found; skipping host city seed.");
            return;
        }

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream);
        if (await reader.ReadLineAsync(ct).ConfigureAwait(false) is null)
        {
            return;
        }

        while (await reader.ReadLineAsync(ct).ConfigureAwait(false) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 6)
            {
                continue;
            }

            if (!int.TryParse(parts[0], out var id))
            {
                continue;
            }

            var cityName = parts[1].Trim();
            var country = parts[2].Trim();
            var venueName = parts[3].Trim();
            var region = parts[4].Trim();
            var airport = parts[5].Trim();

            var existing = await db.HostCities.FindAsync(new object[] { id }, ct).ConfigureAwait(false);
            if (existing is not null)
            {
                existing.CityName = cityName;
                existing.Country = country;
                existing.VenueName = venueName;
                existing.RegionCluster = region;
                existing.AirportCode = airport;
            }
            else
            {
                db.HostCities.Add(new HostCity
                {
                    Id = id,
                    CityName = cityName,
                    Country = country,
                    VenueName = venueName,
                    RegionCluster = region,
                    AirportCode = airport
                });
            }
        }

        await db.SaveChangesAsync(ct).ConfigureAwait(false);
        logger?.LogInformation("Host cities seed completed from {Path}.", path);
    }

    private static string? FindCsvPath(IWebHostEnvironment? env)
    {
        var inOutput = Path.Combine(AppContext.BaseDirectory, "Data", "Fixtures", "host_cities.csv");
        if (File.Exists(inOutput))
        {
            return inOutput;
        }

        if (env is { ContentRootPath: { } content })
        {
            var underContent = Path.Combine(content, "Data", "Fixtures", "host_cities.csv");
            if (File.Exists(underContent))
            {
                return underContent;
            }
        }

        return null;
    }
}
