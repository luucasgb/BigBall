namespace BigBall.Api.Auth;

public sealed class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";

    public string ProjectUrl { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string JwtAudience { get; set; } = "authenticated";

    /// <summary>Chave service-role do Supabase, exigida pelos endpoints admin (ex.: exclusão de usuário). Lida de user secrets/variável de ambiente — nunca commitada.</summary>
    public string ServiceRoleKey { get; set; } = string.Empty;

    public string AuthIssuer => $"{ProjectUrl.TrimEnd('/')}/auth/v1";
    public string JwksUrl => $"{AuthIssuer}/.well-known/jwks.json";
    public string TokenUrl => $"{AuthIssuer}/token?grant_type=password";
    public string SignupUrl => $"{AuthIssuer}/signup";
    public string AdminUsersUrl => $"{AuthIssuer}/admin/users";
}
