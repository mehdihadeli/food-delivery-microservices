using System.Reflection;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace BuildingBlocks.Web.Extensions;

public static class ModuleExtensions
{
    public static WebApplicationBuilder AddModulesServices(
        this WebApplicationBuilder webApplicationBuilder,
        params Assembly[] scanAssemblies)
    {
        // Assemblies are lazy loaded so using AppDomain.GetAssemblies is not reliable (it is possible to get ReflectionTypeLoadException, because some dependent type assembly are lazy and not loaded yet), so we use `GetAllReferencedAssemblies` and it
        // load all referenced assemblies explicitly.
        var assemblies = scanAssemblies.Any()
            ? scanAssemblies
            : ReflectionUtilities.GetReferencedAssemblies(Assembly.GetCallingAssembly())
                .Concat(ReflectionUtilities.GetApplicationPartAssemblies(Assembly.GetCallingAssembly()))
                .Distinct()
                .ToArray();

        var modulesConfiguration = assemblies.SelectMany(x => x.GetLoadableTypes())
            .Where(t =>
                t!.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
                && t.GetConstructor(Type.EmptyTypes) != null
                && typeof(IModuleConfiguration).IsAssignableFrom(t)).ToList();

        var sharedModulesConfiguration = assemblies.SelectMany(x => x.GetLoadableTypes())
            .Where(t =>
                t!.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
                && t.GetConstructor(Type.EmptyTypes) != null
                && typeof(ISharedModulesConfiguration).IsAssignableFrom(t)).ToList();

        foreach (var sharedModule in sharedModulesConfiguration)
        {
            AddModulesDependencyInjection(webApplicationBuilder, sharedModule);
        }

        foreach (var module in modulesConfiguration)
        {
            AddModulesDependencyInjection(webApplicationBuilder, module);
        }

        return webApplicationBuilder;
    }

    private static void AddModulesDependencyInjection(
        WebApplicationBuilder webApplicationBuilder,
        Type module)
    {
        if (module.IsAssignableTo(typeof(IModuleConfiguration)))
        {
            var instantiatedType = (IModuleConfiguration)Activator.CreateInstance(module)!;
            instantiatedType.AddModuleServices(webApplicationBuilder);
            webApplicationBuilder.Services.AddSingleton(instantiatedType);
        }

        if (module.IsAssignableTo(typeof(ISharedModulesConfiguration)))
        {
            var instantiatedType = (ISharedModulesConfiguration)Activator.CreateInstance(module)!;
            instantiatedType.AddSharedModuleServices(webApplicationBuilder);
            webApplicationBuilder.Services.AddSingleton(instantiatedType);
        }
    }

    public static async Task<WebApplication> ConfigureModules(this WebApplication app)
    {
        var moduleConfigurations = app.Services.GetServices<IModuleConfiguration>();
        var sharedModulesConfigurations = app.Services.GetServices<ISharedModulesConfiguration>();

        foreach (var sharedModule in sharedModulesConfigurations)
        {
            await sharedModule.ConfigureSharedModule(app);
        }

        foreach (var module in moduleConfigurations)
        {
            await module.ConfigureModule(app);
        }

        return app;
    }

    public static IEndpointRouteBuilder MapModulesEndpoints(this IEndpointRouteBuilder builder)
    {
        var modules = builder.ServiceProvider.GetServices<IModuleConfiguration>();
        var sharedModules = builder.ServiceProvider.GetServices<ISharedModulesConfiguration>();

        foreach (var module in sharedModules)
        {
            module.MapSharedModuleEndpoints(builder);
        }

        foreach (var module in modules)
        {
            module.MapEndpoints(builder);
        }

        return builder;
    }
}
