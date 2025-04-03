using System.Reflection;
using FluentValidation;
using Scrutor;

namespace BuildingBlocks.Validation.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCustomValidators(this IServiceCollection services, Assembly assembly)
    {
        // TODO: problem with registering internal validators
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );

        return services;
    }
}
