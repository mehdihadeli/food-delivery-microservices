namespace ECommerce.Services.Identity.Shared.Models;

public class AccessToken
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiredAt { get; set; }
    public string CreatedByIp { get; set; }
    public ApplicationUser ApplicationUser { get; set; }
}
