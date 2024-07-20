using FluentValidation;
using FluentValidation.Results;

namespace BuildingBlocks.Validation.Extensions;

public static class ValidatorExtensions
{
    // https://www.jerriepelser.com/blog/validation-response-aspnet-core-webapi
    public static async Task<TRequest> HandleValidationAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ToValidationResultModel().Message);

        return request;
    }

    public static TRequest HandleValidation<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.ToValidationResultModel().Message);

        return request;
    }

    private static ValidationResultModel ToValidationResultModel(this ValidationResult? validationResult)
    {
        return new ValidationResultModel(validationResult);
    }
}
