using MedocIntegration.Common.Constants;
using MedocIntegration.Common.Logging;
using MedocIntegration.Service.Endpoints;
using MedocIntegration.Service.Services;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

// ✅ Bootstrap logger - єдина конфігурація для файлів
Log.Logger = new LoggerConfiguration()
    .ConfigureFileLogging(
        applicationName: ApplicationConstants.ServiceNames.Service,
        minimumLevel: LogEventLevel.Information)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting {ApplicationName} - {ServiceName}",
        ApplicationConstants.ApplicationName,
        ApplicationConstants.ServiceNames.Service);

    var builder = WebApplication.CreateBuilder(args);

    // ✅ Serilog - використовуємо спільну конфігурацію
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .ConfigureFileLogging(
                applicationName: ApplicationConstants.ServiceNames.Service,
                minimumLevel: LogEventLevel.Information,
                isDevelopment: context.HostingEnvironment.IsDevelopment()
            );
    });

    // Windows Service
    builder.Host.UseWindowsService();

    // OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi(options =>
    {
        options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    });

    // Scalar
    builder.Services.Configure<ScalarOptions>(options =>
    {
        options.Title = "Medoc API";
    });

    // Реєстрація сервісів
    builder.Services.AddSingleton<IHealthService, HealthService>();
    builder.Services.AddSingleton<IInfoService, InfoService>();

    var app = builder.Build();

    // HTTP Request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
    });

    // OpenAPI
    app.MapOpenApi();

    // Scalar
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });

    // Endpoints
    app.MapHealthEndpoints();
    app.MapInfoEndpoints();

    Log.Information("Service configured successfully, listening on {BaseUrl}",
        ApplicationConstants.Api.DefaultBaseUrl);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "{ServiceName} terminated unexpectedly",
        ApplicationConstants.ServiceNames.Service);
}
finally
{
    Log.Information("{ServiceName} shutting down",
        ApplicationConstants.ServiceNames.Service);
    await Log.CloseAndFlushAsync();
}
