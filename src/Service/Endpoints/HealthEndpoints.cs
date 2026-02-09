using Microsoft.AspNetCore.OpenApi;
using Service.Services;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/health");

        group.MapGet("/", (IHealthService healthService) =>
        {
            var result = healthService.GetHealthStatus();
            return Results.Ok(result);
        })
        .AddOpenApiOperationTransformer((operation, context, ct) => 
        {
            operation.Summary = "Health check";
            operation.Description = "Перевірка стану служби";
            return Task.CompletedTask;
        });

        return group;
    }
}
