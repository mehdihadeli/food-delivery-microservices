using System.ComponentModel.DataAnnotations;

namespace FoodDelivery.Services.Identity.Api.Pages.Account.Register;

// Use string? in Input Models — Even for Required Fields, We use string? (nullable) for FirstName and LastName,... in the InputModel when: - We're receiving HTTP input (e.g., MVC/Web API). - Validation is handled separately (e.g., via a validator or in a command).
// - Why Nullable? - Model binding in ASP.NET Core does not reject null values by default — users can send nulls or missing fields. - If we declare string Username { get; set; }, the compiler assumes it's always non-null — but model binding can set it to null, resulting in runtime exceptions or warnings. - Using string? makes the contract honest: this field can be null until validated and  after validation, assign to a non-nullable property.
public class InputModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(
        100,
        ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 8
    )]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    public IEnumerable<string>? Permissions { get; set; }

    [Required(ErrorMessage = "At least one role is required")]
    public IEnumerable<string> Roles { get; set; } = default!;
}
