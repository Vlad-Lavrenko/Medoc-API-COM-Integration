using Microsoft.AspNetCore.OpenApi;
using MedocIntegration.Service.Services;
using MedocIntegration.Service.Extensions;

namespace MedocIntegration.Service.Endpoints;

public static class HealthEndpoints
{
    public static RouteGroupBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/health");

        group.MapGet("/", (IHealthService healthService) =>
        {
            try
            {
                var result = healthService.GetHealthStatus();
                return ResultExtensions.OkApiResponse(result);
            }
            catch (Exception ex)
            {
                return ResultExtensions.InternalServerErrorApiResponse(ex.Message);
            }
        })
        .AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            operation.Summary = "Health check";
            operation.Description = "Перевірка стану служби. Повертає статус 'ok' та дані про здоров'я системи.";
            return Task.CompletedTask;
        });

        return group;
    }
}
