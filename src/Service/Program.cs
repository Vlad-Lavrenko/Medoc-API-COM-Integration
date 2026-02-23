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

// Bootstrap logger - file only, no console (Windows Service has no console)
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

    // === MUST BE FIRST: tells ASP.NET Core this is a Windows Service ===
    // This must come before UseSerilog, UseUrls or any other host configuration
    builder.Host.UseWindowsService();

    // Read defaults from appsettings.json
    var appSettingsDefaults = builder.Configuration
        .GetSection(AppSettings.SectionName)
        .Get<AppSettings>() ?? AppSettings.Default;

    // Ensure settings file exists in ProgramData and get actual settings
    var bootstrapSettingsManager = new SettingsManager(
        Microsoft.Extensions.Logging.Abstractions.NullLogger<SettingsManager>.Instance);

    AppSettings appSettings;
    try
    {
        appSettings = await bootstrapSettingsManager
            .EnsureCreatedAsync(appSettingsDefaults);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to load settings, using defaults");
        appSettings = appSettingsDefaults;
    }

    // Validate settings
    try
    {
        SettingsValidator.ValidateAndThrow(appSettings.Api);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Settings validation failed, using defaults");
        appSettings = appSettingsDefaults;
    }

    if (!appSettings.Logging.IsValidLevel())
    {
        Log.Warning("Invalid log level '{Level}', falling back to Information",
            appSettings.Logging.MinimumLevel);
        appSettings = appSettingsDefaults;
    }

    Log.Information("Settings loaded: API {Url}, LogLevel {LogLevel}",
        appSettings.Api.Url,
        appSettings.Logging.MinimumLevel);

    // Serilog - after UseWindowsService
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

    // Register SettingsManager
    builder.Services.AddSingleton<ISettingsManager>(sp =>
        new SettingsManager(sp.GetRequiredService<ILogger<SettingsManager>>()));

    // Register AppSettings
    builder.Services.AddSingleton(appSettings);

    // Configure URL - after UseWindowsService
    builder.WebHost.UseUrls(appSettings.Api.Url);

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

    // Application services
    builder.Services.AddSingleton<IHealthService, HealthService>();
    builder.Services.AddSingleton<IInfoService, InfoService>();

    var app = builder.Build();

    app.UseGlobalExceptionHandler();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000}ms";
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
    });

    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/openapi/{documentName}.json";
    });

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
