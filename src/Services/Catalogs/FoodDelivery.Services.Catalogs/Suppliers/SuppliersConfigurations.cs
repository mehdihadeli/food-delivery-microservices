using FoodDelivery.Services.Catalogs.Suppliers.Contracts;
using FoodDelivery.Services.Catalogs.Suppliers.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FoodDelivery.Services.Catalogs.Suppliers;

internal static class SuppliersConfigurations
{
    internal static WebApplicationBuilder AddSuppliersModuleServices(this WebApplicationBuilder builder)
    {
        builder.Services.TryAddScoped<ISupplierChecker, SupplierChecker>();

        return builder;
    }

    internal static WebApplication UseSuppliersModule(this WebApplication app)
    {
        return app;
    }
}
