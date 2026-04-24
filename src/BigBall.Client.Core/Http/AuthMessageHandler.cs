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
        var token = await _tokens.GetTokenAsync(cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
