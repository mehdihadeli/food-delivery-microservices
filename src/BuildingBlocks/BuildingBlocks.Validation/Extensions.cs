using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Scrutor;

namespace BuildingBlocks.Validation;

public static class Extensions
{
    private static ValidationResultModel ToValidationResultModel(this ValidationResult? validationResult)
    {
        return new ValidationResultModel(validationResult);
    }

    // https://www.jerriepelser.com/blog/validation-response-aspnet-core-webapi
    public static async Task HandleValidationAsync<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.ToValidationResultModel());
    }

    public static void HandleValidation<TRequest>(
        this IValidator<TRequest> validator,
        TRequest request)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.ToValidationResultModel());
    }

    public static IServiceCollection AddCustomValidators(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        // https://docs.fluentvalidation.net/en/latest/di.html
        // I have some problem with registering IQuery validators with this
        // services.AddValidatorsFromAssembly(assembly);
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsImplementedInterfaces()
            .WithLifetime(serviceLifetime));

        return services;
    }
}