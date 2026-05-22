using BigBall.Api.Integrations.SportsApiPro;

namespace BigBall.Api.Endpoints;

public static class AdminSportsEndpoints
{
    public static IEndpointRouteBuilder MapAdminSportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/debug").RequireAuthorization().WithTags("Admin");

        group.MapPost("/world-cup-2026-sports-api-probe", async (
                SportsApiProWorldCup2026ProbeService probe,
                CancellationToken ct) =>
            {
                var dto = await probe.RunAsync(ct).ConfigureAwait(false);
                return dto.Success
                    ? Results.Json(dto, statusCode: StatusCodes.Status200OK)
                    : Results.Json(dto, statusCode: StatusCodes.Status422UnprocessableEntity);
            })
            .WithName("WorldCup2026SportsApiProbe");

        return app;
    }
}
