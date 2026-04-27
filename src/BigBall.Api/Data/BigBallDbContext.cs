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
    public DbSet<Prediction> Predictions => Set<Prediction>();

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
            entity.Property(x => x.KickoffUtc).HasColumnName("kickoff_utc");
            entity.HasIndex(x => x.ExternalKey).IsUnique();
            entity.Property(x => x.Venue).HasColumnName("venue").HasMaxLength(200);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(24);
            entity.Property(x => x.ReferenceHome).HasColumnName("reference_home");
            entity.Property(x => x.ReferenceAway).HasColumnName("reference_away");
            entity.Property(x => x.WentToPenalties).HasColumnName("went_to_penalties");
            entity.Property(x => x.PenaltyWinnerCode).HasColumnName("penalty_winner_code").HasMaxLength(8);
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
