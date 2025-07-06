namespace FoodDelivery.Services.Identity.Api.Pages.Account.Register;

using Microsoft.AspNetCore.Mvc.Rendering;

public class RegisterViewModel
{
    public List<SelectListItem> AvailableRoles { get; set; } = new();
    public List<SelectListItem> AvailablePermissions { get; set; } = new();
}
