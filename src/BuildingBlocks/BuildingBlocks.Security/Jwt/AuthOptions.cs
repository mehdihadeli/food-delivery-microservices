namespace BuildingBlocks.Security.Jwt;

public class JwtOptions
{
    public bool AuthenticationDisabled { get; set; }
    public IEnumerable<string>? AllowAnonymousEndpoints { get; set; }
    public CertificateOptions? Certificate { get; set; }
    public string? Algorithm { get; set; }
    public string? Issuer { get; set; }
    public string? SecretKey { get; set; }
    public string? Authority { get; set; }
    public string? Audience { get; set; }
    public string Challenge { get; set; } = "Bearer";
    public string? MetadataAddress { get; set; }
    public bool SaveToken { get; set; } = true;
    public bool SaveSigninToken { get; set; }
    public bool RequireAudience { get; set; } = true;
    public bool RequireHttpsMetadata { get; set; } = true;
    public bool RequireExpirationTime { get; set; } = true;
    public bool RequireSignedTokens { get; set; } = true;
    public double ExpiryMinutes { get; set; }
    public string? ValidAudience { get; set; }
    public IEnumerable<string>? ValidAudiences { get; set; }
    public string? ValidIssuer { get; set; }
    public IEnumerable<string>? ValidIssuers { get; set; }
    public bool ValidateActor { get; set; }
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateTokenReplay { get; set; }
    public bool ValidateIssuerSigningKey { get; set; }
    public bool RefreshOnIssuerKeyNotFound { get; set; } = true;
    public int RefreshTokenExpiryInDays { get; set; } = 7;
    public bool IncludeErrorDetails { get; set; } = true;
    public string? AuthenticationType { get; set; }
    public string? NameClaimType { get; set; }
    public string? RoleClaimType { get; set; }
    public long RefreshTokenTTL { get; set; }

    public class CertificateOptions
    {
        public string? Location { get; set; }
        public string? RawData { get; set; }
        public string? Password { get; set; }
    }
}
