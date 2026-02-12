using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using MedocIntegration.Common.Logging;
using MedocIntegration.Common.Constants;
using MedocIntegration.Common.Models;
using MedocIntegration.Common.Configuration;
using MedocIntegration.Service.Services;
using MedocIntegration.Service.Endpoints;
using MedocIntegration.Service.Middleware;

// Bootstrap logger
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

    // Налаштування з appsettings.json
    var appSettings = builder.Configuration
        .GetSection(AppSettings.SectionName)
        .Get<AppSettings>();

    if (appSettings == null)
    {
        throw new InvalidOperationException("Не вдалося завантажити AppSettings з конфігурації");
    }

    // Валідація налаштувань
    SettingsValidator.ValidateAndThrow(appSettings.Api);

    if (!appSettings.Logging.IsValidLevel())
    {
        throw new InvalidOperationException($"Невалідний рівень логування: {appSettings.Logging.MinimumLevel}");
    }

    Log.Information("Налаштування завантажено: API {Url}, LogLevel {LogLevel}",
        appSettings.Api.Url,
        appSettings.Logging.MinimumLevel);

    // Serilog з рівнем логування з налаштувань
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .ConfigureFileLogging(
                applicationName: ApplicationConstants.ServiceNames.Service,
                minimumLevel: appSettings.Logging.GetLogLevel(),
                isDevelopment: context.HostingEnvironment.IsDevelopment()
            );
    });

    // Реєстрація SettingsManager
    builder.Services.AddSingleton<ISettingsManager>(sp =>
        new SettingsManager(sp.GetRequiredService<ILogger<SettingsManager>>()));

    // Додавання AppSettings до DI
    builder.Services.AddSingleton(appSettings);

    // Налаштування URL з конфігурації
    builder.WebHost.UseUrls(appSettings.Api.Url);

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

    // Глобальна обробка помилок
    app.UseGlobalExceptionHandler();

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

    Log.Information("Service configured successfully, listening on {Url}", appSettings.Api.Url);

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
