using System.Net;
using System.Text.Json;
using FluentValidation.Results;

namespace BuildingBlocks.Validation;

public class ValidationResultModel
{
    public ValidationResultModel(ValidationResult? validationResult = null)
    {
        Errors = validationResult?.Errors
            .Select(error => new ValidationError(error.PropertyName, error.ErrorMessage))
            .ToList();
    }

    public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
    public string Message { get; set; } = "Validation Failed.";

    public IList<ValidationError>? Errors { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
