using System.Net;
using System.Text.Json;
using BigBall.Client.Core.Http;
using BigBall.Shared.Dtos;

namespace BigBall.Client.Core.Tests;

public class PoolsApiClientTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CreatePoolAsync_ReturnsResponse_WhenOk_PublicPool()
    {
        var poolId = Guid.NewGuid();
        var sut = new PoolsApiClient(CreateHttpClient((req, _) =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Equal("/api/pools", req.RequestUri?.PathAndQuery);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(
                    new CreatePoolResponse(poolId, InviteCode: null))
            };
        }));

        var result = await sut.CreatePoolAsync(
            new CreatePoolRequest("B", "d", "Public", "Prêmio", null));

        Assert.Equal(poolId, result.PoolId);
        Assert.Null(result.InviteCode);
    }

    [Fact]
    public async Task CreatePoolAsync_ReturnsResponse_WhenOk_PrivatePool_WithInviteCode()
    {
        var poolId = Guid.NewGuid();
        const string code = "AB12CD34";
        var sut = new PoolsApiClient(CreateHttpClient((_, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(new CreatePoolResponse(poolId, code))
            };
        }));

        var result = await sut.CreatePoolAsync(
            new CreatePoolRequest("Priv", null, "Private", "Prêmio", "R$ 10"));

        Assert.Equal(poolId, result.PoolId);
        Assert.Equal(code, result.InviteCode);
    }

    [Fact]
    public async Task CreatePoolAsync_ThrowsWithApiErrorMessage_WhenBadRequest()
    {
        var sut = new PoolsApiClient(CreateHttpClient((_, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    """{"error":"Premiação é obrigatória."}""",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
        }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreatePoolAsync(
                new CreatePoolRequest("N", null, "Public", " ", null)));

        Assert.Equal("Premiação é obrigatória.", ex.Message);
    }

    [Fact]
    public async Task CreatePoolAsync_ThrowsWithGenericMessage_WhenServerError_WithoutErrorEnvelope()
    {
        var sut = new PoolsApiClient(CreateHttpClient((_, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreatePoolAsync(
                new CreatePoolRequest("N", null, "Public", "P", null)));

        Assert.Contains("500", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task JoinPoolByInviteAsync_ReturnsResponse_WhenOk()
    {
        var poolId = Guid.NewGuid();
        var sut = new PoolsApiClient(CreateHttpClient((req, _) =>
        {
            Assert.Equal(HttpMethod.Post, req.Method);
            Assert.Equal("/api/pools/join", req.RequestUri?.PathAndQuery);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent(new JoinPoolResponse(poolId, "Bolão X"))
            };
        }));

        var result = await sut.JoinPoolByInviteAsync(new JoinPoolRequest("ab12cd34"));

        Assert.Equal(poolId, result.PoolId);
        Assert.Equal("Bolão X", result.PoolName);
    }

    [Fact]
    public async Task JoinPoolByInviteAsync_ThrowsWithApiErrorMessage_WhenNotFound()
    {
        var sut = new PoolsApiClient(CreateHttpClient((_, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(
                    """{"error":"Código de convite inválido."}""",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
        }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.JoinPoolByInviteAsync(new JoinPoolRequest("XXXXXXXX")));

        Assert.Equal("Código de convite inválido.", ex.Message);
    }

    [Fact]
    public async Task JoinPoolByInviteAsync_ThrowsWithApiErrorMessage_WhenConflict()
    {
        var sut = new PoolsApiClient(CreateHttpClient((_, _) =>
        {
            return new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent(
                    """{"error":"Você já participa deste bolão."}""",
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
        }));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.JoinPoolByInviteAsync(new JoinPoolRequest("AB12CD34")));

        Assert.Equal("Você já participa deste bolão.", ex.Message);
    }

    private static StringContent JsonContent<T>(T value) =>
        new(JsonSerializer.Serialize(value, WebJson), System.Text.Encoding.UTF8, "application/json");

    private static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> send)
    {
        var handler = new LambdaHandler((req, ct) => send(req, ct));
        return new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
    }

    private sealed class LambdaHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _fn;

        public LambdaHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> fn) => _fn = fn;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_fn(request, cancellationToken));
    }
}
