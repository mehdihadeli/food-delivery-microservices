using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions;

public static class HostApplicationBuilderExtensions
{
    public static string? GetProfileName(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"];
    }

    public static bool IsHttpProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Constants.ProfileConstants.HttpProfile;
    }

    public static bool IsHttpsProfile(this IHostApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"] == Constants.ProfileConstants.HttpsProfile;
    }
}
