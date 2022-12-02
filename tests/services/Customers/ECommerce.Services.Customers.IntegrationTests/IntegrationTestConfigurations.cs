using Tests.Shared;
using Tests.Shared.Fixtures;

namespace ECommerce.Services.Customers.IntegrationTests;

public class IntegrationTestConfigurations : TestConfigurations
{
    public IntegrationTestConfigurations()
    {
        this["ASPNETCORE_ENVIRONMENT"] = "test";
    }
}
