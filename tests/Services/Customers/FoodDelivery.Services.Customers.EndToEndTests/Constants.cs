namespace FoodDelivery.Services.Customers.EndToEndTests;

public class Constants
{
    public static class Routes
    {
        private const string RootBaseAddress = "api/v1/customers";
        public const string Health = $"{RootBaseAddress}/healthz";

        public static class Customers
        {
            private const string CustomersBaseAddress = $"{RootBaseAddress}";

            public static string GetByPage => $"{CustomersBaseAddress}/";

            public static string GetById(Guid id) => $"{CustomersBaseAddress}/{id}";

            public static string Delete(long id) => $"{CustomersBaseAddress}/{id}";

            public static string Put(long id) => $"{CustomersBaseAddress}/{id}";

            public static string Create => $"{CustomersBaseAddress}/";
        }

        public static class RestockSubscription
        {
            private const string RestockSubscriptionBaseAddress = $"{RootBaseAddress}/restock-subscriptions";

            public static string GetByPage => $"{RestockSubscriptionBaseAddress}/";

            public static string GetById(Guid id) => $"{RestockSubscriptionBaseAddress}/{id}";

            public static string Delete(long id) => $"{RestockSubscriptionBaseAddress}/{id}";

            public static string Put(long id) => $"{RestockSubscriptionBaseAddress}/{id}";

            public static string Create => $"{RestockSubscriptionBaseAddress}/";
        }
    }
}
