using Microsoft.AspNetCore.Authorization;

namespace BuildingBlocks.Security.ApiKey.Authorization;

public class OnlyAdminsRequirement : IAuthorizationRequirement
{
}
