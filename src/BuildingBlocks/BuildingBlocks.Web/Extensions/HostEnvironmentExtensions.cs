using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Web.Extensions;

public static class HostEnvironmentExtensions
{
    public static bool IsTest(this IHostEnvironment env) => env.IsEnvironment("test");

    public static bool IsDocker(this IHostEnvironment env) => env.IsEnvironment("docker");
}
