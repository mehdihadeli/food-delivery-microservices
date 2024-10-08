using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Security;

public class UnauthorizedProblemDetails : ProblemDetails
{
    public UnauthorizedProblemDetails(string? details = null)
    {
        Title = "UnauthorizedException";
        Detail = details;
        Status = 401;
    }
}
