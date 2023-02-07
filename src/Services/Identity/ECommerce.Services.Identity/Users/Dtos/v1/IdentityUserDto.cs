using ECommerce.Services.Identity.Shared.Models;

namespace ECommerce.Services.Identity.Users.Dtos.v1;

public class IdentityUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? LastLoggedInAt { get; set; }
    public IEnumerable<string>? RefreshTokens { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public UserState UserState { get; set; }
    public DateTime CreatedAt { get; set; }
}
