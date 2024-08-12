using System.Reflection;
using FluentValidation;
using Scrutor;

namespace BuildingBlocks.Validation.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCustomValidators(this IServiceCollection services, Assembly assembly)
    {
        // https://docs.fluentvalidation.net/en/latest/di.html
        // I have some problem with registering IQuery validators with these
        // services.AddValidatorsFromAssembly(assembly);
        services.Scan(scan =>
            scan.FromAssemblies(assembly)
                .AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsImplementedInterfaces()
                .WithLifetime(ServiceLifetime.Transient)
        );

        return services;
    }
}
