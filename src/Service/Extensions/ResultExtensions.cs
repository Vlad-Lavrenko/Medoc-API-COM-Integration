using MedocIntegration.Common.Models;

namespace MedocIntegration.Service.Extensions;

/// <summary>
/// Extension методи для зручного створення API відповідей
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Повертає успішну відповідь з даними
    /// </summary>
    public static IResult OkApiResponse(object? data = null)
    {
        return Results.Ok(ApiResponse.Success(data));
    }

    /// <summary>
    /// Повертає успішну типізовану відповідь
    /// </summary>
    public static IResult OkApiResponse<T>(T? data)
    {
        return Results.Ok(ApiResponse<T>.Success(data));
    }

    /// <summary>
    /// Повертає відповідь з помилкою (400 Bad Request)
    /// </summary>
    public static IResult BadRequestApiResponse(string errorMessage)
    {
        return Results.BadRequest(ApiResponse.Error(errorMessage));
    }

    /// <summary>
    /// Повертає відповідь з помилкою "не знайдено" (404 Not Found)
    /// </summary>
    public static IResult NotFoundApiResponse(string errorMessage)
    {
        return Results.NotFound(ApiResponse.Error(errorMessage));
    }

    /// <summary>
    /// Повертає відповідь з внутрішньою помилкою сервера (500 Internal Server Error)
    /// </summary>
    public static IResult InternalServerErrorApiResponse(string errorMessage)
    {
        return Results.Json(
            ApiResponse.Error(errorMessage),
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
}
