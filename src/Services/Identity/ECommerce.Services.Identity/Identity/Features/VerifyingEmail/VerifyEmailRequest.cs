namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail;

public record VerifyEmailRequest(string Email, string Code);
