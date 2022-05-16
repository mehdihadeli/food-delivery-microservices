namespace ECommerce.Services.Catalogs;

public static class CatalogConstants
{


    public static string IdentityRoleName => "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

    public static class Role
    {
        public const string Admin = "admin";
        public const string User = "user";
    }
}

