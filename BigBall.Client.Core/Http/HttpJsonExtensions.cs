using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BigBall.Client.Core.Http;

internal static class HttpJsonExtensions
{
    internal static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    internal static async Task<T> ReadRequiredAsync<T>(this HttpResponseMessage response, CancellationToken ct)
    {
        response.EnsureSuccessStatusCode();
        var value = await response.Content.ReadFromJsonAsync<T>(Json, ct).ConfigureAwait(false);
        return value ?? throw new InvalidOperationException($"API returned null for {typeof(T).Name}.");
    }

    internal static async Task<string?> TryReadErrorMessageAsync(this HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var dto = await response.Content.ReadFromJsonAsync<ApiErrorEnvelope>(Json, ct).ConfigureAwait(false);
            return string.IsNullOrWhiteSpace(dto?.Error) ? null : dto.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed record ApiErrorEnvelope([property: JsonPropertyName("error")] string? Error);
}
