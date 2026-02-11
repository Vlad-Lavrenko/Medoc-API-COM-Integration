using Microsoft.AspNetCore.OpenApi;
using MedocIntegration.Service.Services;
using MedocIntegration.Service.Extensions;

namespace MedocIntegration.Service.Endpoints;

public static class InfoEndpoints
{
    public static RouteGroupBuilder MapInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/info");

        group.MapGet("/", (IInfoService infoService) =>
        {
            try
            {
                var result = infoService.GetServiceInfo();
                return ResultExtensions.OkApiResponse(result);
            }
            catch (Exception ex)
            {
                return ResultExtensions.InternalServerErrorApiResponse(ex.Message);
            }
        })
        .AddOpenApiOperationTransformer((operation, context, ct) =>
        {
            operation.Summary = "Service information";
            operation.Description = "Інформація про службу: версія, середовище, хост, час запуску.";
            return Task.CompletedTask;
        });

        return group;
    }
}
