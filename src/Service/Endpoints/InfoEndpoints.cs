using Microsoft.AspNetCore.OpenApi;
using Service.Services;

public static class InfoEndpoints
{
    public static RouteGroupBuilder MapInfoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/info");

        group.MapGet("/", (IInfoService infoService) =>
        {
            var info = infoService.GetServiceInfo();
            return Results.Ok(info);
        })
        .AddOpenApiOperationTransformer((operation, context, ct) => 
        {
            operation.Summary = "Service info";
            operation.Description = "Інформація про службу";
            return Task.CompletedTask;
        });

        return group;
    }
}
