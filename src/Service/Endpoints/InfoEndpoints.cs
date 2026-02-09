using Microsoft.AspNetCore.OpenApi;

public static class InfoEndpoints
{
    public static RouteGroupBuilder MapInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/info")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Service info";
                operation.Description = "Інформація про службу";
                return operation;
            });

        group.MapGet("/", () => Results.Ok(new
        {
            service = "Prototype.Service",
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        }));

        return group;
    }
}
