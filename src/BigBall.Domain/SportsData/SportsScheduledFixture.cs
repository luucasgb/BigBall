namespace BigBall.Domain.SportsData;

/// <summary>Lightweight row for schedule discovery (IDs + kickoff + labels).</summary>
public sealed record SportsScheduledFixture(
    string ExternalMatchId,
    DateTime KickoffUtc,
    string? HomeTeamLabel,
    string? AwayTeamLabel);
