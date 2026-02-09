using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Hosting.WindowsServices;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Windows Service
builder.Host.UseWindowsService();

// Тільки OpenAPI (Scalar його використовує)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Налаштування Scalar
builder.Services.Configure<ScalarOptions>(options =>
{
    options.Title = "Prototype Service API";
    options.Theme = ScalarTheme.Purple;
});

var app = builder.Build();

// Scalar завжди доступний (або тільки в Development)
app.MapScalarApiReference();  // ✅ /scalar – головний UI!

// Твої endpoints
app.MapHealthEndpoints();
app.MapInfoEndpoints();

app.Run();
