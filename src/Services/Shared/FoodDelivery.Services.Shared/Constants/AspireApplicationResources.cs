using Humanizer;

namespace FoodDelivery.Services.Shared.Constants;

public static class AspireApplicationResources
{
    public static class Database
    {
        private const string Postfix = "db";
        public static readonly string Catalogs = $"{nameof(Catalogs).Underscore()}{Postfix}";
        public static readonly string Customers = $"{nameof(Customers).Underscore()}{Postfix}";
        public static readonly string Identity = $"{nameof(Identity).Underscore()}{Postfix}";
        public static readonly string Orders = $"{nameof(Orders).Underscore()}{Postfix}";
    }

    public static class Api
    {
        private const string Postfix = "-api";
        public static readonly string Catalogs = $"{nameof(Catalogs).Kebaberize()}{Postfix}";
        public static readonly string Customers = $"{nameof(Customers).Kebaberize()}{Postfix}";
        public static readonly string Identity = $"{nameof(Identity).Kebaberize()}{Postfix}";
        public static readonly string Orders = $"{nameof(Orders).Kebaberize()}{Postfix}";
    }
}
