using System.Net.Http.Json;
using System.Text.Json;

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
}
