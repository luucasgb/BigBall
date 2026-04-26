using System.Net.Http.Headers;
using BigBall.Client.Core.Abstractions;

namespace BigBall.Client.Core.Http;

/// <summary>
/// Injects <c>Authorization: Bearer {jwt}</c> on every request when a token is present.
/// STUB — ready to swap for a Supabase-aware handler per TechSpec §4.3.
/// </summary>
public sealed class AuthMessageHandler : DelegatingHandler
{
    private readonly ITokenStore _tokens;

    public AuthMessageHandler(ITokenStore tokens)
    {
        _tokens = tokens;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string? token = null;
        if (ShouldAttachBearer(request))
        {
            token = await _tokens.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _tokens.ClearAsync(cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    /// <summary>
    /// Anonymous auth endpoints must not receive a stale Bearer token — some hosts reject invalid JWTs before the route runs.
    /// </summary>
    private static bool ShouldAttachBearer(HttpRequestMessage request)
    {
        var path = request.RequestUri?.AbsolutePath.TrimEnd('/') ?? "";
        return !string.Equals(path, "/api/auth/login", StringComparison.OrdinalIgnoreCase);
    }
}
