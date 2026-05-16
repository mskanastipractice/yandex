using System.ComponentModel.DataAnnotations;

namespace WebAPI.Attributes;

/// <summary>
/// Атрибут валидации, проверяющий, что значение не равно значению по умолчанию для своего типа.
/// </summary>
/// <remarks>
/// Используется для DateTime, Guid, int и других value types, где default недопустим.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
internal class NotDefaultAttribute : ValidationAttribute
{
    /// <summary>
    /// Проверяет, что значение не равно default(T).
    /// </summary>
    /// <param name="value">Значение для проверки.</param>
    /// <param name="validationContext">Контекст валидации.</param>
    /// <returns>Результат валидации.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult(ErrorMessage ?? "Значение не может быть null.");
        
        var type = value.GetType();
        var defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
        
        if (value.Equals(defaultValue))
        {
            return new ValidationResult(ErrorMessage ?? $"Поле {validationContext.DisplayName} не может быть значением по умолчанию.");
        }
        
        return ValidationResult.Success;
    }
}