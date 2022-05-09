namespace Store.Services.Identity.Shared.Models;

public class UserOldPassword
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string PasswordHash { get; init; }

    public DateTime SetAt { get; init; }

    public ApplicationUser User { get; init; }
}
