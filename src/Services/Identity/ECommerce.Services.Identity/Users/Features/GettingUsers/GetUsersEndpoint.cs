using Ardalis.ApiEndpoints;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Queries;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;

namespace ECommerce.Services.Identity.Users.Features.GettingUsers;

// https://www.youtube.com/watch?v=SDu0MA6TmuM
// https://github.com/ardalis/ApiEndpoints
public class GetUsersEndpoint : EndpointBaseAsync
    .WithRequest<GetUsersRequest?>
    .WithActionResult<GetUsersResult>
{
    private readonly IQueryProcessor _queryProcessor;

    public GetUsersEndpoint(IQueryProcessor queryProcessor)
    {
        _queryProcessor = queryProcessor;
    }

    [HttpGet(UsersConfigs.UsersPrefixUri, Name = "GetUsers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users",
        OperationId = "GetUsers",
        Tags = new[] { UsersConfigs.Tag })]
    public override async Task<ActionResult<GetUsersResult>> HandleAsync(
        [FromQuery] GetUsersRequest? request,
        CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(request, nameof(request));

        var result = await _queryProcessor.SendAsync(
            new GetUsers
            {
                Filters = request.Filters,
                Includes = request.Includes,
                Page = request.Page,
                Sorts = request.Sorts,
                PageSize = request.PageSize
            },
            cancellationToken);

        return Ok(result);
    }
}
