namespace BigBall.Api.Auth;

public sealed class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";

    public string ProjectUrl { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = "authenticated";

    public string AuthIssuer => $"{ProjectUrl.TrimEnd('/')}/auth/v1";
    public string JwksUrl => $"{AuthIssuer}/.well-known/jwks.json";
    public string TokenUrl => $"{AuthIssuer}/token?grant_type=password";
    public string SignupUrl => $"{AuthIssuer}/signup";
}
