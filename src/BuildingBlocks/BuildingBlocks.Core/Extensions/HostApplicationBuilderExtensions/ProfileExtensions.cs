using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions.HostApplicationBuilderExtensions;

public static class ProfileExtensions
{
    public static string? GetLaunchProfileName(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"];
    }

    public static bool IsHttpLaunchProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Constants.ProfileConstants.HttpProfile;
    }

    public static bool IsHttpsLaunchProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Constants.ProfileConstants.HttpsProfile;
    }
}
