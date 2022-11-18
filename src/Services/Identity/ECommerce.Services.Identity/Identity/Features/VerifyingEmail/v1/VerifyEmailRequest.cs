namespace ECommerce.Services.Identity.Identity.Features.VerifyingEmail.v1;

public record VerifyEmailRequest(string Email, string Code);
