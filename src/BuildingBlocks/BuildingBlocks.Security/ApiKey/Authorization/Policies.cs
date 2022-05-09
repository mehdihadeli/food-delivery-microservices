namespace BuildingBlocks.Security.ApiKey.Authorization;

public static class Policies
{
    public const string OnlyCustomers = nameof(OnlyCustomers);
    public const string OnlyAdmins = nameof(OnlyAdmins);
    public const string OnlyThirdParties = nameof(OnlyThirdParties);
}
