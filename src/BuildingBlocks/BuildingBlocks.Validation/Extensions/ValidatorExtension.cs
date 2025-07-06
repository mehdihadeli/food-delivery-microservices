using FluentValidation;
using Shared.Validation;
using ValidationException = BuildingBlocks.Core.Exception.ValidationException;

namespace BuildingBlocks.Validation.Extensions;

public static class ValidatorExtension
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
        {
            throw new ValidationException(new ValidationResultModel<TRequest>(validationResult).Message);
        }

        return request;
    }

    public static TRequest HandleValidation<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(new ValidationResultModel<TRequest>(validationResult).Message);
        }

        return request;
    }
}
