namespace ECommerce.Services.Identity.Identity.Features.Login;

public record LoginUserRequest(string UserNameOrEmail, string Password, bool Remember);
