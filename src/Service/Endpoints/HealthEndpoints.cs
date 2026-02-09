using Microsoft.AspNetCore.OpenApi;
using Service.Services;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/health")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Health check";
                operation.Description = "Перевірка стану служби";
                return operation;
            });

        group.MapGet("/", (IHealthService healthService) =>
        {
            var result = healthService.GetHealthStatus();
            return Results.Ok(result);
        });

        return group;
    }
}
