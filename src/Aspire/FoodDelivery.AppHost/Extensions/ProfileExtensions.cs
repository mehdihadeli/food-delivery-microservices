namespace FoodDelivery.AppHost.Extensions;

public static class ProfileExtensions
{
    public static string? GetLaunchProfileName(this IDistributedApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"];
    }

    public static bool IsHttpLaunchProfile(this IDistributedApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"]
            == BuildingBlocks.Core.Constants.ProfileConstants.HttpProfile;
    }

    public static bool IsHttpsLaunchProfile(this IDistributedApplicationBuilder builder)
    {
        return builder.Configuration["DOTNET_LAUNCH_PROFILE"]
            == BuildingBlocks.Core.Constants.ProfileConstants.HttpsProfile;
    }
}
