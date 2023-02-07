namespace Tests.Shared;

public static class Constants
{
    public const string LoginApi = "https://localhost:7001/api/v1/identity/login";

    public static class Users
    {
        public static class Admin
        {
            public const string UserId = "4073f0f0-855a-48e6-9168-d4e20f1d2839";
            public const string UserName = "mehdi";
            public const string Email = "mehdi@test.com";
            public const string Password = "123456";
            public const string Role = "admin";
        }

        public static class NormalUser
        {
            public const string UserId = "5073f0f0-855a-48e6-9168-d4e20f1d2840";
            public const string UserName = "mehdi2";
            public const string Email = "mehdi2@test.com";
            public const string Password = "123456";
            public const string Role = "user";
        }
    }

    public class AuthConstants
    {
        public const string Scheme = "TestAuth";
    }
}
