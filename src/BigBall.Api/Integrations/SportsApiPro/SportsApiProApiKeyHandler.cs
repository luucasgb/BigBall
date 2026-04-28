using BigBall.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BigBall.Api.Integrations.SportsApiPro;

/// <summary>Injects <c>x-api-key</c> from <see cref="SportsApiProOptions.ApiKey"/> on every outbound request.</summary>
public sealed class SportsApiProApiKeyHandler(IOptions<SportsApiProOptions> options) : DelegatingHandler
{
    private readonly SportsApiProOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Remove("x-api-key");
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation("x-api-key", _options.ApiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
