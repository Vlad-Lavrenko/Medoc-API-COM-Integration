using Microsoft.AspNetCore.OpenApi;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/health").WithOpenApi();
        group.MapGet("/", () => Results.Ok(new { status = "OK", timestamp = DateTime.UtcNow }));
        return group;
    }
}
