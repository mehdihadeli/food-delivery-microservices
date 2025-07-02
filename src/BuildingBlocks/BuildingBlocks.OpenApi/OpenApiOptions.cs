namespace BuildingBlocks.OpenApi;

public class OpenApiOptions
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? AuthorName { get; set; }
    public Uri? AuthorUrl { get; set; }
    public string? AuthorEmail { get; set; }

    public string LicenseName { get; set; } = "MIT";
    public Uri LicenseUrl { get; set; } = new("https://opensource.org/licenses/MIT");
    public SecurityUIMode SecurityUIMode { get; set; } = SecurityUIMode.Oauth2;
    public static bool IsOpenApiBuild => Environment.GetEnvironmentVariable("OpenApiBuild") == "true";
}

public enum SecurityUIMode
{
    ApiKey,
    Jwt,
    Oauth2,
}
