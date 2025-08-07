namespace BuildingBlocks.Core.Security;

public class OAuthOptions
{
    public string CookieName { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string? ResponseType { get; set; }
    public string? ResponseMode { get; set; }
    public bool GetClaimsFromUserInfoEndpoint { get; set; }
    public bool MapInboundClaims { get; set; }
    public bool SaveTokens { get; set; } = true;
    public IList<string> Scopes { get; set; } = new List<string>();
    public IList<string> OpenApiScopes { get; set; } = new List<string>();
    public string Authority { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int SessionCookieLifetimeMinutes { get; set; } = 60;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    public IList<string> ValidAudiences { get; set; } = new List<string>();
    public IList<string> ValidIssuers { get; set; } = new List<string>();
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
}
