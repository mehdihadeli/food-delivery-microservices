namespace ECommerce.Services.Identity.Identity.Dtos.v1;

public record RefreshTokenDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireAt { get; set; }
    public string CreatedByIp { get; set; } = default!;
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RevokedAt { get; set; }
}
