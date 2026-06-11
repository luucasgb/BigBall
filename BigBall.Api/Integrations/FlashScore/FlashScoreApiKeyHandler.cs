using BigBall.Api.Configuration;
using Microsoft.Extensions.Options;

namespace BigBall.Api.Integrations.FlashScore;

/// <summary>Injects RapidAPI auth headers (<c>X-RapidAPI-Key</c>, <c>X-RapidAPI-Host</c>) on every outbound request.</summary>
public sealed class FlashScoreApiKeyHandler(IOptions<FlashScoreOptions> options) : DelegatingHandler
{
    private readonly FlashScoreOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Remove("X-RapidAPI-Key");
        request.Headers.Remove("X-RapidAPI-Host");

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            request.Headers.TryAddWithoutValidation("X-RapidAPI-Key", _options.ApiKey);

        if (!string.IsNullOrWhiteSpace(_options.RapidApiHost))
            request.Headers.TryAddWithoutValidation("X-RapidAPI-Host", _options.RapidApiHost);

        return base.SendAsync(request, cancellationToken);
    }
}
