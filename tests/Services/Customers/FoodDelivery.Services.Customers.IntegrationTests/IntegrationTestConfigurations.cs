using BuildingBlocks.Core.Web;
using Tests.Shared;

namespace FoodDelivery.Services.Customers.IntegrationTests;

public class IntegrationTestConfigurations : TestConfigurations
{
    public IntegrationTestConfigurations()
    {
        this["ASPNETCORE_ENVIRONMENT"] = Environments.Test;
    }
}
