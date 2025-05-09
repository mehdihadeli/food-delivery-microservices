namespace FoodDelivery.Services.Shared;

public static class Scopes
{
    // Standard system scopes
    public const string Roles = "roles";
    public const string UsersInfo = "info";

    public const string Gateway = "gateway";

    // Catalog service
    public const string CatalogsRead = "catalogs:read";
    public const string CatalogsWrite = "catalogs:write";
    public const string CatalogsFull = "catalogs:full";

    // Order service
    public const string OrdersRead = "orders:read";
    public const string OrdersWrite = "orders:write";
    public const string OrdersFull = "orders:full";

    // Customer service
    public const string CustomersRead = "customers:read";
    public const string CustomersWrite = "customers:write";
    public const string CustomersFull = "customers:full";

    // User service
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersFull = "users:full";

    public static string ToClaim(string scope) => scope.Replace(":", ".", StringComparison.InvariantCulture);
}
