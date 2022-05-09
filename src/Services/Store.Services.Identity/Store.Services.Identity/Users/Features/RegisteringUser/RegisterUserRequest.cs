namespace Store.Services.Identity.Users.Features.RegisteringUser;

public record RegisterUserRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Password,
    string ConfirmPassword)
{
    public IEnumerable<string> Roles { get; init; } = new List<string> { Constants.Role.User };
}
