using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BuildingBlocks.Validation;

public class ValidationResultModel
{
    public ValidationResultModel(ValidationResult? validationResult = null)
    {
        Errors = validationResult
            ?.Errors.Select(error => new ValidationError(error.PropertyName, error.ErrorMessage))
            .ToList();
        Message = JsonConvert.SerializeObject(Errors);
    }

    public int StatusCode { get; set; } = (int)HttpStatusCode.BadRequest;
    public string Message { get; set; }

    public IList<ValidationError>? Errors { get; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
