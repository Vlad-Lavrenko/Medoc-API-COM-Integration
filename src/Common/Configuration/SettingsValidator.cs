using System.ComponentModel.DataAnnotations;

namespace MedocIntegration.Common.Configuration;

/// <summary>
/// Валідатор налаштувань
/// </summary>
public static class SettingsValidator
{
    public static (bool IsValid, List<string> Errors) Validate<T>(T settings) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(settings);

        bool isValid = Validator.TryValidateObject(settings, context, validationResults, true);

        var errors = validationResults.Select(r => r.ErrorMessage ?? "Unknown error").ToList();

        return (isValid, errors);
    }

    public static void ValidateAndThrow<T>(T settings) where T : class
    {
        var (isValid, errors) = Validate(settings);

        if (!isValid)
        {
            throw new ValidationException($"Налаштування не валідні: {string.Join(", ", errors)}");
        }
    }
}
