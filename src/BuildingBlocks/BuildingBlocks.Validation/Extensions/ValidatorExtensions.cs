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
    public static async Task<TRequest> HandleValidationAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.ToValidationResultModel().Errors?.FirstOrDefault()?.Message
                    ?? validationResult.ToValidationResultModel().Message
            );
        }

        return request;
    }

    public static TRequest HandleValidation<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(
                validationResult.ToValidationResultModel().Errors?.FirstOrDefault()?.Message
                    ?? validationResult.ToValidationResultModel().Message
            );
        }

        return request;
    }
}
