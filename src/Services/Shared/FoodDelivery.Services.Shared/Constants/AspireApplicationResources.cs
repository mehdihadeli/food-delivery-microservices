using Humanizer;

namespace FoodDelivery.Services.Shared.Constants;

public static class AspireApplicationResources
{
    public static class PostgresDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "pg";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).Kebaberize()}{Postfix}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).Kebaberize()}{Postfix}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).Kebaberize()}{Postfix}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).Kebaberize()}{Postfix}";
        public static readonly string Health = $"{nameof(Health).Kebaberize()}{Postfix}";
    }

    public static class MongoDatabase
    {
        private const string Postfix = "db";
        private const string Prefix = "mongo";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).Kebaberize()}{Postfix}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).Kebaberize()}{Postfix}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).Kebaberize()}{Postfix}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).Kebaberize()}{Postfix}";
    }

    public static class RedisCache
    {
        private const string Prefix = "redis";
        public static readonly string Catalogs = $"{Prefix}-{nameof(Catalogs).Kebaberize()}";
        public static readonly string Customers = $"{Prefix}-{nameof(Customers).Kebaberize()}";
        public static readonly string Identity = $"{Prefix}-{nameof(Identity).Kebaberize()}";
        public static readonly string Orders = $"{Prefix}-{nameof(Orders).Kebaberize()}";
    }

    public static class Api
    {
        public static readonly string Catalogs = $"{nameof(Catalogs).Kebaberize()}";
        public static readonly string Customers = $"{nameof(Customers).Kebaberize()}";
        public static readonly string Identity = $"{nameof(Identity).Kebaberize()}";
        public static readonly string Orders = $"{nameof(Orders).Kebaberize()}";
        public static readonly string Gateway = $"{nameof(Gateway).Kebaberize()}";
        public static readonly string ApiBff = $"{nameof(ApiBff).Kebaberize()}";
        public static readonly string SpaBff = $"{nameof(SpaBff).Kebaberize()}";
    }

    public static class Ui
    {
        private const string Postfix = "ui";
        public static readonly string Blazor = $"{nameof(Blazor).Kebaberize()}-{Postfix}";
        public static readonly string React = $"{nameof(React).Kebaberize()}-{Postfix}";
    }
}
