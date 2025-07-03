using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FoodDelivery.Services.Identity.Api.Pages.Ciba;

[AllowAnonymous]
[SecurityHeaders]
public class IndexModel : PageModel
{
    public BackchannelUserLoginRequest LoginRequest { get; set; } = default!;

    private readonly IBackchannelAuthenticationInteractionService _backchannelAuthenticationInteraction;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IBackchannelAuthenticationInteractionService backchannelAuthenticationInteractionService,
        ILogger<IndexModel> logger
    )
    {
        _backchannelAuthenticationInteraction = backchannelAuthenticationInteractionService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGet(string id)
    {
        var result = await _backchannelAuthenticationInteraction.GetLoginRequestByInternalIdAsync(id);
        if (result == null)
        {
            _logger.InvalidBackchannelLoginId(id);
            return RedirectToPage("/Home/Error/Index");
        }
        else
        {
            LoginRequest = result;
        }

        return Page();
    }
}
