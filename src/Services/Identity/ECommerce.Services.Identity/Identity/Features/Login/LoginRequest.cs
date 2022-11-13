namespace ECommerce.Services.Identity.Identity.Features.Login;

public record LoginRequest(string UserNameOrEmail, string Password, bool Remember);
