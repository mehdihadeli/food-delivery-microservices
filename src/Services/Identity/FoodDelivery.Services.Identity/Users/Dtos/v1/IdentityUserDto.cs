using FoodDelivery.Services.Shared.Identity.Users;

namespace FoodDelivery.Services.Identity.Users.Dtos.v1;

public class IdentityUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public DateTime? LastLoggedInAt { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
    public IList<string> Permissions { get; set; } = new List<string>();
    public UserState UserState { get; set; }
    public DateTime CreatedAt { get; set; }
}
