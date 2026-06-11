using BigBall.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigBall.Api.Data;

public sealed class BigBallDbContext : DbContext
{
    public BigBallDbContext(DbContextOptions<BigBallDbContext> options) : base(options) { }

    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Pool> Pools => Set<Pool>();
    public DbSet<PoolMembership> PoolMemberships => Set<PoolMembership>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<HostCity> HostCities => Set<HostCity>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<ProviderDailyApiUsage> ProviderDailyApiUsages => Set<ProviderDailyApiUsage>();
    public DbSet<Team> Teams => Set<Team>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Profile>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(120);
            entity.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(512);
            entity.Property(x => x.IsPlatformAdmin).HasColumnName("is_platform_admin");
            entity.Property(x => x.CreateDate).HasColumnName("create_date");
            entity.Property(x => x.IsInactive).HasColumnName("is_inactive").HasDefaultValue(false);
            entity.Property(x => x.DeactivationDate).HasColumnName("deactivation_date");
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Pool>(entity =>
        {
            entity.ToTable("pools");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(140);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
            entity.Property(x => x.Visibility).HasColumnName("visibility").HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.InviteCode).HasColumnName("invite_code").HasMaxLength(64);
            entity.Property(x => x.AdminUserId).HasColumnName("admin_user_id");
            entity.Property(x => x.PrizeDescription).HasColumnName("prize_description").HasMaxLength(300);
            entity.Property(x => x.EntryCost).HasColumnName("entry_cost").HasMaxLength(120);
            entity.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            entity.HasIndex(x => x.InviteCode).IsUnique();
        });

        modelBuilder.Entity<HostCity>(entity =>
        {
            entity.ToTable("host_cities");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            entity.Property(x => x.CityName).HasColumnName("city_name").HasMaxLength(80);
            entity.Property(x => x.Country).HasColumnName("country").HasMaxLength(64);
            entity.Property(x => x.VenueName).HasColumnName("venue_name").HasMaxLength(120);
            entity.Property(x => x.RegionCluster).HasColumnName("region_cluster").HasMaxLength(32);
            entity.Property(x => x.AirportCode).HasColumnName("airport_code").HasMaxLength(8);
        });

        modelBuilder.Entity<PoolMembership>(entity =>
        {
            entity.ToTable("pool_memberships");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PoolId).HasColumnName("pool_id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.JoinedUtc).HasColumnName("joined_utc");
            entity.HasIndex(x => new { x.PoolId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.ToTable("matches");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Phase).HasColumnName("phase").HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.GroupLabel).HasColumnName("group_label").HasMaxLength(64);
            entity.Property(x => x.HomeCode).HasColumnName("home_code").HasMaxLength(32);
            entity.Property(x => x.AwayCode).HasColumnName("away_code").HasMaxLength(32);
            entity.Property(x => x.ExternalKey).HasColumnName("external_key").HasMaxLength(64);
            entity.Property(x => x.ProviderExternalMatchId).HasColumnName("provider_external_match_id").HasMaxLength(32);
            entity.Property(x => x.LastProviderStatusCode).HasColumnName("last_provider_status_code");
            entity.Property(x => x.LastLifecyclePhase).HasColumnName("last_lifecycle_phase").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.ProviderLastSyncedUtc).HasColumnName("provider_last_synced_utc");
            entity.Property(x => x.KickoffUtc).HasColumnName("kickoff_utc");
            entity.HasIndex(x => x.ExternalKey).IsUnique();
            entity.Property(x => x.Venue).HasColumnName("venue").HasMaxLength(200);
            entity.Property(x => x.HostCityId).HasColumnName("host_city_id");
            entity.HasIndex(x => x.HostCityId);
            entity.HasOne(x => x.HostCity)
                .WithMany(x => x.Matches)
                .HasForeignKey(x => x.HostCityId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ReferenceHome).HasColumnName("reference_home");
            entity.Property(x => x.ReferenceAway).HasColumnName("reference_away");
            entity.Property(x => x.WentToPenalties).HasColumnName("went_to_penalties");
            entity.Property(x => x.PenaltyWinnerCode).HasColumnName("penalty_winner_code").HasMaxLength(8);
        });

        modelBuilder.Entity<ProviderDailyApiUsage>(entity =>
        {
            entity.ToTable("provider_daily_api_usage");
            entity.HasKey(x => x.DayUtc);
            entity.Property(x => x.DayUtc).HasColumnName("day_utc");
            entity.Property(x => x.HttpGetCount).HasColumnName("http_get_count").IsRequired();
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(x => x.Code);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(8);
            entity.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(120).IsRequired();
            entity.Property(x => x.BadgeUrl).HasColumnName("badge_url").HasMaxLength(512);
            entity.Property(x => x.BadgeUrlSmall).HasColumnName("badge_url_small").HasMaxLength(512);
            entity.Property(x => x.CountryImageUrl).HasColumnName("country_image_url").HasMaxLength(512);
            entity.Property(x => x.FlashScoreTeamId).HasColumnName("flashscore_team_id").HasMaxLength(32);
            entity.Property(x => x.FlashScoreTeamUrl).HasColumnName("flashscore_team_url").HasMaxLength(64);
            entity.Property(x => x.LastUpdatedUtc).HasColumnName("last_updated_utc");
            entity.Property(x => x.LastSource).HasColumnName("last_source").HasMaxLength(24);
        });

        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.ToTable("predictions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id");
            entity.Property(x => x.PoolId).HasColumnName("pool_id");
            entity.Property(x => x.MatchId).HasColumnName("match_id");
            entity.Property(x => x.Home).HasColumnName("home");
            entity.Property(x => x.Away).HasColumnName("away");
            entity.Property(x => x.PenaltyWinnerCode).HasColumnName("penalty_winner_code").HasMaxLength(8);
            entity.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            entity.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            entity.HasIndex(x => new { x.UserId, x.PoolId, x.MatchId }).IsUnique();
        });
    }
}
