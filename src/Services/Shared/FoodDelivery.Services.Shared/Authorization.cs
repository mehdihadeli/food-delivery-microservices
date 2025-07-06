namespace FoodDelivery.Services.Shared;

public static class Authorization
{
    public static class ClientPermissions
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

        public static string ToScope(string permission) =>
            permission.Replace(".", ":", StringComparison.InvariantCulture);
    }

    public class Roles
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public class Policies
    {
        public const string AdminPolicy = "admin";
        public const string UserPolicy = "user";
        public const string CatalogsWritePolicy = "catalogs.write";
        public const string CatalogsReadPolicy = "catalogs.read";
    }

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

    public static class UserPermissions
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

        public static string ToScope(string permission) =>
            permission.Replace(".", ":", StringComparison.InvariantCulture);
    }
}
