using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.ApiKey.Authorization;

public class OnlyAdminsAuthorizationHandler : AuthorizationHandler<OnlyAdminsRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OnlyAdminsRequirement requirement)
    {
        if (context.User.IsInRole(Roles.Admin)) context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
