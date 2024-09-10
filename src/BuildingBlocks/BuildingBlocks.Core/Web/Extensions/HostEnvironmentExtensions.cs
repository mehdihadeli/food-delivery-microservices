using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Web.Extensions;

public static class HostEnvironmentExtensions
{
    public static bool IsTest(this IHostEnvironment env) => env.IsEnvironment(Environments.Test);

    public static bool IsDependencyTest(this IHostEnvironment env) => env.IsEnvironment(Environments.DependencyTest);

    public static bool IsDocker(this IHostEnvironment env) => env.IsEnvironment(Environments.Docker);
}
