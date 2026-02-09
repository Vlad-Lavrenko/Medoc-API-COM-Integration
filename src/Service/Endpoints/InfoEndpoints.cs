using Microsoft.AspNetCore.OpenApi;

public static class InfoEndpoints
{
    public static RouteGroupBuilder MapInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/info").WithOpenApi();
        group.MapGet("/", () => Results.Ok(new
        {
            service = "Prototype.Service",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        }));
        return group;
    }
}
