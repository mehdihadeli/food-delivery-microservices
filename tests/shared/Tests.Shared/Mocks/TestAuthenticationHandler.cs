using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests.Shared.Mocks;

//https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-5.0#mock-authentication
//https://www.vaughanreid.com/2020/07/asp-net-core-integration-tests-with-webapplicationfactory
//https://blog.joaograssi.com/posts/2021/asp-net-core-testing-permission-protected-api-endpoints
//https://timdeschryver.dev/blog/how-to-test-your-csharp-web-api#authenticationhandler
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly MockAuthUser _mockAuthUser;

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        MockAuthUser mockAuthUser) : base(options, logger, encoder, clock)
    {
        _mockAuthUser = mockAuthUser;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (_mockAuthUser.Claims.Count == 0)
            return Task.FromResult(AuthenticateResult.Fail("Mock auth user not configured."));

        // create the identity and authenticate the request
        var identity = new ClaimsIdentity(_mockAuthUser.Claims, Constants.AuthConstants.Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Constants.AuthConstants.Scheme);

        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
