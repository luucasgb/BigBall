namespace BigBall.Shared.Dtos;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string Token, ProfileDto Profile);

public sealed record ProfileDto(Guid Id, string DisplayName, string? AvatarUrl);
