namespace Store.Services.Identity.Identity.Dtos;

public class RefreshTokenDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireAt { get; set; }
    public string CreatedByIp { get; set; }
    public bool IsExpired { get; set; }
    public bool IsRevoked { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RevokedAt { get; set; }
}
