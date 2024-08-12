using FluentValidation;
using FluentValidation.Results;
using ValidationException = BuildingBlocks.Core.Exception.Types.ValidationException;

namespace BuildingBlocks.Validation.Extensions;

public static class ValidatorExtensions
{
    private static ValidationResultModel ToValidationResultModel(this ValidationResult? validationResult)
    {
        return new ValidationResultModel(validationResult);
    }

    // https://www.jerriepelser.com/blog/validation-response-aspnet-core-webapi
    public static async Task HandleValidationAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ToValidationResultModel().Message);
    }

    public static void HandleValidation<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ToValidationResultModel().Message);
    }
}
