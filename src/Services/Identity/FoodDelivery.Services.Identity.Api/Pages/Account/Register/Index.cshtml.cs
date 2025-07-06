using FoodDelivery.Services.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FoodDelivery.Services.Identity.Api.Pages.Account.Register;

// https://github.com/dotnet/aspnetcore/blob/main/src/Identity/UI/src/Areas/Identity/Pages/V5/Account/Register.cshtml.cs

// we want just an admin user can create a new user
[Authorize(Policy = Authorization.Roles.Admin)]
public class IndexModel(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    ILogger<IndexModel> logger,
    RoleManager<IdentityRole> roleManager
) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new InputModel();

    public RegisterViewModel ViewModel { get; set; }

    public string ReturnUrl { get; set; }

    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        ViewModel = new RegisterViewModel
        {
            AvailableRoles = roleManager
                .Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                .ToList(),

            AvailablePermissions = new List<SelectListItem>
            {
                // Gateway Access
                new("Gateway Access", Authorization.UserPermissions.GatewayAccess),
                // Catalog service
                new("Read Catalogs", Authorization.UserPermissions.CatalogsRead),
                new("Write Catalogs", Authorization.UserPermissions.CatalogsWrite),
                new("Full Catalog Access", Authorization.UserPermissions.CatalogsFull),
                // Order service
                new("Read Orders", Authorization.UserPermissions.OrderRead),
                new("Write Orders", Authorization.UserPermissions.OrderWrite),
                new("Full Order Access", Authorization.UserPermissions.OrderFull),
                // Customer service
                new("Read Customers", Authorization.UserPermissions.CustomerRead),
                new("Write Customers", Authorization.UserPermissions.CustomerWrite),
                new("Full Customer Access", Authorization.UserPermissions.CustomerFull),
                // User service
                new("Read Users", Authorization.UserPermissions.UserRead),
                new("Write Users", Authorization.UserPermissions.UserWrite),
                new("Full Access to Users", Authorization.UserPermissions.UserFull),
            },
        };
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");

        if (!ModelState.IsValid)
        {
            await OnGetAsync(returnUrl);
            return Page();
        }

        var user = new IdentityUser
        {
            UserName = Input.UserName,
            Email = Input.Email,
            PhoneNumber = Input.PhoneNumber,
        };

        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("User created a new account with password.");

            // Add custom claims
            await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("given_name", Input.FirstName));
            await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("family_name", Input.LastName));

            // Assign roles
            foreach (var role in Input.Roles)
            {
                await userManager.AddToRoleAsync(user, role);
            }

            // Assign permissions
            foreach (var permission in Input.Permissions ?? Enumerable.Empty<string>())
            {
                await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("permission", permission));
            }

            await signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        await OnGetAsync(returnUrl);
        return Page();
    }
}
