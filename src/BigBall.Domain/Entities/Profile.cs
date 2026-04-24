namespace BigBall.Domain.Entities;

public sealed class Profile
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsPlatformAdmin { get; set; }
}
