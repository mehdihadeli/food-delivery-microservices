using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Shared.Validation;

public class ValidationResultModel<TRequest>
{
    public ValidationResultModel(ValidationResult? validationResult)
    {
        var validationError = $"Validation failed for type {typeof(TRequest).Name}";
        Errors = validationResult
            ?.Errors.Select(error => new ValidationError(error.PropertyName, error.ErrorMessage))
            .ToList();

        Message = JsonConvert.SerializeObject(new { Message = validationError, Errors = Errors });
    }

    public string Message { get; set; }

    public IList<ValidationError>? Errors { get; }
}
