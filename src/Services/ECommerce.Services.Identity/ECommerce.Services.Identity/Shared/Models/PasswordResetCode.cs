namespace ECommerce.Services.Identity.Shared.Models;

public class PasswordResetCode
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Code { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime? UsedAt { get; set; }
}
