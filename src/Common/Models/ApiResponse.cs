namespace MedocIntegration.Common.Models;

/// <summary>
/// Стандартна обгортка для всіх API відповідей
/// </summary>
public record ApiResponse
{
    /// <summary>
    /// Статус виконання: "ok" або "error"
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Повідомлення про помилку (порожнє якщо успіх)
    /// </summary>
    public string ErrorMsg { get; init; } = string.Empty;

    /// <summary>
    /// Дані відповіді (null якщо помилка)
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Створює успішну відповідь
    /// </summary>
    public static ApiResponse Success(object? data = null) => new()
    {
        Result = "ok",
        Data = data
    };

    /// <summary>
    /// Створює відповідь з помилкою
    /// </summary>
    public static ApiResponse Error(string errorMessage) => new()
    {
        Result = "error",
        ErrorMsg = errorMessage
    };
}

/// <summary>
/// Generic версія для типізованих відповідей
/// </summary>
public record ApiResponse<T>
{
    /// <summary>
    /// Статус виконання: "ok" або "error"
    /// </summary>
    public required string Result { get; init; }

    /// <summary>
    /// Повідомлення про помилку (порожнє якщо успіх)
    /// </summary>
    public string ErrorMsg { get; init; } = string.Empty;

    /// <summary>
    /// Типізовані дані відповіді
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Створює успішну відповідь
    /// </summary>
    public static ApiResponse<T> Success(T? data) => new()
    {
        Result = "ok",
        Data = data
    };

    /// <summary>
    /// Створює відповідь з помилкою
    /// </summary>
    public static ApiResponse<T> Error(string errorMessage) => new()
    {
        Result = "error",
        ErrorMsg = errorMessage
    };

    /// <summary>
    /// Конвертує у нетипізовану відповідь
    /// </summary>
    public ApiResponse ToApiResponse() => new()
    {
        Result = Result,
        ErrorMsg = ErrorMsg,
        Data = Data
    };
}
