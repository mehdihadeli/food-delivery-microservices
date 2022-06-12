using System.Reflection;
using BuildingBlocks.Abstractions.Web.Module;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Web.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection AddModulesServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        params Assembly[] scanAssemblies)
    {
        var assemblies = scanAssemblies.Any() ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();

        var modules = assemblies.SelectMany(x => x.GetTypes()).Where(t =>
            t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
            && t.GetConstructor(Type.EmptyTypes) != null
            && typeof(IModuleDefinition).IsAssignableFrom(t)).ToList();

        var rootModules = modules.Where(x => x.IsAssignableTo(typeof(IRootModuleDefinition))).ToList();
        var childModules = modules.Where(x => x.IsAssignableTo(typeof(IRootModuleDefinition)) == false).ToList();

        if (rootModules.Count > 1)
        {
            throw new System.Exception(
                "Can't define more than one `IRootModuleDefinition` or RootModule in the current app domain.");
        }

        var rootModule = rootModules.SingleOrDefault();
        if (rootModule is { })
            AddModulesDependency(services, configuration, webHostEnvironment, rootModule);

        foreach (var module in childModules)
        {
            AddModulesDependency(services, configuration, webHostEnvironment, module);
        }

        return services;
    }

    public static IServiceCollection AddModulesServices(
        this WebApplicationBuilder webApplicationBuilder,
        params Assembly[] scanAssemblies)
    {
        return AddModulesServices(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            webApplicationBuilder.Environment,
            scanAssemblies);
    }

    public static IServiceCollection AddModuleServices<TModule>(this WebApplicationBuilder webApplicationBuilder)
        where TModule : class, IModuleDefinition
    {
        return AddModuleServices<TModule>(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            webApplicationBuilder.Environment);
    }

    public static IServiceCollection AddModuleServices<TModule>(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
        where TModule : class, IModuleDefinition
    {
        if (!typeof(TModule).IsAssignableTo(typeof(IModuleDefinition)))
        {
            throw new ArgumentException(
                $"{nameof(TModule)} must be implemented {nameof(IModuleDefinition)} or {nameof(IRootModuleDefinition)}");
        }

        AddModulesDependency(services, configuration, webHostEnvironment, typeof(TModule));

        return services;
    }

    private static void AddModulesDependency(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        Type module)
    {
        var instantiatedType = (IModuleDefinition)Activator.CreateInstance(module)!;
        instantiatedType.AddModuleServices(services, configuration, webHostEnvironment);

        if (instantiatedType is IRootModuleDefinition rootInstantiateType)
            services.AddSingleton(rootInstantiateType);
        else
            services.AddSingleton(instantiatedType);
    }

    public static async Task<WebApplication> ConfigureModules(this WebApplication app)
    {
        var childModules = app.Services.GetServices<IModuleDefinition>();
        var rootModule = app.Services.GetService<IRootModuleDefinition>();

        if (rootModule is { })
            await rootModule.ConfigureModule(app);

        foreach (var module in childModules)
        {
            await module.ConfigureModule(app);
        }

        return app;
    }

    public static async Task<WebApplication> ConfigureModule<TModule>(
        this WebApplication app)
        where TModule : class, IModuleDefinition
    {
        var module = app.Services.GetRequiredService<TModule>();
        await module.ConfigureModule(app);

        return app;
    }

    public static IEndpointRouteBuilder MapModulesEndpoints(
        this IEndpointRouteBuilder builder,
        params Assembly[] scanAssemblies)
    {
        var modules = builder.ServiceProvider.GetServices<IModuleDefinition>();
        var rootModule = builder.ServiceProvider.GetService<IRootModuleDefinition>();

        if (rootModule is { })
            rootModule.MapEndpoints(builder);

        foreach (var module in modules)
        {
            module.MapEndpoints(builder);
        }

        return builder;
    }

    public static IEndpointRouteBuilder MapModuleEndpoints<TModule>(this IEndpointRouteBuilder builder)
        where TModule : class, IModuleDefinition
    {
        var module = builder.ServiceProvider.GetRequiredService<TModule>();
        module.MapEndpoints(builder);

        return builder;
    }
}
