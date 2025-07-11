using System.Reflection;
using BuildingBlocks.Abstractions.Web.Problem;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Scrutor;

namespace BuildingBlocks.Web.ProblemDetail;

// https://www.strathweb.com/2022/08/problem-details-responses-everywhere-with-asp-net-core-and-net-7/
public static class DependencyInjectionExtensions
{
    public static IHostApplicationBuilder AddCustomProblemDetails(
        this IHostApplicationBuilder builder,
        Action<ProblemDetailsOptions>? configure = null,
        bool useExceptionHandler = true,
        params Assembly[] scanAssemblies
    )
    {
        var assemblies = scanAssemblies.Length != 0 ? scanAssemblies : [Assembly.GetCallingAssembly()];

        builder.Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Extensions.Add("trace-id", ctx.HttpContext.TraceIdentifier);
                ctx.ProblemDetails.Extensions.Add(
                    "instance",
                    $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}"
                );

                configure?.Invoke(options);
            }
        );
        if (useExceptionHandler)
        {
            builder.Services.AddExceptionHandler<DefaultExceptionHandler>();
        }
        else
        {
            builder.Services.AddSingleton<IProblemDetailsService, ProblemDetailsService>();
            builder.Services.AddSingleton<IProblemDetailsWriter, ProblemDetailsWriter>();
        }

        RegisterAllMappers(builder.Services, assemblies);

        return builder;
    }

    private static void RegisterAllMappers(IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.Scan(scan =>
            scan.FromAssemblies(scanAssemblies)
                .AddClasses(classes => classes.AssignableTo<IProblemDetailMapper>(), false)
                .UsingRegistrationStrategy(RegistrationStrategy.Append)
                .As<IProblemDetailMapper>()
                .WithLifetime(ServiceLifetime.Singleton)
        );
    }
}
