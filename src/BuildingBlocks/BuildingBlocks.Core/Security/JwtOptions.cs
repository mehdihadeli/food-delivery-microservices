namespace BuildingBlocks.Core.Security;

public class JwtOptions
{
    public string? Algorithm { get; set; }
    public string? Authority { get; set; }
    public string? Issuer { get; set; }
    public string SecretKey { get; set; } = null!;
    public string? Audience { get; set; }
    public double TokenLifeTimeSecond { get; set; } = 300;
    public bool CheckRevokedAccessTokens { get; set; }
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);
    public IList<string> ValidAudiences { get; set; } = new List<string>();
    public IList<string> ValidIssuers { get; set; } = new List<string>();
}
