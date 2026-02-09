using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Windows Service
builder.Host.UseWindowsService();

// ✅ Ключ: OpenAPI з правильними операціями
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});

// Scalar
builder.Services.Configure<ScalarOptions>(options =>
{
    options.Title = "Service API";
});

// Реєстрація сервісів для DI
builder.Services.AddSingleton<IHealthService, HealthService>();
builder.Services.AddSingleton<IInfoService, InfoService>();

var app = builder.Build();

// Генеруємо OpenAPI документ
app.MapOpenApi();  // /openapi/v1.json

// Scalar з явним шляхом до OpenAPI
app.MapScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "/openapi/{documentName}.json"; 
});

// Endpoints З OpenApi метаданими
app.MapHealthEndpoints();
app.MapInfoEndpoints();

app.Run();
