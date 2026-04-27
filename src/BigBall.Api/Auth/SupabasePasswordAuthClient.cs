using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace BigBall.Api.Auth;

public sealed class SupabasePasswordAuthClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly JsonSerializerOptions SignupRequestJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _http;
    private readonly SupabaseAuthOptions _options;

    public SupabasePasswordAuthClient(HttpClient http, IOptions<SupabaseAuthOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<SupabasePasswordSignInResponse?> SignInAsync(string email, string password, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenUrl)
        {
            Content = JsonContent.Create(new { email, password })
        };
        request.Headers.TryAddWithoutValidation("apikey", _options.PublishableKey);

        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SupabasePasswordSignInResponse>(JsonOptions, cancellationToken: ct);
    }

    /// <summary>
    /// Calls GoTrue <c>POST /signup</c>. On success, either returns tokens (auto-confirmed e-mail) or the created user without session (confirmation e-mail sent).
    /// </summary>
    public async Task<SupabaseSignUpResult> SignUpAsync(
        string email,
        string password,
        string? fullName,
        string? redirectTo,
        CancellationToken ct)
    {
        var body = SignupRequestBody.Create(email, password, fullName, redirectTo);
        var jsonBody = JsonSerializer.Serialize(body, SignupRequestJsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.SignupUrl)
        {
            Content = new StringContent(jsonBody, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.TryAddWithoutValidation("apikey", _options.PublishableKey);

        using var response = await _http.SendAsync(request, ct);
        var json = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            return new SupabaseSignUpResult(null, null, MapGoTrueErrorMessage(json, response.StatusCode));
        }

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (TryGetNonEmptyString(root, "access_token", out _))
        {
            var session = JsonSerializer.Deserialize<SupabasePasswordSignInResponse>(json, JsonOptions);
            if (session is null || string.IsNullOrEmpty(session.AccessToken))
            {
                return new SupabaseSignUpResult(null, null, "Resposta inválida do serviço de cadastro.");
            }

            return new SupabaseSignUpResult(session, null, null);
        }

        var user = JsonSerializer.Deserialize<SupabaseAuthUser>(json, JsonOptions);
        if (user is null || user.Id == Guid.Empty)
        {
            return new SupabaseSignUpResult(null, null, "Resposta inválida do serviço de cadastro.");
        }

        return new SupabaseSignUpResult(null, user, null);
    }

    private sealed record SignupRequestBody(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password,
        [property: JsonPropertyName("data")] Dictionary<string, string>? Data,
        [property: JsonPropertyName("redirect_to")] string? RedirectTo)
    {
        public static SignupRequestBody Create(string email, string password, string? fullName, string? redirectTo)
        {
            Dictionary<string, string>? data = null;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                data = new Dictionary<string, string> { ["full_name"] = fullName.Trim() };
            }

            return new SignupRequestBody(email, password, data, string.IsNullOrWhiteSpace(redirectTo) ? null : redirectTo.Trim());
        }
    }

    private static bool TryGetNonEmptyString(JsonElement root, string name, out string? value)
    {
        value = null;
        if (!root.TryGetProperty(name, out var prop) || prop.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = prop.GetString();
        return !string.IsNullOrEmpty(value);
    }

    private static string MapGoTrueErrorMessage(string json, System.Net.HttpStatusCode statusCode)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("msg", out var msg) && msg.ValueKind == JsonValueKind.String)
            {
                var m = msg.GetString();
                if (!string.IsNullOrEmpty(m))
                {
                    return TranslateGoTrueMessage(m);
                }
            }

            if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            {
                var m = message.GetString();
                if (!string.IsNullOrEmpty(m))
                {
                    return TranslateGoTrueMessage(m);
                }
            }

            if (root.TryGetProperty("error_description", out var ed) && ed.ValueKind == JsonValueKind.String)
            {
                var m = ed.GetString();
                if (!string.IsNullOrEmpty(m))
                {
                    return TranslateGoTrueMessage(m);
                }
            }
        }
        catch (JsonException)
        {
            // fall through
        }

        return statusCode switch
        {
            System.Net.HttpStatusCode.UnprocessableEntity => "Não foi possível concluir o cadastro.",
            System.Net.HttpStatusCode.BadRequest => "Dados de cadastro inválidos.",
            _ => "Não foi possível concluir o cadastro."
        };
    }

    private static string TranslateGoTrueMessage(string msg)
    {
        if (string.Equals(msg, "User already registered", StringComparison.OrdinalIgnoreCase))
        {
            return "Este e-mail já está cadastrado.";
        }

        if (string.Equals(msg, "Signups not allowed for this instance", StringComparison.OrdinalIgnoreCase))
        {
            return "Novos cadastros estão desativados.";
        }

        return msg;
    }
}

public sealed record SupabaseSignUpResult(
    SupabasePasswordSignInResponse? Session,
    SupabaseAuthUser? UserPendingConfirmation,
    string? ErrorMessage);

public sealed record SupabasePasswordSignInResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("user")] SupabaseAuthUser User);

public sealed record SupabaseAuthUser(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("user_metadata")] SupabaseUserMetadata? UserMetadata);

public sealed record SupabaseUserMetadata(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("full_name")] string? FullName);
