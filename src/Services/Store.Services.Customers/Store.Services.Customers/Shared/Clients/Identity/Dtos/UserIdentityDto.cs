namespace Store.Services.Customers.Shared.Clients.Identity.Dtos;

public class UserIdentityDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
