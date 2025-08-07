namespace FoodDelivery.Services.Shared.Constants;

public static class AspireApplicationResources
{
    public static class PostgresDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "pg";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).ToLowerInvariant()}{Postfix}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).ToLowerInvariant()}{Postfix}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).ToLowerInvariant()}{Postfix}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).ToLowerInvariant()}{Postfix}";
        public static readonly string Health = $"{nameof(Health).ToLowerInvariant()}{Postfix}";
    }

    public static class MongoDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "mongo";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).ToLowerInvariant()}{Postfix}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).ToLowerInvariant()}{Postfix}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).ToLowerInvariant()}{Postfix}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).ToLowerInvariant()}{Postfix}";
    }

    public static class RedisCache
    {
        private const string Prefix = "redis";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).ToLowerInvariant()}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).ToLowerInvariant()}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).ToLowerInvariant()}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).ToLowerInvariant()}";
    }

    public static class Api
    {
        public static readonly string Catalogs = $"{nameof(Catalogs).ToLowerInvariant()}";
        public static readonly string Customers = $"{nameof(Customers).ToLowerInvariant()}";
        public static readonly string Identity = $"{nameof(Identity).ToLowerInvariant()}";
        public static readonly string Orders = $"{nameof(Orders).ToLowerInvariant()}";
        public static readonly string Gateway = $"{nameof(Gateway).ToLowerInvariant()}";
        public static readonly string ApiBff = $"{nameof(ApiBff).ToLowerInvariant()}";
        public static readonly string SpaBff = $"{nameof(SpaBff).ToLowerInvariant()}";
    }

    public static class Ui
    {
        private const string Postfix = "ui";
        public static readonly string Blazor = $"{nameof(Blazor).ToLowerInvariant()}-{Postfix}";
        public static readonly string React = $"{nameof(React).ToLowerInvariant()}-{Postfix}";
    }
}
