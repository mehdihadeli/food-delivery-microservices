namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail;

public class VerifyEmailRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}
