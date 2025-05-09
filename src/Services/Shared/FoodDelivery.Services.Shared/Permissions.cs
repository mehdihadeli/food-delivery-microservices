namespace FoodDelivery.Services.Shared;

public static class Permissions
{
    public const string GatewayAccess = "gateway";

    // Catalog service
    public const string CatalogsRead = "catalogs.read";
    public const string CatalogsWrite = "catalogs.write";
    public const string CatalogsFull = "catalogs.full";

    // Order service
    public const string OrderRead = "orders.read";
    public const string OrderWrite = "orders.write";
    public const string OrderFull = "orders.full";

    // Customer service
    public const string CustomerRead = "customers.read";
    public const string CustomerWrite = "customers.write";
    public const string CustomerFull = "customers.full";

    // User service
    public const string UserRead = "users.read";
    public const string UserWrite = "users.write";
    public const string UserFull = "users.full";

    public static string ToScope(string permission) => permission.Replace(".", ":", StringComparison.InvariantCulture);
}
